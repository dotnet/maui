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
		public override Task<IImageSourceServiceResult?> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
		{
			var fileImageSource = (IFileImageSource)imageSource;

			if (!fileImageSource.IsEmpty)
			{
				var callback = new ImageLoaderCallback();

				try
				{
					var id = imageView.Context?.GetDrawableId(fileImageSource.File) ?? -1;
					if (id > 0)
					{
						imageView.SetImageResource(id);
						return Task.FromResult<IImageSourceServiceResult?>(new ImageSourceServiceLoadResult());
					}
					else
					{
						PlatformInterop.LoadImageFromFile(imageView, fileImageSource.File, callback);
					}

					return callback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image file '{File}'.", fileImageSource.File);
					throw;
				}
			}

			return Task.FromResult<IImageSourceServiceResult?>(null);
		}

		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			var fileImageSource = (IFileImageSource)imageSource;
			if (!fileImageSource.IsEmpty)
			{
				try
				{
					var id = context?.GetDrawableId(fileImageSource.File) ?? -1;
					if (id > 0)
					{
						var d = context?.GetDrawable(id);
						if (d is not null)
							return Task.FromResult<IImageSourceServiceResult<Drawable>?>(new ImageSourceServiceResult(d));
					}

					var drawableCallback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromFile(context, fileImageSource.File, drawableCallback);

					return drawableCallback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image file '{File}'.", fileImageSource.File);
					throw;
				}
			}
			
			return Task.FromResult<IImageSourceServiceResult<Drawable>?>(null);
		}
	}
}