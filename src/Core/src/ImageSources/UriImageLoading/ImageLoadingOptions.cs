#nullable enable
using System;
using System.Net.Http;

namespace Microsoft.Maui
{
	/// <summary>
	/// Configures how remote images (for example a <c>UriImageSource</c>) are downloaded and cached.
	/// </summary>
	/// <remarks>
	/// Configure these options with <see cref="Hosting.ImageSourcesMauiAppBuilderExtensions.ConfigureImageLoading(Hosting.MauiAppBuilder, Action{ImageLoadingOptions})"/>.
	/// Per-image caching is still controlled by <see cref="IUriImageSource.CachingEnabled"/> and <see cref="IUriImageSource.CacheValidity"/>.
	/// </remarks>
	public sealed class ImageLoadingOptions
	{
		internal const int DefaultMaxConcurrentDownloads = 6;
		internal const int DefaultMaxRetryCount = 2;
		internal const long DefaultDiskCacheSizeLimit = 256L * 1024 * 1024;
		internal const long DefaultMemoryCacheSizeLimit = 16L * 1024 * 1024;
		internal static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromMilliseconds(500);
		internal static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(30);

		/// <summary>
		/// Gets or sets a callback that is invoked once when the shared <see cref="HttpClient"/> used for image downloads is created,
		/// for example to add default request headers such as <c>Authorization</c> or <c>User-Agent</c>.
		/// </summary>
		/// <remarks>
		/// Cached images are keyed by URI only. If the response depends on request headers (for example a signed-in user),
		/// clear the cache with <see cref="IImageCache.ClearAsync"/> when those headers change.
		/// </remarks>
		public Action<HttpClient>? ConfigureHttpClient { get; set; }

		/// <summary>
		/// Gets or sets a factory for the <see cref="HttpMessageHandler"/> used for image downloads.
		/// When <see langword="null"/>, the platform default handler is used.
		/// </summary>
		public Func<HttpMessageHandler>? HttpMessageHandlerFactory { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of image downloads that may run at the same time. The default is 6.
		/// </summary>
		/// <remarks>Additional downloads wait for a free slot. Cache hits are not limited.</remarks>
		public int MaxConcurrentDownloads { get; set; } = DefaultMaxConcurrentDownloads;

		/// <summary>
		/// Gets or sets how many times a download is retried after a transient failure
		/// (a network error, a timeout, or an HTTP 408, 429 or 5xx response). The default is 2. Set to 0 to disable retries.
		/// </summary>
		public int MaxRetryCount { get; set; } = DefaultMaxRetryCount;

		/// <summary>
		/// Gets or sets the base delay before retrying a failed download. Each further retry doubles the delay. The default is 500 milliseconds.
		/// </summary>
		/// <remarks>A <c>Retry-After</c> header sent by the server takes precedence, up to a limit.</remarks>
		public TimeSpan RetryDelay { get; set; } = DefaultRetryDelay;

		/// <summary>
		/// Gets or sets the timeout for a single download attempt. The default is 30 seconds.
		/// </summary>
		public TimeSpan RequestTimeout { get; set; } = DefaultRequestTimeout;

		/// <summary>
		/// Gets or sets the maximum size, in bytes, of a downloaded image.
		/// When <see langword="null"/>, the <c>Microsoft.Maui.Controls.UriImageSource.MaxResponseContentBufferSize</c> <see cref="AppContext"/> value is used, or 50 MB if it is not set.
		/// </summary>
		public long? MaxDownloadSize { get; set; }

		/// <summary>
		/// Gets or sets the maximum total size, in bytes, of downloaded images kept on disk. The least recently used images are removed first. The default is 256 MB.
		/// </summary>
		public long DiskCacheSizeLimit { get; set; } = DefaultDiskCacheSizeLimit;

		/// <summary>
		/// Gets or sets the maximum total size, in bytes, of encoded images kept in memory so that they can be shown again without reading from disk.
		/// The default is 16 MB. Set to 0 to disable the memory cache.
		/// </summary>
		/// <remarks>On Android, decoded images are cached in memory by the platform image loader instead.</remarks>
		public long MemoryCacheSizeLimit { get; set; } = DefaultMemoryCacheSizeLimit;

		/// <summary>
		/// Gets or sets the directory where downloaded images are cached.
		/// When <see langword="null"/>, a folder inside the app's cache directory is used.
		/// </summary>
		/// <remarks>Use a directory dedicated to the image cache. Files the cache did not write are left alone.</remarks>
		public string? CacheDirectory { get; set; }
	}
}
