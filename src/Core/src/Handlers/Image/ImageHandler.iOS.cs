using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, UIImageView>
	{
		protected override UIImageView CreateNativeView() => new UIImageView();

		[MissingMapper]
		public static void MapAspect(ImageHandler handler, IImage image)
		{
		}

		[MissingMapper]
		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image)
		{
		}

		[MissingMapper]
		public static void MapSource(ImageHandler handler, IImage image)
		{
		}
	}
}