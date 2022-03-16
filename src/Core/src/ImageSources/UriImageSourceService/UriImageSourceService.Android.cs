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
		public override Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
		{
			if (imageSource is IUriImageSource uriImageSource && !uriImageSource.IsEmpty)
			{
				try
				{
					var callback = new ImageLoaderCallback();

					ImageLoader.LoadFromUri(imageView, uriImageSource.Uri.OriginalString, new Java.Lang.Boolean(uriImageSource.CachingEnabled), callback);

					return callback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image stream.");
					throw;
				}
			}

			return Task.FromResult<IImageSourceServiceResult<bool>>(new ImageSourceServiceResult(false));
		}

		public override Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(Context context, IImageSource imageSource, Action<Drawable?> callback, CancellationToken cancellationToken = default)
		{
			if (imageSource is IUriImageSource uriImageSource && !uriImageSource.IsEmpty)
			{
				try
				{
					var drawableCallback = new ImageLoaderDrawableCallback(callback);

					ImageLoader.LoadFromUri(context, uriImageSource.Uri.OriginalString, new Java.Lang.Boolean(uriImageSource.CachingEnabled), drawableCallback);

					return drawableCallback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image stream.");
					throw;
				}
			}

			return Task.FromResult<IImageSourceServiceResult<bool>>(new ImageSourceServiceResult(false));
		}
	}
}