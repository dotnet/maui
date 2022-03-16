#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService
	{
		public override Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
		{
			if (imageSource is IFileImageSource fileImageSource)
			{
				var callback = new ImageLoaderCallback();

				try
				{
					var id = imageView.Context?.GetDrawableId(fileImageSource.File) ?? -1;
					if (id > 0)
					{
						imageView.SetImageResource(id);
						return Task.FromResult<IImageSourceServiceResult<bool>>(new ImageSourceServiceResult(true));
					}
					else
					{
						ImageLoader.LoadFromFile(imageView, fileImageSource.File, callback);
					}

					return callback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image file '{File}'.", fileImageSource.File);
					throw;
				}
			}

			return Task.FromResult<IImageSourceServiceResult<bool>>(new ImageSourceServiceResult(false));
		}

		public override Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(Context context, IImageSource imageSource, Action<Drawable?> callback, CancellationToken cancellationToken = default)
		{
			if (imageSource is IFileImageSource fileImageSource)
			{
				try
				{
					var id = context?.GetDrawableId(fileImageSource.File) ?? -1;
					if (id > 0)
					{
						var d = context?.GetDrawable(id);

						callback?.Invoke(d);

						return Task.FromResult<IImageSourceServiceResult<bool>>(new ImageSourceServiceResult(d is not null));
					}

					var drawableCallback = new ImageLoaderCallback(callback);

					ImageLoader.LoadFromFile(context, fileImageSource.File, drawableCallback);

					return drawableCallback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image file '{File}'.", fileImageSource.File);
					throw;
				}
			}

			return Task.FromResult<IImageSourceServiceResult<bool>>(new ImageSourceServiceResult(false));
		}
	}
}