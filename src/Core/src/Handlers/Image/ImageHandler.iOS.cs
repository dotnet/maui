using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, UIImageView>
	{
		protected override UIImageView CreateNativeView() => new UIImageView();

		public static void MapAspect(ImageHandler handler, IImage image)
		{
			handler.NativeView?.UpdateAspect(image);
		}

		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image)
		{
			handler.NativeView?.UpdateIsAnimationPlaying(image);
		}

		public static async void MapSource(ImageHandler handler, IImage image) =>
			await MapSourceAsync(handler, image);

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