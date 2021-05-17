#nullable enable
using System;
using System.Threading.Tasks;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : EViewHandler<IImage, Image>
	{
		protected override Image CreateNativeView() => new Image(NativeParent);

		protected override void DisconnectHandler(Image nativeView)
		{
			base.DisconnectHandler(nativeView);
			_sourceManager.Reset();
		}

		public static void MapAspect(ImageHandler handler, IImage image) =>
			handler.NativeView?.UpdateAspect(image);

		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image) =>
			handler.NativeView?.UpdateIsAnimationPlaying(image);

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