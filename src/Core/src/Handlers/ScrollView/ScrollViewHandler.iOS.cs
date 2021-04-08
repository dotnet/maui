using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScroll, UIScrollView>
	{
		protected override UIScrollView CreateNativeView()
		{
			return new UIScrollView();
		}

		public static void MapContentSize(ScrollViewHandler handler, IScroll scrollView)
		{
			handler.NativeView?.UpdateContentSize(scrollView);
		}
	}
}