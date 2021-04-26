using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, Image>
	{
		protected override Image CreateNativeView() => new Image();

		protected override void DisconnectHandler(ImageView nativeView)
		{
			base.DisconnectHandler(nativeView);

			_sourceManager.CompleteLoad(null);
		}

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