using System.Runtime.CompilerServices;
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

		protected override void DisconnectHandler(Image platformView)
		{
			base.DisconnectHandler(platformView);
			SourceLoader.Reset();
		}

		public override bool NeedsContainer => true;

		protected override void SetupContainer()
		{
			base.SetupContainer();

			// VerticalAlignment only works when the child's Height is Auto
			PlatformView.Height = double.NaN;

			MapHeight(this, VirtualView);
			MapWidth(this, VirtualView);
		}

		protected override void RemoveContainer()
		{
			base.RemoveContainer();

			MapHeight(this, VirtualView);
			MapWidth(this, VirtualView);
		}

		public static void MapHeight(IImageHandler handler, IImage view) =>
			// VerticalAlignment only works when the container's Height is set and the child's Height is Auto. The child's Height
			// is set to Auto when the container is introduced
			(handler.ContainerView as FrameworkElement)!.Height = view.Height;

		public static void MapWidth(IImageHandler handler, IImage view) =>
			(handler.ContainerView as FrameworkElement)!.Width = view.Width;

		public static void MapBackground(IImageHandler handler, IImage image)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.ToPlatform().UpdateBackground(image);
		}

		public static void MapAspect(IImageHandler handler, IImage image)
		{
			handler.UpdateValue(nameof(IViewHandler.ContainerView));
			handler.PlatformView?.UpdateAspect(image, (handler.ContainerView as WrapperView)!);
		}

		public static void MapIsAnimationPlaying(IImageHandler handler, IImage image) =>
			handler.PlatformView?.UpdateIsAnimationPlaying(image);

		public static void MapSource(IImageHandler handler, IImage image) =>
			MapSourceAsync(handler, image).FireAndForget(handler);

		public static Task MapSourceAsync(IImageHandler handler, IImage image) =>
			handler.SourceLoader.UpdateImageSourceAsync();

		partial class ImageImageSourcePartSetter
		{
			public override void SetImageSource(ImageSource? platformImage)
			{
				if (Handler?.PlatformView is not WImage image)
					return;

				image.Source = platformImage;
			}
		}
	}
}