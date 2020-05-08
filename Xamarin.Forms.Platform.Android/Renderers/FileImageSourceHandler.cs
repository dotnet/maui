using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Widget;
using Android.Net;
using Xamarin.Forms.Internals;
using Android.Graphics.Drawables;

namespace Xamarin.Forms.Platform.Android
{
	public sealed class FileImageSourceHandler : IImageSourceHandler, IImageViewHandler, IAnimationSourceHandler
    {
		// This is set to true when run under designer context
		internal static bool DecodeSynchronously {
			get;
			set;
		}

		public async Task<Bitmap> LoadImageAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default(CancellationToken))
		{
			string file = ((FileImageSource)imagesource).File;
			Bitmap bitmap;
			if (File.Exists (file))
				bitmap = !DecodeSynchronously ? (await BitmapFactory.DecodeFileAsync (file).ConfigureAwait (false)) : BitmapFactory.DecodeFile (file);
			else
				bitmap = !DecodeSynchronously ? (await context.Resources.GetBitmapAsync (file, context).ConfigureAwait (false)) : context.Resources.GetBitmap (file, context);

			if (bitmap == null)
			{
				Internals.Log.Warning(nameof(FileImageSourceHandler), "Could not find image or image file was invalid: {0}", imagesource);
			}

			return bitmap;
		}

		public Task LoadImageAsync(ImageSource imagesource, ImageSource placeholder, ImageView imageView, CancellationToken cancellationToken = default(CancellationToken))
		{
			string file = ((FileImageSource)imagesource).File;
			if (File.Exists(file))
			{
				var uri = Uri.Parse(file);
				if (uri != null)
					imageView.SetImageURI(uri);
				else
				{
					Log.Warning(nameof(FileImageSourceHandler), "Could not find image or image file was invalid: {0}", imagesource);
					return SetImagePlaceholderAsync(imageView, placeholder);
				}
			}
			else
			{
				var drawable = ResourceManager.GetDrawable(imageView.Context, file);
				if (drawable != null)
					imageView.SetImageDrawable(drawable);
				else
				{
					Log.Warning(nameof(FileImageSourceHandler), "Could not find image or image file was invalid: {0}", imagesource);
					return SetImagePlaceholderAsync(imageView, placeholder);
				}
			}

			return Task.FromResult(true);
		}

		Task SetImagePlaceholderAsync(ImageView imageView, ImageSource placeholder, CancellationToken cancellation = default(CancellationToken))
		{
			string file = ((FileImageSource)placeholder).File;
			var drawable = ResourceManager.GetDrawable(imageView.Context, file);
			if (drawable != null)
				imageView.SetImageDrawable(drawable);
			else
			{
				Log.Warning(nameof(FileImageSourceHandler), "Could not find image or image file was invalid: {0}", placeholder);
			}
			return Task.CompletedTask;
    }
    
		public Task<IFormsAnimationDrawable> LoadImageAnimationAsync(ImageSource imagesource, Context context, CancellationToken cancelationToken = default, float scale = 1)
		{
			return FormsAnimationDrawable.LoadImageAnimationAsync(imagesource, context, cancelationToken);
		}
	}
}