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

					if (stream.CanSeek)
					{
						stream.Position = 0;
					}
					// Convert stream to byte array to avoid AndroidMarshalMethod InputStreamAdapter issues in release builds
					byte[] buffer;
					int length;
					using (var memoryStream = new MemoryStream())
					{
						await stream.CopyToAsync(memoryStream, cancellationToken);
						buffer = memoryStream.GetBuffer();
						length = (int)memoryStream.Length;
					}

					// Decode the byte array into a Bitmap
					var bitmap = global::Android.Graphics.BitmapFactory.DecodeByteArray(buffer, 0, length);
					if (bitmap == null)
					{
						Logger?.LogWarning("Failed to decode image from stream data.");
						return null;
					}

					// Set the bitmap to the ImageView
					imageView.SetImageBitmap(bitmap);

					// Convert Bitmap to Drawable for consistent return type
					var drawable = new global::Android.Graphics.Drawables.BitmapDrawable(imageView.Resources, bitmap);

					stream?.Dispose();

					// Create a result object with proper dispose handling
					return new ImageSourceServiceLoadResult(() =>
					{
						bitmap?.Dispose();
						drawable?.Dispose();
					});
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