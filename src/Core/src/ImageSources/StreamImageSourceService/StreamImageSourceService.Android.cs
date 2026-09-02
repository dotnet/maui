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
				try
				{
					byte[] bytes;
					using (var stream = await streamImageSource.GetStreamAsync(cancellationToken))
						bytes = await GetStreamBytesAsync(stream, cancellationToken);

					var callback = new ImageLoaderCallback();

					PlatformInterop.LoadImageFromBytes(imageView, bytes, callback);

					return await callback.Result;
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image stream.");
					throw;
				}
			}

			return null;
		}

		public override async Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
		{
			var streamImageSource = (IStreamImageSource)imageSource;

			if (!streamImageSource.IsEmpty)
			{
				try
				{
					byte[] bytes;
					using (var stream = await streamImageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false))
						bytes = await GetStreamBytesAsync(stream, cancellationToken).ConfigureAwait(false);

					var drawableCallback = new ImageLoaderResultCallback();

					PlatformInterop.LoadImageFromBytes(context, bytes, drawableCallback);

					return await drawableCallback.Result.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					Logger?.LogWarning(ex, "Unable to load image stream.");
					throw;
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