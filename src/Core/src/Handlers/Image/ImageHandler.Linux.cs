#nullable enable
using System;

namespace Microsoft.Maui.Handlers
{
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
		
		[MissingMapper]
		public static void MapSource(ImageHandler handler, IImage image) { }
	}
}