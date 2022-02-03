using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WImage = Microsoft.UI.Xaml.Controls.Image;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, Image>
	{
		protected override Image CreatePlatformView() => new Image();

		protected override void DisconnectHandler(Image nativeView)
		{
			base.DisconnectHandler(nativeView);
			SourceLoader.Reset();
		}

		public override bool NeedsContainer =>
			VirtualView?.Background != null ||
			base.NeedsContainer;

		public static void MapBackground(IImageHandler handler, IImage image)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.ToPlatform().UpdateBackground(image);
		}

		public static void MapAspect(IImageHandler handler, IImage image) =>
			handler.TypedPlatformView?.UpdateAspect(image);

		public static void MapIsAnimationPlaying(IImageHandler handler, IImage image) =>
			handler.TypedPlatformView?.UpdateIsAnimationPlaying(image);

		public static void MapSource(IImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapSourceAsync(IImageHandler handler, IImage image)
		{
			if (handler.PlatformView == null)
				return Task.CompletedTask;

			handler.TypedPlatformView.Clear();
			return handler.SourceLoader.UpdateImageSourceAsync();
		}

		void OnSetImageSource(ImageSource? obj)
		{
			PlatformView.Source = obj;
		}
	}
}