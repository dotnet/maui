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
					var bytes = await GetStreamBytesAsync(stream, cancellationToken);

					var callback = new ImageLoaderCallback();

					PlatformInterop.LoadImageFromBytes(imageView, bytes, callback);

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
					var bytes = await GetStreamBytesAsync(stream, cancellationToken).ConfigureAwait(false);

					var drawableCallback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromBytes(context, bytes, drawableCallback);

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

		static async Task<byte[]> GetStreamBytesAsync(Stream stream, CancellationToken cancellationToken)
		{
			using var memoryStream = new MemoryStream();
			await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
			return memoryStream.ToArray();
		}
	}
}