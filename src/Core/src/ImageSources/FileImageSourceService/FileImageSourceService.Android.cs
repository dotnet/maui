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
	public partial class FileImageSourceService
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
							var resourceCallback = new ImageLoaderCallback();
							PlatformInterop.LoadImageFromResource(imageView, id, resourceCallback);
							return resourceCallback.Result;
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

		public override Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			var fileImageSource = (IFileImageSource)imageSource;
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
							var resourceCallback = new ImageLoaderResultCallback();
							PlatformInterop.LoadImageFromResource(context, id, resourceCallback);
							return resourceCallback.Result;
						}
					}

					var callback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromFile(context, file, callback);

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