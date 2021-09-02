using System;

namespace Microsoft.Maui.Handlers
{
	public partial class BoxViewHandler : ViewHandler<IBoxView, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapColor(IViewHandler handler, IBoxView boxView) { }
		public static void MapCornerRadius(IViewHandler handler, IBoxView boxView) { }
	}
}