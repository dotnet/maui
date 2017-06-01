using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	public sealed class ImageLoaderSourceHandler : IImageSourceHandler
	{
		public async Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			var imageLoader = imagesource as UriImageSource;
			Bitmap bitmap = null;
			if (imageLoader?.Uri != null)
			{
				using (Stream imageStream = await imageLoader.GetStreamAsync(cancelationToken).ConfigureAwait(false))
					bitmap =  await BitmapFactory.DecodeStreamAsync(imageStream).ConfigureAwait(false);
			}

			if (bitmap == null)
			{
				Log.Warning(nameof(ImageLoaderSourceHandler), "Could not retrieve image or image data was invalid: {0}", imageLoader);
			}

			return bitmap;
		}
	}
}