#nullable enable
using System;
using System.IO;
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
			var streamImageSource = (IStreamImageSource)imageSource;

			if (!streamImageSource.IsEmpty)
			{
				Stream? stream = null;
				try
				{
					stream = await streamImageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);
					
					var callback = new ImageLoaderCallback(drawable =>
					{
						stream?.Dispose();
					});

					PlatformInterop.LoadImageFromStream(imageView, stream, callback);

					var result = await callback.Result.ConfigureAwait(false);
					if (!result.Value)
						throw new ApplicationException($"Unable to load image stream.");

					return result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image stream.");
					throw;
				}
				finally
				{
					if (stream != null)
						GC.KeepAlive(stream);
				}
			}

			return new ImageSourceServiceResult(false);
		}

		public override async Task<IImageSourceServiceResult<bool>> LoadDrawableAsync(Context context, IImageSource imageSource, Action<Drawable?> callback, CancellationToken cancellationToken = default)
		{
			var streamImageSource = (IStreamImageSource)imageSource;

			if (!streamImageSource.IsEmpty)
			{
				Stream? stream = null;

				try
				{
					stream = await streamImageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);

					var drawableCallback = new ImageLoaderCallback(drawable =>
					{
						callback(drawable);
						stream?.Dispose();
					});

					PlatformInterop.LoadImageFromStream(context, stream, drawableCallback);

					var result = await drawableCallback.Result.ConfigureAwait(false);
					if (!result.Value)
						throw new ApplicationException($"Unable to load image stream.");

					return result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image stream.");
					throw;
				}
				finally
				{
					if (stream != null)
						GC.KeepAlive(stream);
				}
			}

			return new ImageSourceServiceResult(false);
		}
	}
}