#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class UriImageSourceService
	{
		public override async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
		{
			var uriImageSource = (IUriImageSource)imageSource;
			if (!uriImageSource.IsEmpty)
			{
				try
				{
					var callback = new ImageLoaderCallback();

					PlatformInterop.LoadImageFromUri(imageView, uriImageSource.Uri.OriginalString, new Java.Lang.Boolean(uriImageSource.CachingEnabled), callback);

					var result = await callback.Result.ConfigureAwait(false);
					if (!result.Value)
						throw new ApplicationException($"Unable to load image uri '{uriImageSource.Uri.OriginalString}'.");
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image uri '{Uri}'.", uriImageSource.Uri.OriginalString);
					throw;
				}
			}

			return new ImageSourceServiceResult(false);
		}

		public override async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(Context context, IImageSource imageSource, Action<Drawable?> callback, CancellationToken cancellationToken = default)
		{
			var uriImageSource = (IUriImageSource)imageSource;
			if (!uriImageSource.IsEmpty)
			{
				try
				{
					var drawableCallback = new ImageLoaderCallback(callback);

					PlatformInterop.LoadImageFromUri(context, uriImageSource.Uri.OriginalString, new Java.Lang.Boolean(uriImageSource.CachingEnabled), drawableCallback);

					var result = await drawableCallback.Result.ConfigureAwait(false);
					if (!result.Value)
						throw new ApplicationException($"Unable to load image uri '{uriImageSource.Uri.OriginalString}'.");
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image uri '{Uri}'.", uriImageSource.Uri.OriginalString);
					throw;
				}
			}

			return new ImageSourceServiceResult(false);
		}
	}
}