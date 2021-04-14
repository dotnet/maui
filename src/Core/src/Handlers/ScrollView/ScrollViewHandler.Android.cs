using AndroidX.Core.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScroll, NestedScrollView>
	{
		ScrollViewContainer? _scrollViewContainer;

		protected override NestedScrollView CreateNativeView() =>
			new NestedScrollView(Context);

		protected override void ConnectHandler(NestedScrollView nativeView)
		{
			_scrollViewContainer = new ScrollViewContainer(MauiContext, Context);
			nativeView.AddView(_scrollViewContainer);

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(NestedScrollView nativeView)
		{
			_scrollViewContainer?.Dispose();
			_scrollViewContainer = null;

			base.DisconnectHandler(nativeView);
		}

		public static void MapContent(ScrollViewHandler handler, IScroll scrollView)
		{
			handler.NativeView?.UpdateContent(scrollView, handler._scrollViewContainer);
		}
	}
}