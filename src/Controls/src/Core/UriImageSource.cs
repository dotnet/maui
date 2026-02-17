#nullable disable
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls
{
	// TODO: CACHING https://github.com/dotnet/runtime/issues/52332
	/// <summary>An <see cref="ImageSource"/> that loads an image from a URI, with caching support.</summary>
	public sealed partial class UriImageSource : ImageSource, IStreamImageSource
	{
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
			nameof(CachingEnabled), typeof(bool), typeof(UriImageSource), true);

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
			set => SetValue(CachingEnabledProperty, value);
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
			userToken.Register(CancellationTokenSource.Cancel);
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
				MauiLog.Warning<UriImageSource>(ex, "Error getting stream for {Uri}", Uri);
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

			Stream stream = null;

			if (CachingEnabled)
			{
				// TODO: CACHING https://github.com/dotnet/runtime/issues/52332

				// var key = GetKey();
				// var cached = TryGetFromCache(key, out stream)
				if (stream is null)
					stream = await DownloadStreamAsync(uri, cancellationToken).ConfigureAwait(false);
				// if (!cached)
				//    Cache(key, stream)
			}
			else
			{
				stream = await DownloadStreamAsync(uri, cancellationToken).ConfigureAwait(false);
			}

			return stream;
		}

		async Task<Stream> DownloadStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			try
			{
				using var client = new HttpClient();

				// Do not remove this await otherwise the client will dispose before
				// the stream even starts
				return await StreamWrapper.GetStreamAsync(uri, cancellationToken, client).ConfigureAwait(false);
			}
			catch (Exception ex)
			{

				MauiLog.Warning<UriImageSource>(ex, "Error getting stream for {Uri}", Uri);
				return null;
			}
		}

		void OnUriChanged()
		{
			CancellationTokenSource?.Cancel();

			OnSourceChanged();
		}
	}
}
