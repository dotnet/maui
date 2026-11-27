#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class FileImageSourceService : IViewSizedImageSourceService
	{
		public override Task<IImageSourceServiceResult?> LoadDrawableAsync(IImageSource imageSource, ImageView imageView, CancellationToken cancellationToken = default)
		{
			var fileImageSource = (IFileImageSource)imageSource;

			if (!fileImageSource.IsEmpty)
			{
				var file = fileImageSource.File;

				try
				{
					if (!Path.IsPathRooted(file) || !File.Exists(file))
					{
						var id = imageView.Context?.GetDrawableId(file) ?? -1;
						if (id > 0)
						{
							imageView.SetImageResource(id);
							return Task.FromResult<IImageSourceServiceResult?>(new ImageSourceServiceLoadResult());
						}
					}

					var callback = new ImageLoaderCallback();

					PlatformInterop.LoadImageFromFile(imageView, file, callback);

					return callback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image file '{File}'.", file);
					throw;
				}
			}

			return Task.FromResult<IImageSourceServiceResult?>(null);
		}

		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default) =>
			GetDrawableAsync((IFileImageSource)imageSource, context, IViewSizedImageSourceService.SizeOriginal, IViewSizedImageSourceService.SizeOriginal);

		Task<IImageSourceServiceResult<Drawable>?> IViewSizedImageSourceService.GetDrawableAsync(IImageSource imageSource, Context context, int width, int height, CancellationToken cancellationToken) =>
			// A derived service may load images differently: use its implementation (at the image's own size).
			GetType() == typeof(FileImageSourceService)
				? GetDrawableAsync((IFileImageSource)imageSource, context, width, height)
				: GetDrawableAsync(imageSource, context, cancellationToken);

		Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IFileImageSource fileImageSource, Context context, int width, int height)
		{
			if (!fileImageSource.IsEmpty)
			{
				var file = fileImageSource.File;

				try
				{
					if (!Path.IsPathRooted(file) || !File.Exists(file))
					{
						var id = context?.GetDrawableId(file) ?? -1;
						if (id > 0)
						{
							var d = context?.GetDrawable(id);
							if (d is not null)
								return Task.FromResult<IImageSourceServiceResult<Drawable>?>(new ImageSourceServiceResult(d));
						}
					}

					var callback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromFile(context, file, width, height, callback);

					return callback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image file '{File}'.", file);
					throw;
				}
			}

			return Task.FromResult<IImageSourceServiceResult<Drawable>?>(null);
		}
	}
}