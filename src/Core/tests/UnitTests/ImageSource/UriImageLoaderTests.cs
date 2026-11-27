using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.ImageSource
{
	[Category(TestCategory.Core, TestCategory.ImageSource)]
	public class UriImageLoaderTests : IDisposable
	{
		static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4 };
		static readonly byte[] OtherPng = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 5, 6, 7, 8 };
		static readonly Uri ImageUri = new("https://example.com/images/photo.png?size=large");

		readonly string _cacheDirectory = Path.Combine(Path.GetTempPath(), "maui-uri-image-tests", Guid.NewGuid().ToString("N"));
		readonly FakeHandler _handler = new();
		DateTimeOffset _now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

		public void Dispose()
		{
			try
			{
				Directory.Delete(_cacheDirectory, recursive: true);
			}
			catch (DirectoryNotFoundException)
			{
			}
		}

		UriImageLoader CreateLoader(Action<ImageLoadingOptions> configure = null, bool? acceptFilesWithoutMetadata = null)
		{
			var options = new ImageLoadingOptions
			{
				CacheDirectory = _cacheDirectory,
				HttpMessageHandlerFactory = () => _handler,
				RetryDelay = TimeSpan.FromMilliseconds(1),
			};
			configure?.Invoke(options);
			return new UriImageLoader(options, null, acceptFilesWithoutMetadata) { UtcNow = () => _now };
		}

		[Fact]
		public async Task SecondLoadIsServedFromMemorySynchronously()
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader();

			Assert.Equal(Png, await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));

			var second = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			Assert.True(second.IsCompletedSuccessfully);
			Assert.Equal(Png, await second);
			Assert.Equal(1, _handler.RequestCount);
		}

		[Fact]
		public async Task ImagesArePersistedToDisk()
		{
			_handler.Respond(_ => Ok(Png));
			await CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromDays(1));

			var data = await CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromDays(1));

			Assert.Equal(Png, data);
			Assert.Equal(1, _handler.RequestCount);
		}

		[Fact]
		public async Task ExpiredImageIsRevalidatedAndRenewedOnNotModified()
		{
			_handler.Respond(_ => Ok(Png, etag: "\"v1\""));
			var loader = CreateLoader();
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1));

			_now = _now.AddHours(2);
			_handler.Respond(request =>
			{
				Assert.Contains(request.Headers.IfNoneMatch, e => e.Tag == "\"v1\"");
				return new HttpResponseMessage(HttpStatusCode.NotModified);
			});

			Assert.Equal(Png, await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));
			Assert.Equal(2, _handler.RequestCount);

			// Renewed: valid again for the full validity period, even in a new loader (from disk).
			_now = _now.AddMinutes(50);
			Assert.Equal(Png, await CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));
			Assert.Equal(2, _handler.RequestCount);
		}

		[Fact]
		public async Task ExpiredImageIsReplacedWhenTheServerReturnsNewContent()
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader();
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1));

			_now = _now.AddHours(2);
			_handler.Respond(_ => Ok(OtherPng));

			Assert.Equal(OtherPng, await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));
			Assert.Equal(OtherPng, await CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));
			Assert.Equal(2, _handler.RequestCount);
		}

		[Fact]
		public async Task ExpiredImageIsUsedWhenTheServerCannotBeReached()
		{
			_handler.Respond(_ => Ok(Png));
			await CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromHours(1));

			_now = _now.AddDays(3);
			_handler.Respond(new Func<HttpRequestMessage, HttpResponseMessage>(_ => throw new HttpRequestException("offline")));

			Assert.Equal(Png, await CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));

			// No retries: the expired copy is shown as soon as the server cannot be reached.
			Assert.Equal(2, _handler.RequestCount);
		}

		[Theory]
		[InlineData(HttpStatusCode.NotFound)]
		[InlineData(HttpStatusCode.Forbidden)]
		public async Task ExpiredImageIsNotUsedWhenTheServerRejectsIt(HttpStatusCode status)
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader();
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1));

			_now = _now.AddHours(2);
			_handler.Respond(_ => new HttpResponseMessage(status));

			var ex = await Assert.ThrowsAsync<HttpRequestException>(() => loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));
			Assert.Equal(status, ex.StatusCode);
			Assert.Null(loader.DiskCache.GetEntry(ImageUri));
		}

		[Fact]
		public async Task ImageOnlyInMemoryIsNotUsedAfterTheServerRejectsIt()
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader(o => o.DiskCacheSizeLimit = 0);
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));

			_handler.Respond(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));
			await Assert.ThrowsAsync<HttpRequestException>(() => loader.LoadAsync(ImageUri, true, TimeSpan.Zero));

			// A longer validity must not bring back the rejected image.
			await Assert.ThrowsAsync<HttpRequestException>(() => loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
			Assert.Equal(3, _handler.RequestCount);
		}

		[Theory]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task ImageIsNotCachedWhenAnOverlappingRequestWasRejected(bool rejectionFirst, bool diskCacheEnabled)
		{
			// A downloads the image while B (another validity, so another request) is rejected by the server.
			var startedA = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var startedB = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var releaseA = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var releaseB = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			_handler.Respond(async _ =>
			{
				if (startedA.TrySetResult(true))
				{
					await releaseA.Task;
					return Ok(Png);
				}

				startedB.SetResult(true);
				await releaseB.Task;
				return new HttpResponseMessage(HttpStatusCode.Forbidden);
			});
			var loader = CreateLoader(o => o.DiskCacheSizeLimit = diskCacheEnabled ? ImageLoadingOptions.DefaultDiskCacheSizeLimit : 0);

			var a = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			await startedA.Task;
			var b = loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1));
			await startedB.Task;

			if (rejectionFirst)
			{
				releaseB.SetResult(true);
				await Assert.ThrowsAsync<HttpRequestException>(() => b);
				releaseA.SetResult(true);
				Assert.Equal(Png, await a);
			}
			else
			{
				releaseA.SetResult(true);
				Assert.Equal(Png, await a);
				releaseB.SetResult(true);
				await Assert.ThrowsAsync<HttpRequestException>(() => b);
			}

			Assert.False(loader.MemoryCache.TryGet(ImageUri.OriginalString, out _));
			Assert.Null(loader.DiskCache.GetEntry(ImageUri));
		}

		[Fact]
		public async Task RejectionDoesNotDeleteFilesTheCacheDidNotWrite()
		{
			// The test loader uses a custom cache directory, where files without metadata are not the cache's.
			Directory.CreateDirectory(_cacheDirectory);
			var unrelated = Path.Combine(_cacheDirectory, UriImageDiskCache.GetFileName(ImageUri));
			File.WriteAllText(unrelated, "{}");
			_handler.Respond(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

			await Assert.ThrowsAsync<HttpRequestException>(() => CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));

			Assert.True(File.Exists(unrelated));
		}

		[Fact]
		public async Task RejectionIsAppliedWhenManyOtherImagesWereRemovedMeanwhile()
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader();
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1));

			var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			_handler.Respond(async _ =>
			{
				started.TrySetResult(true);
				await release.Task;
				return new HttpResponseMessage(HttpStatusCode.Forbidden);
			});
			var revalidation = loader.LoadAsync(ImageUri, true, TimeSpan.Zero);
			await started.Task;

			// Enough removals of other images to exceed the number of images whose versions are tracked.
			for (var i = 0; i <= UriImageLoader.MaxTrackedRemovals; i++)
				await loader.RemoveAsync(new Uri($"https://example.com/other/{i}.png"));

			release.SetResult(true);
			await Assert.ThrowsAsync<HttpRequestException>(() => revalidation);

			Assert.Null(loader.DiskCache.GetEntry(ImageUri));
			await Assert.ThrowsAsync<HttpRequestException>(() => loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));
		}

		[Fact]
		public async Task RejectionFromBeforeAClearDoesNotRemoveWhatWasCachedAfterIt()
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader();
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1));

			var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			_handler.Respond(async _ =>
			{
				started.TrySetResult(true);
				await release.Task;
				return new HttpResponseMessage(HttpStatusCode.Forbidden);
			});
			var oldRequest = loader.LoadAsync(ImageUri, true, TimeSpan.Zero);
			await started.Task;

			// For example the user signed in to another account: the cache is cleared and the image is loaded again.
			await loader.ClearAsync();
			_handler.Respond(_ => Ok(OtherPng));
			Assert.Equal(OtherPng, await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));

			release.SetResult(true);
			await Assert.ThrowsAsync<HttpRequestException>(() => oldRequest);

			Assert.NotNull(loader.DiskCache.GetEntry(ImageUri));
			Assert.True(loader.MemoryCache.TryGet(ImageUri.OriginalString, out var item));
			Assert.Equal(OtherPng, item.Data);
		}

		[Fact]
		public async Task CachedImagesCanBeReadWithoutDownloadingThem()
		{
			_handler.Respond(_ => Ok(Png));
			await CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromHours(1));

			// Even when expired, and only from the disk (another loader).
			_now = _now.AddDays(3);
			var loader = CreateLoader();
			Assert.Equal(Png, await loader.TryLoadCachedAsync(ImageUri));
			Assert.Null(await loader.TryLoadCachedAsync(new Uri("https://example.com/other.png")));
			Assert.Equal(1, _handler.RequestCount);
		}

		[Fact]
		public async Task NotModifiedWithNoStoreDoesNotRenew()
		{
			_handler.Respond(_ => Ok(Png, etag: "\"v1\""));
			var loader = CreateLoader(o => o.MemoryCacheSizeLimit = 0);
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1));

			_now = _now.AddHours(2);
			_handler.Respond(_ =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.NotModified);
				response.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true };
				return response;
			});

			Assert.Equal(Png, await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));
			Assert.Null(loader.DiskCache.GetEntry(ImageUri));
		}

		[Fact]
		public async Task ExpiredImageIsNotUsedWhenTheServerReturnsSomethingElse()
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader();
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1));

			// For example a login page after the session expired.
			_now = _now.AddHours(2);
			_handler.Respond(_ =>
			{
				var response = Ok(System.Text.Encoding.UTF8.GetBytes("<html>login</html>"));
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
				return response;
			});

			await Assert.ThrowsAsync<InvalidDataException>(() => loader.LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));
		}

		[Fact]
		public async Task RemoveAsyncDoesNotAffectOtherImages()
		{
			var release = new TaskCompletionSource<bool>();
			_handler.Respond(async request =>
			{
				if (request.RequestUri.AbsolutePath.EndsWith("other.png", StringComparison.Ordinal))
					await release.Task;
				return Ok(Png);
			});
			var loader = CreateLoader();
			var other = new Uri("https://example.com/other.png");

			var otherLoad = loader.LoadAsync(other, true, TimeSpan.FromDays(1));
			await WaitUntil(() => _handler.RequestCount == 1);
			await loader.RemoveAsync(ImageUri);
			release.SetResult(true);
			await otherLoad;

			Assert.NotNull(loader.DiskCache.GetEntry(other));
			Assert.True(loader.LoadAsync(other, true, TimeSpan.FromDays(1)).IsCompletedSuccessfully);
		}

		[Fact]
		public async Task CustomStreamsAreNotShared()
		{
			var release = new TaskCompletionSource<bool>();
			var calls = 0;
			async Task<Stream> Fetch(CancellationToken token)
			{
				Interlocked.Increment(ref calls);
				await release.Task;
				return new MemoryStream(Png);
			}
			var loader = CreateLoader();
			var uri = new Uri("custom://images/shared");

			var first = loader.LoadAsync(uri, false, TimeSpan.FromDays(1), Fetch);
			var second = loader.LoadAsync(uri, false, TimeSpan.FromDays(1), Fetch);
			await WaitUntil(() => Volatile.Read(ref calls) == 2);
			release.SetResult(true);

			await Task.WhenAll(first, second);
		}

		[Fact]
		public async Task LoadAfterClearAsyncDoesNotShareADownloadStartedBeforeIt()
		{
			var release = new TaskCompletionSource<bool>();
			_handler.Respond(async _ =>
			{
				await release.Task;
				return Ok(Png);
			});
			var loader = CreateLoader();

			var before = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			await WaitUntil(() => _handler.RequestCount == 1);
			await loader.ClearAsync();
			var after = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			await WaitUntil(() => _handler.RequestCount == 2);

			release.SetResult(true);
			await Task.WhenAll(before, after);
			Assert.Equal(2, _handler.RequestCount);
		}

		[Fact]
		public async Task ClearAsyncKeepsFilesTheCacheDidNotWrite()
		{
			Directory.CreateDirectory(_cacheDirectory);
			var sentinel = Path.Combine(_cacheDirectory, "settings.json");
			File.WriteAllText(sentinel, "{}");
			var hashNamedSentinel = Path.Combine(_cacheDirectory, "0123456789ABCDEF.profile.json");
			File.WriteAllText(hashNamedSentinel, "{}");
			var metaSentinel = Path.Combine(_cacheDirectory, "0123456789ABCDEF.notes.meta");
			File.WriteAllText(metaSentinel, "not image cache metadata");
			var tmpSentinel = Path.Combine(_cacheDirectory, "0123456789ABCDEF.download.tmp");
			File.WriteAllText(tmpSentinel, "{}");
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader(o => o.DiskCacheSizeLimit = 1);
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));

			await loader.ClearAsync();

			Assert.Equal(
				new[] { tmpSentinel, metaSentinel, hashNamedSentinel, sentinel }.OrderBy(f => f, StringComparer.Ordinal),
				Directory.GetFiles(_cacheDirectory).OrderBy(f => f, StringComparer.Ordinal));
		}

		[Fact]
		public async Task SlowDownloadsThatKeepProgressingDoNotTimeOut()
		{
			_handler.Respond(_ =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new SlowStream(Png, TimeSpan.FromMilliseconds(40))) };
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
				return response;
			});
			var loader = CreateLoader(o =>
			{
				o.RequestTimeout = TimeSpan.FromMilliseconds(150);
				o.MaxRetryCount = 0;
			});

			// 12 bytes, one byte per read, 40 ms each: about 500 ms in total, but never 150 ms without progress.
			Assert.Equal(Png, await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
		}

		[Fact]
		public async Task StalledDownloadsTimeOutEvenIfTheStreamIgnoresCancellation()
		{
			_handler.Respond(_ =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new SlowStream(Png, Timeout.InfiniteTimeSpan)) };
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
				return response;
			});
			var loader = CreateLoader(o =>
			{
				o.RequestTimeout = TimeSpan.FromMilliseconds(100);
				o.MaxRetryCount = 0;
			});

			var load = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			Assert.Same(load, await Task.WhenAny(load, Task.Delay(5000)));
			await Assert.ThrowsAsync<TimeoutException>(() => load);
		}

		[Fact]
		public async Task ImagesLabeledAsPlainTextAreAccepted()
		{
			_handler.Respond(_ =>
			{
				var response = Ok(Png);
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
				return response;
			});

			Assert.Equal(Png, await CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
		}

		[Fact]
		public async Task ImageUrlsEndingInCacheFileExtensionsAreCached()
		{
			_handler.Respond(_ => Ok(Png));
			var uri = new Uri("https://example.com/images/photo.meta");
			await CreateLoader().LoadAsync(uri, true, TimeSpan.FromDays(1));

			Assert.Equal(Png, await CreateLoader().LoadAsync(uri, true, TimeSpan.FromDays(1)));
			Assert.Equal(1, _handler.RequestCount);
		}

		[Fact]
		public async Task CachingDisabledAlwaysDownloadsAndStoresNothing()
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader();

			await loader.LoadAsync(ImageUri, false, TimeSpan.FromDays(1));
			await loader.LoadAsync(ImageUri, false, TimeSpan.FromDays(1));

			Assert.Equal(2, _handler.RequestCount);
			Assert.Equal(0, loader.MemoryCache.Count);
			Assert.False(Directory.Exists(_cacheDirectory) && Directory.EnumerateFiles(_cacheDirectory).Any());
		}

		[Fact]
		public async Task ConcurrentLoadsShareOneDownload()
		{
			var release = new TaskCompletionSource<bool>();
			_handler.Respond(async _ =>
			{
				await release.Task;
				return Ok(Png);
			});
			var loader = CreateLoader();

			var loads = Enumerable.Range(0, 5).Select(_ => loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1))).ToArray();
			await WaitUntil(() => _handler.RequestCount == 1);
			release.SetResult(true);

			foreach (var data in await Task.WhenAll(loads))
				Assert.Equal(Png, data);
			Assert.Equal(1, _handler.RequestCount);
		}

		[Fact]
		public async Task DownloadIsCancelledWhenTheLastWaiterCancels()
		{
			var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			_handler.Respond(async (request, token) =>
			{
				using var registration = token.Register(() => cancelled.TrySetResult(true));
				await Task.Delay(Timeout.Infinite, token);
				return Ok(Png);
			});
			var loader = CreateLoader();

			using var first = new CancellationTokenSource();
			using var second = new CancellationTokenSource();
			var firstLoad = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1), first.Token);
			var secondLoad = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1), second.Token);
			await WaitUntil(() => _handler.RequestCount == 1);

			first.Cancel();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => firstLoad);
			Assert.False(cancelled.Task.IsCompleted);

			second.Cancel();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => secondLoad);
			Assert.True(await Task.WhenAny(cancelled.Task, Task.Delay(5000)) == cancelled.Task);
			await WaitUntil(() => loader.InFlightCount == 0);
		}

		[Fact]
		public async Task RemainingWaiterStillGetsTheImage()
		{
			var release = new TaskCompletionSource<bool>();
			_handler.Respond(async _ =>
			{
				await release.Task;
				return Ok(Png);
			});
			var loader = CreateLoader();

			using var cts = new CancellationTokenSource();
			var cancelledLoad = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1), cts.Token);
			var load = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			await WaitUntil(() => _handler.RequestCount == 1);

			cts.Cancel();
			release.SetResult(true);

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelledLoad);
			Assert.Equal(Png, await load);
		}

		[Theory]
		[InlineData(HttpStatusCode.ServiceUnavailable)]
		[InlineData(HttpStatusCode.TooManyRequests)]
		[InlineData(HttpStatusCode.RequestTimeout)]
		public async Task TransientFailuresAreRetried(HttpStatusCode status)
		{
			var attempts = 0;
			_handler.Respond(_ => ++attempts < 3 ? new HttpResponseMessage(status) : Ok(Png));

			Assert.Equal(Png, await CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
			Assert.Equal(3, _handler.RequestCount);
		}

		[Fact]
		public async Task PermanentFailuresAreNotRetriedAndNotCached()
		{
			_handler.Respond(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
			var loader = CreateLoader();

			var ex = await Assert.ThrowsAsync<HttpRequestException>(() => loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
			Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
			Assert.Equal(1, _handler.RequestCount);

			_handler.Respond(_ => Ok(Png));
			Assert.Equal(Png, await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
		}

		[Fact]
		public async Task RetriesAreLimited()
		{
			_handler.Respond(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

			await Assert.ThrowsAsync<HttpRequestException>(() => CreateLoader(o => o.MaxRetryCount = 1).LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
			Assert.Equal(2, _handler.RequestCount);
		}

		[Fact]
		public async Task AttemptsTimeOut()
		{
			_handler.Respond(async (_, token) =>
			{
				await Task.Delay(Timeout.Infinite, token);
				return Ok(Png);
			});
			var loader = CreateLoader(o =>
			{
				o.RequestTimeout = TimeSpan.FromMilliseconds(50);
				o.MaxRetryCount = 1;
			});

			await Assert.ThrowsAsync<TimeoutException>(() => loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
			Assert.Equal(2, _handler.RequestCount);
		}

		[Fact]
		public async Task AttemptsTimeOutWhenTheHandlerIgnoresCancellation()
		{
			var never = new TaskCompletionSource<HttpResponseMessage>();
			_handler.Respond((_, _) => never.Task);
			var loader = CreateLoader(o =>
			{
				o.RequestTimeout = TimeSpan.FromMilliseconds(50);
				o.MaxRetryCount = 0;
			});

			var load = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));

			Assert.Same(load, await Task.WhenAny(load, Task.Delay(5000)));
			await Assert.ThrowsAsync<TimeoutException>(() => load);
		}

		[Fact]
		public async Task NonImageResponsesAreRejected()
		{
			_handler.Respond(_ =>
			{
				var response = Ok(System.Text.Encoding.UTF8.GetBytes("<html>login</html>"));
				response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
				return response;
			});

			await Assert.ThrowsAsync<InvalidDataException>(() => CreateLoader().LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
			Assert.Equal(1, _handler.RequestCount);
		}

		[Fact]
		public async Task ResponsesLargerThanTheLimitAreRejected()
		{
			_handler.Respond(_ => Ok(new byte[1024]));

			await Assert.ThrowsAsync<InvalidDataException>(() => CreateLoader(o => o.MaxDownloadSize = 100).LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
		}

		[Fact]
		public async Task NoStoreResponsesAreNotPersisted()
		{
			_handler.Respond(_ =>
			{
				var response = Ok(Png);
				response.Headers.CacheControl = new CacheControlHeaderValue { NoStore = true };
				return response;
			});
			var loader = CreateLoader();

			await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));

			Assert.Equal(2, _handler.RequestCount);
		}

		[Fact]
		public async Task LeastRecentlyUsedImagesAreEvictedFromDisk()
		{
			_handler.Respond(_ => Ok(new byte[400]));
			var loader = CreateLoader(o =>
			{
				o.DiskCacheSizeLimit = 1000;
				o.MemoryCacheSizeLimit = 0;
			});
			var first = new Uri("https://example.com/1.png");
			var second = new Uri("https://example.com/2.png");
			var third = new Uri("https://example.com/3.png");

			await loader.LoadAsync(first, true, TimeSpan.FromDays(1));
			_now = _now.AddMinutes(1);
			await loader.LoadAsync(second, true, TimeSpan.FromDays(1));
			_now = _now.AddMinutes(1);
			await loader.LoadAsync(first, true, TimeSpan.FromDays(1)); // first is now more recently used
			_now = _now.AddMinutes(1);
			await loader.LoadAsync(third, true, TimeSpan.FromDays(1));

			Assert.True(loader.DiskCache.Size <= 1000);
			Assert.NotNull(loader.DiskCache.GetEntry(first));
			Assert.Null(loader.DiskCache.GetEntry(second));
			Assert.NotNull(loader.DiskCache.GetEntry(third));
		}

		[Fact]
		public async Task ClearAsyncRemovesEverything()
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader();
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));

			await ((IImageCache)loader).ClearAsync();

			Assert.Equal(0, loader.MemoryCache.Count);
			Assert.Empty(Directory.EnumerateFiles(_cacheDirectory));
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			Assert.Equal(2, _handler.RequestCount);
		}

		[Fact]
		public async Task DownloadThatFinishesAfterClearAsyncIsNotPersisted()
		{
			var release = new TaskCompletionSource<bool>();
			_handler.Respond(async _ =>
			{
				await release.Task;
				return Ok(Png);
			});
			var loader = CreateLoader();

			var load = loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			await WaitUntil(() => _handler.RequestCount == 1);
			await loader.ClearAsync();
			release.SetResult(true);

			Assert.Equal(Png, await load);
			Assert.Equal(0, loader.MemoryCache.Count);
			Assert.False(Directory.Exists(_cacheDirectory) && Directory.EnumerateFiles(_cacheDirectory).Any());
		}

		[Fact]
		public async Task RemoveAsyncRemovesOneImage()
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader();
			var other = new Uri("https://example.com/other.png");
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			await loader.LoadAsync(other, true, TimeSpan.FromDays(1));

			await loader.RemoveAsync(ImageUri);

			await loader.LoadAsync(other, true, TimeSpan.FromDays(1));
			Assert.Equal(2, _handler.RequestCount);
			await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1));
			Assert.Equal(3, _handler.RequestCount);
		}

		[Fact]
		public async Task FilesWithoutMetadataAreUsedAsCachedImages()
		{
			// Files written by the previous iOS cache: "{crc64}{extension}" without metadata.
			Directory.CreateDirectory(_cacheDirectory);
			var legacyPath = Path.Combine(_cacheDirectory, UriImageDiskCache.GetFileName(ImageUri));
			File.WriteAllBytes(legacyPath, Png);
			File.SetLastWriteTimeUtc(legacyPath, _now.UtcDateTime.AddMinutes(-10));

			Assert.Equal(Png, await CreateLoader(acceptFilesWithoutMetadata: true).LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));
			Assert.Equal(0, _handler.RequestCount);

			_now = _now.AddHours(1);
			_handler.Respond(_ => Ok(OtherPng));
			Assert.Equal(OtherPng, await CreateLoader(acceptFilesWithoutMetadata: true).LoadAsync(ImageUri, true, TimeSpan.FromHours(1)));
		}

		[Fact]
		public async Task LegacyFileNameIsKept()
		{
			using var crc = new Crc64HashAlgorithm();
			Assert.Equal(crc.ComputeHashString(ImageUri.OriginalString) + ".png", UriImageDiskCache.GetFileName(ImageUri));

			_handler.Respond(_ => Ok(Png));
			var result = await CreateLoader().LoadFileAsync(ImageUri, true, TimeSpan.FromDays(1));
			Assert.Equal(Path.Combine(_cacheDirectory, UriImageDiskCache.GetFileName(ImageUri)), result.FilePath);
		}

		[Fact]
		public async Task CachedFileIsAvailableWithoutIOOnceIndexed()
		{
			_handler.Respond(_ => Ok(Png));
			var loader = CreateLoader();
			var downloaded = await loader.LoadFileAsync(ImageUri, true, TimeSpan.FromDays(1));

			Assert.True(loader.TryGetCachedFile(ImageUri, TimeSpan.FromDays(1), out var cached));
			Assert.Equal(downloaded.FilePath, cached.FilePath);
			Assert.Equal(downloaded.Version, cached.Version);

			_now = _now.AddDays(2);
			Assert.False(loader.TryGetCachedFile(ImageUri, TimeSpan.FromDays(1), out _));
		}

		[Fact]
		public async Task VersionChangesOnlyWhenTheContentChanges()
		{
			_handler.Respond(_ => Ok(Png, etag: "\"v1\""));
			var loader = CreateLoader();
			var first = await loader.LoadFileAsync(ImageUri, true, TimeSpan.FromHours(1));

			_now = _now.AddHours(2);
			_handler.Respond(_ => new HttpResponseMessage(HttpStatusCode.NotModified));
			var revalidated = await loader.LoadFileAsync(ImageUri, true, TimeSpan.FromHours(1));
			Assert.Equal(first.Version, revalidated.Version);

			_now = _now.AddHours(2);
			_handler.Respond(_ => Ok(OtherPng));
			var changed = await loader.LoadFileAsync(ImageUri, true, TimeSpan.FromHours(1));
			Assert.NotEqual(first.Version, changed.Version);
		}

		[Fact]
		public async Task CustomStreamIsUsedAndCached()
		{
			var calls = 0;
			var loader = CreateLoader();
			Task<Stream> Fetch(CancellationToken token)
			{
				calls++;
				return Task.FromResult<Stream>(new MemoryStream(Png));
			}

			var uri = new Uri("custom://images/1");
			Assert.Equal(Png, await loader.LoadAsync(uri, true, TimeSpan.FromDays(1), Fetch));
			Assert.Equal(Png, await CreateLoader().LoadAsync(uri, true, TimeSpan.FromDays(1), Fetch));

			Assert.Equal(1, calls);
			Assert.Equal(0, _handler.RequestCount);
		}

		[Fact]
		public async Task DownloadsAreLimited()
		{
			var release = new TaskCompletionSource<bool>();
			_handler.Respond(async _ =>
			{
				await release.Task;
				return Ok(Png);
			});
			var loader = CreateLoader(o => o.MaxConcurrentDownloads = 2);

			var loads = Enumerable.Range(0, 4)
				.Select(i => loader.LoadAsync(new Uri($"https://example.com/{i}.png"), true, TimeSpan.FromDays(1)))
				.ToArray();
			await WaitUntil(() => _handler.RequestCount == 2);
			await Task.Delay(50);
			Assert.Equal(2, _handler.RequestCount);

			release.SetResult(true);
			await Task.WhenAll(loads);
			Assert.Equal(4, _handler.RequestCount);
		}

		[Fact]
		public void ConfigureImageLoadingConfiguresTheLoader()
		{
			HttpClient configuredClient = null;
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			builder.ConfigureImageSources();
			builder.ConfigureImageLoading(o => o.CacheDirectory = _cacheDirectory);
			builder.ConfigureImageLoading(o =>
			{
				o.MemoryCacheSizeLimit = 0;
				o.ConfigureHttpClient = client => configuredClient = client;
			});
			var services = builder.Build().Services;

			var loader = services.GetRequiredService<UriImageLoader>();
			Assert.Same(loader, services.GetRequiredService<IImageCache>());
			Assert.Equal(_cacheDirectory, loader.DiskCache.Directory);
			Assert.Same(loader, UriImageLoader.GetCurrent(services));

			var service = (UriImageSourceService)services.GetRequiredService<IImageSourceServiceProvider>()
				.GetRequiredImageSourceService(typeof(IUriImageSource));
			Assert.Same(loader, service.Loader);
		}

		[Fact]
		public async Task ConfigureHttpClientIsInvoked()
		{
			_handler.Respond(request =>
			{
				Assert.Equal("Bearer token", request.Headers.Authorization?.ToString());
				return Ok(Png);
			});
			var loader = CreateLoader(o => o.ConfigureHttpClient = client => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token"));

			Assert.Equal(Png, await loader.LoadAsync(ImageUri, true, TimeSpan.FromDays(1)));
		}

		static HttpResponseMessage Ok(byte[] data, string etag = null)
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(data) };
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
			if (etag is not null)
				response.Headers.ETag = EntityTagHeaderValue.Parse(etag);
			return response;
		}

		static async Task WaitUntil(Func<bool> condition)
		{
			var timeout = DateTime.UtcNow.AddSeconds(5);
			while (!condition())
			{
				if (DateTime.UtcNow > timeout)
					throw new TimeoutException("Condition was not met.");
				await Task.Delay(5);
			}
		}

		/// <summary>A stream that returns one byte per read after a delay, ignoring cancellation.</summary>
		sealed class SlowStream : Stream
		{
			readonly byte[] _data;
			readonly TimeSpan _delay;
			int _position;

			public SlowStream(byte[] data, TimeSpan delay)
			{
				_data = data;
				_delay = delay;
			}

			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException();
			public override long Position { get => _position; set => throw new NotSupportedException(); }
			public override void Flush() { }
			public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

			public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
			{
				await Task.Delay(_delay, CancellationToken.None);
				if (_position >= _data.Length)
					return 0;
				buffer[offset] = _data[_position++];
				return 1;
			}

			public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
			{
				var array = new byte[buffer.Length];
				return new ValueTask<int>(ReadAsync(array, 0, array.Length, cancellationToken).ContinueWith(t =>
				{
					array.AsSpan(0, t.Result).CopyTo(buffer.Span);
					return t.Result;
				}, TaskScheduler.Default));
			}
		}

		sealed class FakeHandler : HttpMessageHandler
		{
			Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _respond =
				(_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
			int _requestCount;

			public int RequestCount => Volatile.Read(ref _requestCount);

			public void Respond(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
				_respond = (request, _) => Task.FromResult(respond(request));

			public void Respond(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) =>
				_respond = (request, _) => respond(request);

			public void Respond(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) =>
				_respond = respond;

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				Interlocked.Increment(ref _requestCount);
				return _respond(request, cancellationToken);
			}
		}
	}
}
