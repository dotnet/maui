using AndroidX.Core.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScroll, NestedScrollView>
	{
		protected override NestedScrollView CreateNativeView()
		{
			return new NestedScrollView(Context);
		}

		public static void MapContentSize(ScrollViewHandler handler, IScroll scrollView)
		{
			handler.NativeView?.UpdateContentSize(scrollView);
		}
	}
}