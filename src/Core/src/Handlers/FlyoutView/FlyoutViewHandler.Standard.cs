using System;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapFlyout(IFlyoutViewHandler handler, IFlyoutView flyoutView) { }
		public static void MapDetail(IFlyoutViewHandler handler, IFlyoutView flyoutView) { }
		public static void MapIsPresented(IFlyoutViewHandler handler, IFlyoutView flyoutView) { }
		public static void MapFlyoutBehavior(IFlyoutViewHandler handler, IFlyoutView flyoutView) { }
		public static void MapFlyoutWidth(IFlyoutViewHandler handler, IFlyoutView flyoutView) { }
		public static void MapIsGestureEnabled(IFlyoutViewHandler handler, IFlyoutView flyoutView) { }
	}
}
