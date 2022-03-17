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
		public override async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(IImageSource imageSource, Android.Widget.ImageView imageView, CancellationToken cancellationToken = default)
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
						return new ImageSourceServiceResult(true);
					}
					else
					{
						PlatformInterop.LoadImageFromFile(imageView, fileImageSource.File, callback);
					}

					var result = await callback.Result.ConfigureAwait(false);
					if (!result.Value)
						throw new ApplicationException($"Unable to load image file '{fileImageSource.File}'.");

					return result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image file '{File}'.", fileImageSource.File);
					throw;
				}
			}
			return new ImageSourceServiceResult(false);
		}

		public override async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(Context context, IImageSource imageSource, Action<Drawable?> callback, CancellationToken cancellationToken = default)
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

						callback?.Invoke(d);

						return new ImageSourceServiceResult(d is not null);
					}

					var drawableCallback = new ImageLoaderCallback(callback);

					PlatformInterop.LoadImageFromFile(context, fileImageSource.File, drawableCallback);

					var result = await drawableCallback.Result.ConfigureAwait(false);
					if (!result.Value)
						throw new ApplicationException($"Unable to load image file '{fileImageSource.File}'.");
					
					return result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image file '{File}'.", fileImageSource.File);
					throw;
				}
			}
			return new ImageSourceServiceResult(false);
		}
	}
}