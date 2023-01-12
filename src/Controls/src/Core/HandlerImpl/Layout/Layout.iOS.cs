#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) =>
			UpdateInputTransparent(handler, layout);

		public static void MapInputTransparent(ILayoutHandler handler, Layout layout) =>
			UpdateInputTransparent(handler, layout);

		static void MapInputTransparent(IViewHandler handler, IView layout) =>
			UpdateInputTransparent(handler, layout);

		static void UpdateInputTransparent(IViewHandler handler, IView layout)
		{
			if (handler.PlatformView is UIKit.UIView uiView)
				uiView.UpdateInputTransparent(handler, layout);

			if (layout is Layout l)
			{
				l.UpdateDescendantInputTransparent();
			}
		}
	}
}
