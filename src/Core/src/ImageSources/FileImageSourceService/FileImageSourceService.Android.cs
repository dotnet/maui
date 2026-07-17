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

		public override async Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
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

							var result = await resourceCallback.Result.ConfigureAwait(false);
							if (result is null)
							{
								Logger?.LogWarning("Unable to load image resource '{File}'.", file);
								return null;
							}

							// The async Glide callback is not itself cancelable, so recheck the token before
							// returning: a superseded (canceled) resource load must not overwrite a newer source
							// that a non-ImageView consumer already applied (ImageSourcePartExtensions.UpdateSourceAsync).
							if (cancellationToken.IsCancellationRequested)
							{
								return null;
							}

							return result;
						}
					}

					var callback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromFile(context, file, callback);

					var fileResult = await callback.Result.ConfigureAwait(false);

					// Same cancellation recheck as the resource path above: don't apply a stale drawable from a
					// superseded load after a newer source has been requested on this same context consumer.
					if (cancellationToken.IsCancellationRequested)
					{
						return null;
					}

					return fileResult;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image file '{File}'.", file);
					throw;
				}
			}

			return null;
		}
	}
}