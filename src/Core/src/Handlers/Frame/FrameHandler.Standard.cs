using System;

namespace Microsoft.Maui.Handlers
{
	public partial class FrameHandler : ViewHandler<IFrame, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapContent(IViewHandler handler, IFrame frame) { }
		public static void MapBackgroundColor(IViewHandler handler, IFrame frame) { }
		public static void MapBorderColor(IViewHandler handler, IFrame frame) { }
		public static void MapHasShadow(IViewHandler handler, IFrame frame) { }
		public static void MapCornerRadius(IViewHandler handler, IFrame frame) { }
	}
}