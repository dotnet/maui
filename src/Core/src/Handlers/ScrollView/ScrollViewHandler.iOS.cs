using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScroll, UIScrollView>
	{
		protected override UIScrollView CreateNativeView()
		{
			return new UIScrollView();
		}

		public static void MapContent(ScrollViewHandler handler, IScroll scrollView)
		{
			handler.NativeView?.UpdateContent(scrollView, handler.MauiContext);
		}
	}
}