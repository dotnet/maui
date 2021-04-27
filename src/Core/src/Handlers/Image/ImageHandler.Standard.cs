#nullable enable
using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapAspect(ImageHandler handler, IImage image) { }
		public static void MapIsAnimationPlaying(ImageHandler handler, IImage image) { }
		public static void MapSource(ImageHandler handler, IImage image) { }
	}
}