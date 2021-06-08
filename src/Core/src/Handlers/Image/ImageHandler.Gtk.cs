#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Handlers
{
	// https://developer.gnome.org/gtk3/stable/GtkImage.html
	
	public partial class ImageHandler : ViewHandler<IImage, Gtk.Image>
	{

		protected override Gtk.Image CreateNativeView()
		{
			var img = new Gtk.Image();

			return img;
		}

		[MissingMapper]
		public static void MapAspect(ImageHandler handler, IImage image) { }
		
		[MissingMapper]
		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image) { }

		public static void MapSource(ImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static async Task MapSourceAsync(ImageHandler handler, IImage image)
		{
			if (handler.NativeView == null)
				return;

			var token = handler._sourceManager.BeginLoad();

			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();
			var result = await handler.NativeView.UpdateSourceAsync(image, provider, token);

			handler._sourceManager.CompleteLoad(result);
		}
	}
}