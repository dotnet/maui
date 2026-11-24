#nullable disable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>An <see cref="ImageSource"/> that loads an image from a URI, with caching support.</summary>
	/// <remarks>
	/// Images are downloaded with a shared <see cref="System.Net.Http.HttpClient"/> and cached in memory and on disk for <see cref="CacheValidity"/>.
	/// Expired images are revalidated with the server and are still shown when the server cannot be reached.
	/// Configure downloading and caching with <c>MauiAppBuilder.ConfigureImageLoading</c>, and clear the cache with <see cref="IImageCache"/>.
	/// </remarks>
	public sealed partial class UriImageSource : ImageSource, IStreamImageSource, ILoaderUriImageSource
	{
		internal const int DefaultMaxResponseContentBufferSize = UriImageLoader.DefaultMaxDownloadSize;
		internal const string MaxResponseContentBufferSizeAppContextKey = UriImageLoader.MaxDownloadSizeAppContextKey;

		/// <summary>Bindable property for <see cref="Uri"/>.</summary>
		public static readonly BindableProperty UriProperty = BindableProperty.Create(
			nameof(Uri), typeof(Uri), typeof(UriImageSource), default(Uri),
			propertyChanged: (bindable, oldvalue, newvalue) => ((UriImageSource)bindable).OnUriChanged(),
			validateValue: (bindable, value) => value == null || ((Uri)value).IsAbsoluteUri);

		/// <summary>Bindable property for <see cref="CacheValidity"/>.</summary>
		public static readonly BindableProperty CacheValidityProperty = BindableProperty.Create(
			nameof(CacheValidity), typeof(TimeSpan), typeof(UriImageSource), TimeSpan.FromDays(1));

		/// <summary>Bindable property for <see cref="CachingEnabled"/>.</summary>
		public static readonly BindableProperty CachingEnabledProperty = BindableProperty.Create(
			nameof(CachingEnabled), typeof(bool), typeof(UriImageSource), BooleanBoxes.TrueBox);

		/// <summary>Gets a value indicating whether this image source is empty.</summary>
		public override bool IsEmpty => Uri == null;

		/// <summary>Gets or sets how long the cached image remains valid. This is a bindable property.</summary>
		public TimeSpan CacheValidity
		{
			get => (TimeSpan)GetValue(CacheValidityProperty);
			set => SetValue(CacheValidityProperty, value);
		}

		/// <summary>Gets or sets whether caching is enabled. This is a bindable property.</summary>
		public bool CachingEnabled
		{
			get => (bool)GetValue(CachingEnabledProperty);
			set => SetValue(CachingEnabledProperty, BooleanBoxes.Box(value));
		}

		/// <summary>Gets or sets the URI of the image to load. This is a bindable property.</summary>
		[System.ComponentModel.TypeConverter(typeof(UriTypeConverter))]
		public Uri Uri
		{
			get => (Uri)GetValue(UriProperty);
			set => SetValue(UriProperty, value);
		}

		async Task<Stream> IStreamImageSource.GetStreamAsync(CancellationToken userToken)
		{
			if (IsEmpty)
				return null;

			await OnLoadingStarted();
			using var registration = userToken.Register(CancellationTokenSource.Cancel);
			Stream stream;

			try
			{
				stream = await GetStreamAsync(Uri, CancellationTokenSource.Token);
				await OnLoadingCompleted(false);
			}
			catch (OperationCanceledException)
			{
				await OnLoadingCompleted(true);
				throw;
			}
			catch (Exception ex)
			{
				MauiLogger<UriImageSource>.Log(LogLevel.Warning, ex, $"Error getting stream for {Uri}");
				throw;
			}

			return stream;
		}

		/// <summary>Returns a string representation of this <see cref="UriImageSource"/>.</summary>
		public override string ToString()
		{
			return $"Uri: {Uri}";
		}

		async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				var data = await UriImageLoader.GetCurrent()
					.LoadAsync(uri, CachingEnabled, CacheValidity, cancellationToken)
					.ConfigureAwait(false);

				return new MemoryStream(data, writable: false);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch (Exception ex)
			{
				MauiLogger<UriImageSource>.Log(LogLevel.Warning, ex, $"Error getting stream for {Uri}");
				return null;
			}
		}

		internal static int GetMaxResponseContentBufferSize() =>
			UriImageLoader.GetDefaultMaxDownloadSize();

		void OnUriChanged()
		{
			CancellationTokenSource?.Cancel();

			OnSourceChanged();
		}
	}
}
