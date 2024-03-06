using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public sealed class StreamImagesourceHandler : IImageSourceHandler, IAnimationSourceHandler
	{
		public async Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			var streamsource = imagesource as StreamImageSource;
			Bitmap bitmap = null;
			if (streamsource?.Stream != null)
			{
				using (Stream stream = await ((IStreamImageSource)streamsource).GetStreamAsync(cancelationToken).ConfigureAwait(false))
				{
					bitmap = await BitmapFactory.DecodeStreamAsync(stream).ConfigureAwait(false);
				}
			}

			if (bitmap == null)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<ImageLoaderSourceHandler>()?.LogWarning("Image data was invalid: {streamsource}", streamsource);
			}

			return bitmap;
		}

		public Task<IFormsAnimationDrawable> LoadImageAnimationAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default, float scale = 1)
		{
			return FormsAnimationDrawable.LoadImageAnimationAsync(imagesource, context, cancelationToken);
		}
	}
}