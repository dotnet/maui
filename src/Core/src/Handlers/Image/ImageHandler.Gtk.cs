#nullable enable
using System;
using System.Threading.Tasks;
using Gdk;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{

	public partial class ImageHandler : ViewHandler<IImage, ImageView>
	{

		protected override ImageView CreatePlatformView()
		{
			var img = new ImageView();

			return img;
		}

		[MissingMapper]
		public static void MapAspect(IImageHandler handler, IImage image) { }

		[MissingMapper]
		public static void MapIsAnimationPlaying(IImageHandler handler, IImage image) { }

		public static void MapSource(IImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static async Task MapSourceAsync(IImageHandler handler, IImage image)
		{
			if (handler.PlatformView == null)
				return;

			await handler.SourceLoader.UpdateImageSourceAsync();

		}

		void OnSetImageSource(Pixbuf? obj)
		{
			PlatformView.Image = obj;
		}

	}

}