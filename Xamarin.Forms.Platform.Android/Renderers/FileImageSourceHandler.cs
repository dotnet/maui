using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.Widget;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	public sealed class FileImageSourceHandler : IImageSourceHandler, IImageViewHandler, IAnimationSourceHandler
	{
		// This is set to true when run under designer context
		internal static bool DecodeSynchronously
		{
			get;
			set;
		}

		public async Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			string file = ((FileImageSource)imagesource).File;
			Bitmap bitmap;
			if (File.Exists(file))
				bitmap = !DecodeSynchronously ? (await BitmapFactory.DecodeFileAsync(file).ConfigureAwait(false)) : BitmapFactory.DecodeFile(file);
			else
				bitmap = !DecodeSynchronously ? (await context.Resources.GetBitmapAsync(file, context).ConfigureAwait(false)) : context.Resources.GetBitmap(file, context);

			if (bitmap == null)
			{
				Internals.Log.Warning(nameof(FileImageSourceHandler), "Could not find image or image file was invalid: {0}", imagesource);
			}

			return bitmap;
		}

		public Task LoadImageAsync(ImageSource imagesource, ImageView imageView, CancellationToken cancellationToken = default(CancellationToken))
		{
			string file = ((FileImageSource)imagesource).File;
			if (File.Exists(file))
			{
				var uri = Uri.Parse(file);
				if (uri != null)
					imageView.SetImageURI(uri);
				else
					Log.Warning(nameof(FileImageSourceHandler), "Could not find image or image file was invalid: {0}", imagesource);
			}
			else
			{
				var drawable = ResourceManager.GetDrawable(imageView.Context, file);
				if (drawable != null)
					imageView.SetImageDrawable(drawable);
				else
					Log.Warning(nameof(FileImageSourceHandler), "Could not find image or image file was invalid: {0}", imagesource);
			}

			return Task.FromResult(true);
		}

		public Task<IFormsAnimationDrawable> LoadImageAnimationAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default, float scale = 1)
		{
			return FormsAnimationDrawable.LoadImageAnimationAsync(imagesource, context, cancelationToken);
		}
	}
}