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

							// The async Glide callback is not itself cancelable, so recheck the token before
							// inspecting the result. A superseded (canceled) resource load must propagate
							// cancellation even when the callback returned null, otherwise the non-ImageView
							// consumer (ImageSourcePartExtensions.UpdateSourceAsync) treats null as a Glide
							// failure and calls setImage(null), clearing the newer source it already applied.
							// Dispose any drawable we are dropping and propagate cancellation so the caller's
							// OperationCanceledException path leaves the newer image intact.
							if (cancellationToken.IsCancellationRequested)
							{
								result?.Dispose();
								throw new OperationCanceledException(cancellationToken);
							}

							if (result is null)
							{
								Logger?.LogWarning("Unable to load image resource '{File}'.", file);
								return null;
							}

							return result;
						}
					}

					var callback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromFile(context, file, callback);

					var fileResult = await callback.Result.ConfigureAwait(false);

					// Same cancellation handling as the resource path above: propagate cancellation (disposing
					// the dropped drawable) instead of returning a null that a superseded load would turn into
					// setImage(null), clearing a newer source on the non-ImageView consumer.
					if (cancellationToken.IsCancellationRequested)
					{
						fileResult?.Dispose();
						throw new OperationCanceledException(cancellationToken);
					}

					return fileResult;
				}
				catch (OperationCanceledException)
				{
					// Normal cancellation of a superseded load — propagate without logging it as a failure.
					throw;
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