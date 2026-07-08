using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public sealed class ImageLoaderSourceHandler : IAnimationSourceHandler, IImageSourceHandler
	{
		public async Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			Bitmap bitmap = null;

			if (imagesource is IStreamImageSource imageLoader)
			{
				using var imageStream = await imageLoader.GetStreamAsync(cancelationToken).ConfigureAwait(false);
				if (imageStream != null)
				{
					bitmap = await BitmapFactory.DecodeStreamAsync(imageStream).ConfigureAwait(false);

					if (bitmap == null)
					{
						Application.Current?.FindMauiContext()?.CreateLogger<ImageLoaderSourceHandler>()?.LogWarning("Could not retrieve image or image data was invalid: {imagesource}", imagesource);
					}
				}
			}

			return bitmap;
		}

		public Task<IFormsAnimationDrawable> LoadImageAnimationAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default, float scale = 1)
		{
			return FormsAnimationDrawable.LoadImageAnimationAsync(imagesource, context, cancelationToken);
		}
	}
}