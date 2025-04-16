#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Widget;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui
{
	public partial class StreamImageSourceService
	{
		public override async Task<IImageSourceServiceResult?> LoadDrawableAsync(IImageSource imageSource, ImageView imageView, CancellationToken cancellationToken = default)
		{
			var streamImageSource = (IStreamImageSource)imageSource;

			if (!streamImageSource.IsEmpty)
			{
				Stream? stream = null;
				try
				{
					stream = await streamImageSource.GetStreamAsync(cancellationToken);

					var callback = new ImageLoaderCallback();

					PlatformInterop.LoadImageFromStream(imageView, stream, callback);

					var result = await callback.Result;

					stream?.Dispose();

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

			return null;
		}

		public override async Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			var streamImageSource = (IStreamImageSource)imageSource;

			if (!streamImageSource.IsEmpty)
			{
				Stream? stream = null;

				try
				{
					stream = await streamImageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);

					var drawableCallback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromStream(context, stream, drawableCallback);

					var result = await drawableCallback.Result.ConfigureAwait(false);

					stream?.Dispose();

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

			return null;
		}
	}
}