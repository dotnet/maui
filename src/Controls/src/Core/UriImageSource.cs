using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls
{
	// TODO: CACHING https://github.com/dotnet/runtime/issues/52332
	/// <include file="../../docs/Microsoft.Maui.Controls/UriImageSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.UriImageSource']/Docs/*" />
	public sealed partial class UriImageSource : ImageSource, IStreamImageSource
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/UriImageSource.xml" path="//Member[@MemberName='UriProperty']/Docs/*" />
		public static readonly BindableProperty UriProperty = BindableProperty.Create(
			nameof(Uri), typeof(Uri), typeof(UriImageSource), default(Uri),
			propertyChanged: (bindable, oldvalue, newvalue) => ((UriImageSource)bindable).OnUriChanged(),
			validateValue: (bindable, value) => value == null || ((Uri)value).IsAbsoluteUri);

		public static readonly BindableProperty CacheValidityProperty = BindableProperty.Create(
			nameof(CacheValidity), typeof(TimeSpan), typeof(UriImageSource), TimeSpan.FromDays(1));

		public static readonly BindableProperty CachingEnabledProperty = BindableProperty.Create(
			nameof(CachingEnabled), typeof(bool), typeof(UriImageSource), true);

		/// <include file="../../docs/Microsoft.Maui.Controls/UriImageSource.xml" path="//Member[@MemberName='IsEmpty']/Docs/*" />
		public override bool IsEmpty => Uri == null;

		/// <include file="../../docs/Microsoft.Maui.Controls/UriImageSource.xml" path="//Member[@MemberName='CacheValidity']/Docs/*" />
		public TimeSpan CacheValidity
		{
			get => (TimeSpan)GetValue(CacheValidityProperty);
			set => SetValue(CacheValidityProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/UriImageSource.xml" path="//Member[@MemberName='CachingEnabled']/Docs/*" />
		public bool CachingEnabled
		{
			get => (bool)GetValue(CachingEnabledProperty);
			set => SetValue(CachingEnabledProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/UriImageSource.xml" path="//Member[@MemberName='Uri']/Docs/*" />
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

			OnLoadingStarted();
			userToken.Register(CancellationTokenSource.Cancel);
			Stream stream;

			try
			{
				stream = await GetStreamAsync(Uri, CancellationTokenSource.Token);
				OnLoadingCompleted(false);
			}
			catch (OperationCanceledException)
			{
				OnLoadingCompleted(true);
				throw;
			}
			catch (Exception ex)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<UriImageSource>()?.LogWarning(ex, "Error getting stream for {Uri}", Uri);
				throw;
			}

			return stream;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/UriImageSource.xml" path="//Member[@MemberName='ToString']/Docs/*" />
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

				Application.Current?.FindMauiContext()?.CreateLogger<UriImageSource>()?.LogWarning(ex, "Error getting stream for {Uri}", Uri);
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
