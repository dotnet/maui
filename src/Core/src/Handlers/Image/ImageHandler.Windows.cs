using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, Image>
	{
		protected override Image CreateNativeView() => new Image();

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