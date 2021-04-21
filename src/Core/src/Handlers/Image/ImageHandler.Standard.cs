using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
		
		public static void MapAspect(IViewHandler handler, IImage image) { }
	}
}