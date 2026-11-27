#nullable enable
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	/// <summary>
	/// Downloads and caches remote images for every platform.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>One shared <see cref="HttpClient"/>, a limit on concurrent downloads, and retries with backoff for transient failures.</item>
	/// <item>Concurrent requests for the same image share one download, which is cancelled when the last caller stops waiting.</item>
	/// <item>A memory cache and a disk cache that honor <see cref="IUriImageSource.CachingEnabled"/> and <see cref="IUriImageSource.CacheValidity"/>.
	/// Expired images are revalidated with the server (ETag / Last-Modified) and are still shown if the server cannot be reached.</item>
	/// </list>
	/// </remarks>
	internal sealed class UriImageLoader : IImageCache, IDisposable
	{
		internal const string MaxDownloadSizeAppContextKey = "Microsoft.Maui.Controls.UriImageSource.MaxResponseContentBufferSize";
		internal const int DefaultMaxDownloadSize = 50 * 1024 * 1024;
		internal const int MaxTrackedRemovals = 1024;

		static readonly TimeSpan MaxRetryAfter = TimeSpan.FromSeconds(10);
		static readonly object RetryAfterKey = typeof(UriImageLoader).FullName + ".RetryAfter";
		static readonly object StatusCodeKey = typeof(UriImageLoader).FullName + ".StatusCode";
		static UriImageLoader? s_shared;

		readonly ConcurrentDictionary<string, InFlightLoad> _inFlight = new(StringComparer.Ordinal);
		readonly Lazy<HttpClient> _httpClient;
		readonly SemaphoreSlim _downloadSlots;
		readonly ILogger? _logger;
		readonly int _maxRetryCount;
		readonly TimeSpan _retryDelay;
		readonly TimeSpan _requestTimeout;
		readonly long? _maxDownloadSize;
		readonly Random _jitter = new();
		readonly ConcurrentDictionary<string, int> _removals = new(StringComparer.Ordinal);

		// Makes "is this still the current image" and "keep it in memory" one step, relative to clearing,
		// removing and publishing newer versions.
		readonly object _memoryPublishLock = new();
		int _cacheGeneration;
		int _lastClearGeneration;
		int _disposed;

#if IOS || MACCATALYST
		readonly Foundation.NSObject? _memoryWarningObserver;
#endif

		public UriImageLoader(ImageLoadingOptions options, ILogger? logger = null)
			: this(options, logger, acceptFilesWithoutMetadata: null)
		{
		}

		/// <param name="options">The options.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="acceptFilesWithoutMetadata">Overrides whether files cached by earlier versions are recognized (for tests).</param>
		internal UriImageLoader(ImageLoadingOptions options, ILogger? logger, bool? acceptFilesWithoutMetadata)
		{
			_logger = logger;
			_maxRetryCount = Math.Max(0, options.MaxRetryCount);
			_retryDelay = options.RetryDelay < TimeSpan.Zero ? TimeSpan.Zero : options.RetryDelay;
			_requestTimeout = options.RequestTimeout <= TimeSpan.Zero ? ImageLoadingOptions.DefaultRequestTimeout : options.RequestTimeout;
			_maxDownloadSize = options.MaxDownloadSize;

			var maxConcurrentDownloads = Math.Max(1, options.MaxConcurrentDownloads);
			_downloadSlots = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);

			var handlerFactory = options.HttpMessageHandlerFactory;
			var configureHttpClient = options.ConfigureHttpClient;
			_httpClient = new Lazy<HttpClient>(() => CreateHttpClient(handlerFactory, configureHttpClient), LazyThreadSafetyMode.ExecutionAndPublication);

			MemoryCache = new UriImageMemoryCache(options.MemoryCacheSizeLimit);
			// Images cached by earlier versions (without metadata) are only recognized in the default directory.
			DiskCache = new UriImageDiskCache(
				options.CacheDirectory ?? GetDefaultCacheDirectory(),
				options.DiskCacheSizeLimit,
				() => UtcNow(),
				acceptFilesWithoutMetadata ?? options.CacheDirectory is null);
			DiskCache.BeginIndexing();

#if IOS || MACCATALYST
			try
			{
				var memoryCache = new WeakReference<UriImageMemoryCache>(MemoryCache);
				_memoryWarningObserver = UIKit.UIApplication.Notifications.ObserveDidReceiveMemoryWarning((_, _) =>
				{
					if (memoryCache.TryGetTarget(out var cache))
						cache.Clear();
				});
			}
			catch (Exception)
			{
				// Not running inside an app (for example unit tests).
			}
#endif
		}

		/// <summary>
		/// The loader used when no app-configured loader is available.
		/// </summary>
		internal static UriImageLoader Shared =>
			LazyInitializer.EnsureInitialized(ref s_shared, () => new UriImageLoader(new ImageLoadingOptions()))!;

		internal static UriImageLoader GetCurrent(IServiceProvider? services = null)
		{
			var loader = services?.GetService<UriImageLoader>();
#if !NETSTANDARD2_0
			loader ??= IPlatformApplication.Current?.Services?.GetService<UriImageLoader>();
#endif
			return loader ?? Shared;
		}

		internal static bool CanLoad(Uri? uri) =>
			uri is not null && uri.IsAbsoluteUri && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

		/// <summary>
		/// Gets the delegate the loader should use to get the image of <paramref name="imageSource"/>,
		/// or <see langword="null"/> when the loader should download the URI itself.
		/// </summary>
		/// <remarks>
		/// Custom <see cref="IUriImageSource"/> implementations that also implement <see cref="IStreamImageSource"/>
		/// keep providing their own stream, as they did before the loader existed.
		/// </remarks>
		internal static Func<CancellationToken, Task<Stream?>>? GetCustomFetch(IUriImageSource imageSource) =>
			imageSource is IStreamImageSource streamImageSource && imageSource is not ILoaderUriImageSource
				? token => streamImageSource.GetStreamAsync(token)!
				: null;

		/// <summary>
		/// Whether <paramref name="imageSource"/> can be loaded by the loader.
		/// </summary>
		internal static bool CanLoad(IUriImageSource imageSource) =>
			!imageSource.IsEmpty && (CanLoad(imageSource.Uri) || GetCustomFetch(imageSource) is not null);

		internal UriImageMemoryCache MemoryCache { get; }

		internal UriImageDiskCache DiskCache { get; }

		internal Func<DateTimeOffset> UtcNow { get; set; } = () => DateTimeOffset.UtcNow;

		internal int InFlightCount => _inFlight.Count;

		/// <summary>
		/// Gets the encoded bytes of the image at <paramref name="uri"/>, from the cache when possible.
		/// </summary>
		/// <remarks>When the image is in the memory cache and still valid, the returned task is already completed.</remarks>
		public Task<byte[]> LoadAsync(Uri uri, bool cachingEnabled, TimeSpan cacheValidity, CancellationToken cancellationToken = default) =>
			LoadAsync(uri, cachingEnabled, cacheValidity, null, cancellationToken);

		/// <param name="uri">The URI of the image; also the cache key.</param>
		/// <param name="cachingEnabled">Whether the cache may be used.</param>
		/// <param name="cacheValidity">How long a cached image is used without checking the server again.</param>
		/// <param name="fetch">When set, used to get the image instead of an HTTP GET (for custom <see cref="IUriImageSource"/> implementations that are also <see cref="IStreamImageSource"/>).</param>
		/// <param name="cancellationToken">Cancels waiting for the image.</param>
		public Task<byte[]> LoadAsync(Uri uri, bool cachingEnabled, TimeSpan cacheValidity, Func<CancellationToken, Task<Stream?>>? fetch, CancellationToken cancellationToken = default)
		{
			ValidateUri(uri, fetch);
			cancellationToken.ThrowIfCancellationRequested();
			fetch = OnCallerContext(fetch);

			if (cachingEnabled && MemoryCache.TryGet(uri.OriginalString, out var item) && IsFresh(item.ValidatedAt.UtcTicks, cacheValidity))
				return Task.FromResult(item.Data);

			return LoadBytesCoreAsync(uri, cachingEnabled, cacheValidity, fetch, cancellationToken);
		}

		/// <summary>
		/// Gets the image at <paramref name="uri"/> as a file in the disk cache when caching is enabled and the disk cache is available,
		/// otherwise as bytes. Does not use the memory cache.
		/// </summary>
		public async Task<UriImageResult> LoadFileAsync(Uri uri, bool cachingEnabled, TimeSpan cacheValidity, Func<CancellationToken, Task<Stream?>>? fetch = null, CancellationToken cancellationToken = default)
		{
			ValidateUri(uri, fetch);
			fetch = OnCallerContext(fetch);

			for (var attempt = 0; ; attempt++)
			{
				var result = await JoinAsync(uri, GetVersion(uri), cachingEnabled, cacheValidity, fetch, cancellationToken).ConfigureAwait(false);
				if (result.Entry is null || File.Exists(result.Entry.DataPath))
					return result;

				// The file was removed (for example the cache was cleared) after it was resolved.
				DiskCache.Remove(result.Entry);
				if (attempt > 0)
					throw new IOException($"Unable to read the cached image for '{uri}'.");
			}
		}

		/// <summary>
		/// Gets the cached image for <paramref name="uri"/> (even if it expired) without downloading it, or <see langword="null"/> when it is not cached.
		/// </summary>
		internal Task<byte[]?> TryLoadCachedAsync(Uri uri, CancellationToken cancellationToken = default)
		{
			if (MemoryCache.TryGet(uri.OriginalString, out var item))
				return Task.FromResult<byte[]?>(item.Data);

			return Task.Run(
				async () => DiskCache.GetEntry(uri) is { } entry
					? await DiskCache.TryReadAsync(entry, cancellationToken).ConfigureAwait(false)
					: null,
				cancellationToken);
		}

		/// <summary>
		/// Returns the valid cached file for <paramref name="uri"/> without performing any I/O, if there is one.
		/// </summary>
		public bool TryGetCachedFile(Uri uri, TimeSpan cacheValidity, out UriImageResult result)
		{
			result = default;

			if (uri is null || !uri.IsAbsoluteUri || !DiskCache.TryGetEntry(uri, out var entry) || entry is null)
				return false;

			if (!IsFresh(entry.ValidatedUtcTicks, cacheValidity))
				return false;

			result = new UriImageResult(null, entry, cacheable: true);
			return true;
		}

		/// <inheritdoc/>
		public Task ClearAsync(CancellationToken cancellationToken = default)
		{
			// Forget the disk entries and start a new cache version in one step: a load either sees the old
			// entries with the old version (and cannot keep what it loads) or neither. Then remove the files.
			lock (_memoryPublishLock)
			{
				DiskCache.ResetIndex(() => _lastClearGeneration = Interlocked.Increment(ref _cacheGeneration));
				MemoryCache.Clear();
			}
			return Task.Run(DiskCache.DeleteUnindexedFiles, cancellationToken);
		}

		/// <inheritdoc/>
		public Task RemoveAsync(Uri uri, CancellationToken cancellationToken = default)
		{
			if (uri is null)
				throw new ArgumentNullException(nameof(uri));

			// Only loads of this image are affected; downloads of it that are in progress are not kept.
			// The disk entry is removed and the version changed in one step (see ClearAsync).
			var key = uri.OriginalString;
			return Task.Run(
				() =>
				{
					lock (_memoryPublishLock)
					{
						DiskCache.Remove(uri, () => ChangeVersion(key));
						MemoryCache.Remove(key);
					}
				},
				cancellationToken);
		}

		/// <summary>
		/// Releases the HTTP client and stops listening for memory warnings. Called when the app's services are disposed.
		/// </summary>
		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposed, 1) != 0)
				return;

#if IOS || MACCATALYST
			_memoryWarningObserver?.Dispose();
#endif
			if (_httpClient.IsValueCreated)
				_httpClient.Value.Dispose();
			MemoryCache.Clear();
		}

		/// <summary>
		/// Removes an image that turned out to be unusable (for example it could not be decoded) from the caches.
		/// </summary>
		internal void Invalidate(Uri uri)
		{
			MemoryCache.Remove(uri.OriginalString);
			DiskCache.Remove(uri);
		}

		/// <summary>
		/// Identifies the state of the cache for <paramref name="uri"/>: it changes when the cache is cleared or the image is removed.
		/// Results of loads that started with another version are not kept.
		/// </summary>
		long GetVersion(Uri uri) =>
			((long)Volatile.Read(ref _cacheGeneration) << 32) | (uint)(_removals.TryGetValue(uri.OriginalString, out var removals) ? removals : 0);

		/// <summary>
		/// Changes the version of the image with cache key <paramref name="key"/>, so that loads that started before cannot keep what they load.
		/// Must be called while holding <see cref="_memoryPublishLock"/> and the disk cache lock.
		/// </summary>
		void ChangeVersion(string key)
		{
			// Keep the number of tracked images bounded: start a new version for all images instead.
			if (_removals.Count >= MaxTrackedRemovals && !_removals.ContainsKey(key))
			{
				Interlocked.Increment(ref _cacheGeneration);
				_removals.Clear();
			}

			_removals.AddOrUpdate(key, 1, static (_, count) => count + 1);
		}

		/// <summary>
		/// Whether the cache was cleared after a load with <paramref name="version"/> started. Must be called while holding <see cref="_memoryPublishLock"/>.
		/// </summary>
		/// <remarks>Unlike the version itself, this does not change when only versions of single images change.</remarks>
		bool ClearedSince(long version) =>
			(int)(version >> 32) < _lastClearGeneration;

		/// <summary>
		/// Removes <paramref name="entry"/> (if it is still the cached version) and the memory copy,
		/// unless the cache was cleared since the load (with <paramref name="version"/>) started.
		/// </summary>
		void InvalidateEntry(Uri uri, UriImageDiskCache.Entry? entry, long version)
		{
			// Same gate as publishing to memory, so a load that already checked its entry cannot put it back.
			lock (_memoryPublishLock)
			{
				if (ClearedSince(version))
					return;

				MemoryCache.Remove(uri.OriginalString);
				if (entry is not null)
					DiskCache.Remove(entry);
			}
		}

		/// <summary>
		/// Forgets the image after the server rejected it (for example 404 or 403): whatever is cached for it, including copies
		/// stored by overlapping loads, and what loads that are still in progress get. Requests that started before the cache
		/// was cleared (with an older <paramref name="version"/>) do not affect what was cached after it (for example for another account).
		/// </summary>
		void Reject(Uri uri, long version)
		{
			var key = uri.OriginalString;
			lock (_memoryPublishLock)
			{
				if (ClearedSince(version))
					return;

				DiskCache.RemoveIfCached(uri, () => ChangeVersion(key));
				MemoryCache.Remove(key);
			}
		}

		internal static int GetDefaultMaxDownloadSize()
		{
			var configuredValue = AppContext.GetData(MaxDownloadSizeAppContextKey);
			if (configuredValue is null)
				return DefaultMaxDownloadSize;

			int maxDownloadSize;
			if (configuredValue is int intValue)
			{
				maxDownloadSize = intValue;
			}
			else if (configuredValue is long longValue && longValue >= int.MinValue && longValue <= int.MaxValue)
			{
				maxDownloadSize = (int)longValue;
			}
			else if (configuredValue is string stringValue &&
				int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
			{
				maxDownloadSize = parsedValue;
			}
			else
			{
				throw new InvalidOperationException(
					$"The AppContext value '{MaxDownloadSizeAppContextKey}' must be an integer number of bytes.");
			}

			if (maxDownloadSize <= 0)
			{
				throw new InvalidOperationException(
					$"The AppContext value '{MaxDownloadSizeAppContextKey}' must be between 1 and {int.MaxValue} bytes.");
			}

			return maxDownloadSize;
		}

		async Task<byte[]> LoadBytesCoreAsync(Uri uri, bool cachingEnabled, TimeSpan cacheValidity, Func<CancellationToken, Task<Stream?>>? fetch, CancellationToken cancellationToken)
		{
			for (var attempt = 0; ; attempt++)
			{
				var version = GetVersion(uri);
				var result = await JoinAsync(uri, version, cachingEnabled, cacheValidity, fetch, cancellationToken).ConfigureAwait(false);

				var data = result.Data;
				if (data is null && result.Entry is not null)
				{
					data = await DiskCache.TryReadAsync(result.Entry, cancellationToken).ConfigureAwait(false);
					if (data is null)
					{
						// The file was removed after it was resolved; resolve again (which downloads it).
						if (attempt > 0)
							throw new IOException($"Unable to read the cached image for '{uri}'.");
						continue;
					}
				}

				if (data is null)
					throw new InvalidOperationException($"Unable to load image from '{uri}'.");

				// Only keep the image in memory when it is still the cached version (not replaced while it was read,
				// and the cache was not cleared or the image removed since the load started).
				if (cachingEnabled && result.Cacheable)
				{
					var validatedAt = result.Entry is not null
						? new DateTimeOffset(result.Entry.ValidatedUtcTicks, TimeSpan.Zero)
						: UtcNow();

					lock (_memoryPublishLock)
					{
						if (version == GetVersion(uri) && (result.Entry is null || DiskCache.IsCurrent(result.Entry)))
							MemoryCache.Set(uri.OriginalString, data, validatedAt);
					}
				}

				return data;
			}
		}

		async Task<UriImageResult> JoinAsync(Uri uri, long version, bool cachingEnabled, TimeSpan cacheValidity, Func<CancellationToken, Task<Stream?>>? fetch, CancellationToken cancellationToken)
		{
			// Loads started after the cache was cleared never share a download that started before it.
			// Custom streams are never shared: each image source provides its own.
			var key = string.Concat(
				version.ToString(CultureInfo.InvariantCulture), "|",
				fetch is null ? "h|" : "s" + Guid.NewGuid().ToString("N") + "|",
				cachingEnabled ? "1|" : "0|",
				cacheValidity.Ticks.ToString(CultureInfo.InvariantCulture), "|",
				uri.OriginalString);

			while (true)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var load = _inFlight.GetOrAdd(key, static _ => new InFlightLoad());
				Task<UriImageResult> task;

				lock (load)
				{
					if (load.IsClosed)
					{
						task = null!;
					}
					else
					{
						load.Waiters++;
						task = load.Task ??= StartLoad(load, key, version, uri, cachingEnabled, cacheValidity, fetch);
					}
				}

				if (task is null)
				{
					// This load finished or was abandoned while we were joining; start or join a new one.
					RemoveInFlight(key, load);
					continue;
				}

				try
				{
					return await WaitAsync(task, cancellationToken).ConfigureAwait(false);
				}
				finally
				{
					ReleaseWaiter(key, load);
				}
			}
		}

		Task<UriImageResult> StartLoad(InFlightLoad load, string key, long version, Uri uri, bool cachingEnabled, TimeSpan cacheValidity, Func<CancellationToken, Task<Stream?>>? fetch)
		{
			var token = load.Cancellation.Token;
			var task = Task.Run(() => ResolveAsync(uri, version, cachingEnabled, cacheValidity, fetch, token), token);

			task.ContinueWith(
				t =>
				{
					// Observe the exception so that abandoned loads do not raise UnobservedTaskException.
					_ = t.Exception;

					lock (load)
						load.IsClosed = true;

					RemoveInFlight(key, load);
					load.Cancellation.Dispose();
				},
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);

			return task;
		}

		void ReleaseWaiter(string key, InFlightLoad load)
		{
			lock (load)
			{
				load.Waiters--;

				if (load.Waiters > 0 || load.IsClosed || load.Task is null || load.Task.IsCompleted)
					return;

				// Nobody is waiting for this image anymore (for example a recycled cell), so stop downloading it.
				load.IsClosed = true;
				load.Cancellation.Cancel();
			}

			RemoveInFlight(key, load);
		}

		void RemoveInFlight(string key, InFlightLoad load) =>
			((System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<string, InFlightLoad>>)_inFlight)
				.Remove(new System.Collections.Generic.KeyValuePair<string, InFlightLoad>(key, load));

		async Task<UriImageResult> ResolveAsync(Uri uri, long version, bool cachingEnabled, TimeSpan cacheValidity, Func<CancellationToken, Task<Stream?>>? fetch, CancellationToken cancellationToken)
		{
			UriImageDiskCache.Entry? cached = null;

			if (cachingEnabled)
			{
				cached = DiskCache.GetEntry(uri);
				if (cached is not null && IsFresh(cached.ValidatedUtcTicks, cacheValidity))
					return new UriImageResult(null, cached, cacheable: true);
			}

			DownloadResponse response;
			try
			{
				// With an expired copy to fall back on, a single attempt is enough: retrying would only delay showing it.
				var maxRetryCount = cached is not null && File.Exists(cached.DataPath) ? 0 : _maxRetryCount;
				response = await DownloadWithRetriesAsync(uri, cached, fetch, maxRetryCount, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex) when (IsPermanentHttpFailure(ex))
			{
				// The server says the image is gone or not accessible (for example 404 or 403): do not keep showing it,
				// including a copy that is only in memory.
				Reject(uri, version);
				throw;
			}
			catch (Exception ex) when (cached is not null && IsTransient(ex, cancellationToken) && File.Exists(cached.DataPath))
			{
				// Prefer an expired image over no image when the server cannot be reached. The copy stays expired,
				// so it is not kept in memory and the next load tries the server again.
				_logger?.LogWarning(ex, "Unable to revalidate image '{Uri}'; using the expired cached copy.", uri);
				return new UriImageResult(null, cached, cacheable: false);
			}

			if (response.NotModified && cached is not null)
			{
				if (response.NoStore)
				{
					// The server confirmed the copy but does not allow storing it: use it once, then forget it.
					var copy = await DiskCache.TryReadAsync(cached, cancellationToken).ConfigureAwait(false);
					InvalidateEntry(uri, cached, version);
					if (copy is not null)
						return new UriImageResult(copy, null, cacheable: false);
				}
				else if (DiskCache.Renew(cached, response.ETag, response.LastModified, UtcNow()))
				{
					return new UriImageResult(null, cached, cacheable: true);
				}

				// The cached file disappeared while revalidating it.
				response = await DownloadWithRetriesAsync(uri, null, fetch, _maxRetryCount, cancellationToken).ConfigureAwait(false);
			}

			var data = response.Data!;
			var cacheable = cachingEnabled && !response.NoStore;

			UriImageDiskCache.Entry? entry = null;
			if (cacheable && version == GetVersion(uri))
			{
				entry = DiskCache.Write(uri, data, response.ETag, response.LastModified, UtcNow(), () => version == GetVersion(uri));

				// The cache was cleared (or the image removed) while the image was written: do not keep it.
				if (entry is not null && version != GetVersion(uri))
				{
					DiskCache.Remove(entry);
					entry = null;
				}
			}
			else if (cachingEnabled && response.NoStore)
			{
				InvalidateEntry(uri, cached, version);
			}

			return new UriImageResult(data, entry, cacheable && version == GetVersion(uri));
		}

		async Task<DownloadResponse> DownloadWithRetriesAsync(Uri uri, UriImageDiskCache.Entry? cached, Func<CancellationToken, Task<Stream?>>? fetch, int maxRetryCount, CancellationToken cancellationToken)
		{
			for (var attempt = 0; ; attempt++)
			{
				try
				{
					await _downloadSlots.WaitAsync(cancellationToken).ConfigureAwait(false);
					try
					{
						return fetch is null
							? await DownloadAsync(uri, cached, cancellationToken).ConfigureAwait(false)
							: await FetchAsync(uri, fetch, cancellationToken).ConfigureAwait(false);
					}
					finally
					{
						_downloadSlots.Release();
					}
				}
				catch (Exception ex) when (attempt < maxRetryCount && IsTransient(ex, cancellationToken))
				{
					var delay = GetRetryDelay(ex, attempt);
					_logger?.LogDebug(ex, "Downloading image '{Uri}' failed (attempt {Attempt}); retrying in {Delay}.", uri, attempt + 1, delay);
					await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
				}
			}
		}

		async Task<DownloadResponse> DownloadAsync(Uri uri, UriImageDiskCache.Entry? cached, CancellationToken cancellationToken)
		{
			using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			timeout.CancelAfter(_requestTimeout);

			using var request = new HttpRequestMessage(HttpMethod.Get, uri);
			if (cached is not null)
			{
				if (cached.ETag is { } eTag && EntityTagHeaderValue.TryParse(eTag, out var parsedETag))
					request.Headers.IfNoneMatch.Add(parsedETag);
				else if (cached.LastModified is { } lastModified &&
					DateTimeOffset.TryParse(lastModified, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedLastModified))
					request.Headers.IfModifiedSince = parsedLastModified;
			}

			try
			{
				// Some platform handlers do not observe cancellation while connecting,
				// so stop waiting when the attempt times out even if the handler does not.
				using var response = await WaitOrAbandonAsync(
					_httpClient.Value.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token),
					timeout.Token).ConfigureAwait(false);

				var eTag = response.Headers.ETag?.ToString();
				var lastModified = response.Content.Headers.LastModified?.ToString("R", CultureInfo.InvariantCulture);
				var noStore = response.Headers.CacheControl?.NoStore == true;

				if (response.StatusCode == HttpStatusCode.NotModified && cached is not null)
					return new DownloadResponse(null, eTag, lastModified, noStore, notModified: true);

				if (!response.IsSuccessStatusCode)
					throw CreateHttpException(uri, response);

				var mediaType = response.Content.Headers.ContentType?.MediaType;
				if (IsNonImageMediaType(mediaType))
					throw new InvalidDataException($"The response from '{uri}' is not an image (Content-Type '{mediaType}').");

				var maxSize = _maxDownloadSize ?? GetDefaultMaxDownloadSize();
				if (response.Content.Headers.ContentLength is long contentLength && contentLength > maxSize)
					throw new InvalidDataException($"The image at '{uri}' is {contentLength} bytes, which is larger than the maximum of {maxSize} bytes.");

				// From here on the timeout applies to each read, so slow downloads that keep making progress succeed.
				var data = await ReadBoundedAsync(response.Content, maxSize, uri, timeout, _requestTimeout).ConfigureAwait(false);
				if (data.Length == 0)
					throw new InvalidDataException($"The response from '{uri}' was empty.");

				return new DownloadResponse(data, eTag, lastModified, noStore, notModified: false);
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				throw new TimeoutException($"Downloading the image at '{uri}' timed out after {_requestTimeout.TotalSeconds:0.##} seconds.", ex);
			}
		}

		async Task<DownloadResponse> FetchAsync(Uri uri, Func<CancellationToken, Task<Stream?>> fetch, CancellationToken cancellationToken)
		{
			using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			timeout.CancelAfter(_requestTimeout);

			try
			{
				using var stream = await WaitOrAbandonAsync(fetch(timeout.Token), timeout.Token).ConfigureAwait(false)
					?? throw new InvalidOperationException($"Unable to load image stream from URI '{uri}'.");

				var data = await ReadBoundedAsync(stream, _maxDownloadSize ?? GetDefaultMaxDownloadSize(), uri, timeout, _requestTimeout).ConfigureAwait(false);
				if (data.Length == 0)
					throw new InvalidDataException($"The image stream for '{uri}' was empty.");

				return new DownloadResponse(data, null, null, noStore: false, notModified: false);
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				throw new TimeoutException($"Loading the image at '{uri}' timed out after {_requestTimeout.TotalSeconds:0.##} seconds.", ex);
			}
		}

		static async Task<byte[]> ReadBoundedAsync(HttpContent content, long maxSize, Uri uri, CancellationTokenSource timeout, TimeSpan idleTimeout)
		{
			timeout.CancelAfter(idleTimeout);
#if NETSTANDARD
			using var stream = await WaitOrAbandonAsync(content.ReadAsStreamAsync(), timeout.Token).ConfigureAwait(false);
#else
			using var stream = await WaitOrAbandonAsync(content.ReadAsStreamAsync(timeout.Token), timeout.Token).ConfigureAwait(false);
#endif
			return await ReadBoundedAsync(stream, maxSize, uri, timeout, idleTimeout).ConfigureAwait(false);
		}

		/// <summary>
		/// Reads the whole stream, failing when it is larger than <paramref name="maxSize"/> or when a single read
		/// takes longer than <paramref name="idleTimeout"/> (even if the stream does not observe cancellation).
		/// </summary>
		static async Task<byte[]> ReadBoundedAsync(Stream stream, long maxSize, Uri uri, CancellationTokenSource timeout, TimeSpan idleTimeout)
		{
			using var buffer = new MemoryStream();
			var chunk = new byte[16 * 1024];
			while (true)
			{
				timeout.CancelAfter(idleTimeout);
				var read = await WaitOrAbandonAsync(stream.ReadAsync(chunk, 0, chunk.Length, timeout.Token), timeout.Token).ConfigureAwait(false);
				if (read <= 0)
					break;

				if (buffer.Length + read > maxSize)
					throw new InvalidDataException($"The image at '{uri}' is larger than the maximum of {maxSize} bytes.");
				buffer.Write(chunk, 0, read);
			}
			return buffer.ToArray();
		}

		static bool IsNonImageMediaType(string? mediaType)
		{
			if (string.IsNullOrEmpty(mediaType))
				return false;

			// Typical for error or login pages served with a 200 status. Everything else (including images that
			// misconfigured servers label as text/plain) is left to the platform decoder.
			return mediaType!.Equals("text/html", StringComparison.OrdinalIgnoreCase)
				|| mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase)
				|| mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)
				|| mediaType.Equals("application/xhtml+xml", StringComparison.OrdinalIgnoreCase);
		}

		static HttpRequestException CreateHttpException(Uri uri, HttpResponseMessage response)
		{
			var statusCode = response.StatusCode;
			var message = $"Downloading the image at '{uri}' failed with HTTP status {(int)statusCode} ({response.ReasonPhrase}).";
#if NETSTANDARD
			var exception = new HttpRequestException(message);
#else
			var exception = new HttpRequestException(message, null, statusCode);
#endif
			exception.Data[StatusCodeKey] = (int)statusCode;

			var retryAfter = response.Headers.RetryAfter;
			if (retryAfter?.Delta is TimeSpan delta)
				exception.Data[RetryAfterKey] = delta;
			else if (retryAfter?.Date is DateTimeOffset date)
				exception.Data[RetryAfterKey] = date - DateTimeOffset.UtcNow;

			return exception;
		}

		static bool IsPermanentHttpFailure(Exception exception) =>
			exception is HttpRequestException && exception.Data[StatusCodeKey] is int status &&
			status >= 400 && status < 500 && status != 408 && status != 429;

		static bool IsTransient(Exception exception, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return false;

			switch (exception)
			{
				case TimeoutException:
					return true;
				case HttpRequestException httpException:
					if (httpException.Data[StatusCodeKey] is int status)
						return status == 408 || status == 429 || (status >= 500 && status < 600);
					for (var inner = exception.InnerException; inner is not null; inner = inner.InnerException)
					{
						if (inner is AuthenticationException)
							return false;
					}
					return true;
				case IOException:
					return true;
				default:
					return false;
			}
		}

		TimeSpan GetRetryDelay(Exception exception, int attempt)
		{
			if (exception.Data[RetryAfterKey] is TimeSpan retryAfter && retryAfter > TimeSpan.Zero)
				return retryAfter < MaxRetryAfter ? retryAfter : MaxRetryAfter;

			var baseMilliseconds = _retryDelay.TotalMilliseconds * Math.Pow(2, attempt);
			double jitter;
			lock (_jitter)
			{
#pragma warning disable CA5394 // Retry jitter does not need a secure random number generator.
				jitter = _jitter.NextDouble() * 0.2 * baseMilliseconds;
#pragma warning restore CA5394
			}
			return TimeSpan.FromMilliseconds(baseMilliseconds + jitter);
		}

		bool IsFresh(long validatedUtcTicks, TimeSpan cacheValidity) =>
			cacheValidity > TimeSpan.Zero && UtcNow().UtcTicks - validatedUtcTicks < cacheValidity.Ticks;

		static void ValidateUri(Uri uri, Func<CancellationToken, Task<Stream?>>? fetch)
		{
			if (uri is null)
				throw new ArgumentNullException(nameof(uri));

			if (fetch is null && !CanLoad(uri))
				throw new ArgumentException($"'{uri}' is not an absolute http or https URI.", nameof(uri));
		}

		static HttpClient CreateHttpClient(Func<HttpMessageHandler>? handlerFactory, Action<HttpClient>? configure)
		{
			var client = handlerFactory is null
				? new HttpClient()
				: new HttpClient(handlerFactory(), disposeHandler: true);

			// Each attempt has its own timeout (ImageLoadingOptions.RequestTimeout).
			client.Timeout = Timeout.InfiniteTimeSpan;
			configure?.Invoke(client);
			return client;
		}

		static string? GetDefaultCacheDirectory()
		{
			try
			{
				return Path.Combine(Storage.FileSystem.CacheDirectory, "com.microsoft.maui", "MauiUriImages");
			}
			catch (Exception)
			{
				// No file system available (for example netstandard or unit tests): the disk cache is disabled.
				return null;
			}
		}

		/// <summary>
		/// Waits for <paramref name="task"/> until <paramref name="cancellationToken"/> is cancelled, even if the operation ignores it.
		/// An abandoned operation is observed, and its result is disposed when it completes later.
		/// </summary>
		static async Task<T> WaitOrAbandonAsync<T>(Task<T> task, CancellationToken cancellationToken)
		{
			try
			{
				return await WaitAsync(task, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				// Also when the operation completed at the same time: its result must still be released.
				_ = task.ContinueWith(
					static t =>
					{
						if (t.Status == TaskStatus.RanToCompletion)
							(t.Result as IDisposable)?.Dispose();
						else
							_ = t.Exception;
					},
					CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
				throw;
			}
		}

		/// <summary>
		/// Makes <paramref name="fetch"/> run on the calling thread's context (for example the UI thread), where custom
		/// image sources provided their stream before the loader existed. Everything else runs on the thread pool.
		/// </summary>
		static Func<CancellationToken, Task<Stream?>>? OnCallerContext(Func<CancellationToken, Task<Stream?>>? fetch)
		{
			var context = SynchronizationContext.Current;
			if (fetch is null || context is null)
				return fetch;

			return token =>
			{
				var completion = new TaskCompletionSource<Stream?>(TaskCreationOptions.RunContinuationsAsynchronously);
				context.Post(_ =>
				{
					try
					{
						fetch(token).ContinueWith(
							t =>
							{
								if (t.IsCanceled)
									completion.TrySetCanceled();
								else if (t.IsFaulted)
									completion.TrySetException(t.Exception!.InnerExceptions);
								else
									completion.TrySetResult(t.Result);
							},
							CancellationToken.None,
							TaskContinuationOptions.ExecuteSynchronously,
							TaskScheduler.Default);
					}
					catch (Exception ex)
					{
						completion.TrySetException(ex);
					}
				}, null);
				return completion.Task;
			};
		}

		static async Task<T> WaitAsync<T>(Task<T> task, CancellationToken cancellationToken)
		{
#if NETSTANDARD
			if (!cancellationToken.CanBeCanceled || task.IsCompleted)
				return await task.ConfigureAwait(false);

			var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			using (cancellationToken.Register(static state => ((TaskCompletionSource<bool>)state!).TrySetResult(true), cancelled))
			{
				if (await Task.WhenAny(task, cancelled.Task).ConfigureAwait(false) != task)
					throw new OperationCanceledException(cancellationToken);
			}

			return await task.ConfigureAwait(false);
#else
			return await task.WaitAsync(cancellationToken).ConfigureAwait(false);
#endif
		}

		sealed class InFlightLoad
		{
			public CancellationTokenSource Cancellation { get; } = new();

			public Task<UriImageResult>? Task;

			public int Waiters;

			public bool IsClosed;
		}

		readonly struct DownloadResponse
		{
			public DownloadResponse(byte[]? data, string? eTag, string? lastModified, bool noStore, bool notModified)
			{
				Data = data;
				ETag = eTag;
				LastModified = lastModified;
				NoStore = noStore;
				NotModified = notModified;
			}

			public byte[]? Data { get; }

			public string? ETag { get; }

			public string? LastModified { get; }

			public bool NoStore { get; }

			public bool NotModified { get; }
		}
	}

	/// <summary>
	/// Marks an <see cref="IUriImageSource"/> whose stream is a plain HTTP download of its URI, so the loader downloads the URI itself.
	/// </summary>
	internal interface ILoaderUriImageSource
	{
	}

	/// <summary>
	/// The result of loading a remote image: the bytes, the cached file, or both.
	/// </summary>
	internal readonly struct UriImageResult
	{
		public UriImageResult(byte[]? data, UriImageDiskCache.Entry? entry, bool cacheable)
		{
			Data = data;
			Entry = entry;
			Cacheable = cacheable;
		}

		public byte[]? Data { get; }

		internal UriImageDiskCache.Entry? Entry { get; }

		/// <summary>The cached file, if the image is stored in the disk cache.</summary>
		public string? FilePath => Entry?.DataPath;

		/// <summary>Changes whenever the content of <see cref="FilePath"/> changes.</summary>
		public long Version => Entry?.DownloadedUtcTicks ?? 0;

		public bool Cacheable { get; }
	}
}
