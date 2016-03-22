using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;

namespace Xamarin.Forms.Platform.Android
{
	public sealed class FileImageSourceHandler : IImageSourceHandler
	{
		public async Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			string file = ((FileImageSource)imagesource).File;
			return await (File.Exists(file) ? BitmapFactory.DecodeFileAsync(file) : context.Resources.GetBitmapAsync(file)).ConfigureAwait(false);
		}
	}
}