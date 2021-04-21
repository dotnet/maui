using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, UIImageView>
	{
		protected override UIImageView CreateNativeView()
		{
			return new UIImageView();
		}
		
		public static void MapAspect(ImageHandler handler, IImage image)
		{
			handler.NativeView?.UpdateAspect(image);
		}
	}
}