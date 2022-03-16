#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService
	{
		public override async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
		{
			if (imageSource is IStreamImageSource streamImageSource && !streamImageSource.IsEmpty)
			{
				try
				{
					var stream = await streamImageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);

					var callback = new ImageLoaderCallback();

					ImageLoader.LoadFromStream(imageView, stream, callback);

					return await callback.Result.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image stream.");
					throw;
				}
			}

			return new ImageSourceServiceResult(false);
		}

		public override async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(Context context, IImageSource imageSource, Action<Drawable?> callback, CancellationToken cancellationToken = default)
		{
			if (imageSource is IStreamImageSource streamImageSource && !streamImageSource.IsEmpty)
			{
				try
				{
					var stream = await streamImageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);

					var drawableCallback = new ImageLoaderCallback(callback);

					ImageLoader.LoadFromStream(context, stream, drawableCallback);

					return await drawableCallback.Result.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image stream.");
					throw;
				}
			}

			return new ImageSourceServiceResult(false);
		}
	}
}