using WSwipeItem = Microsoft.UI.Xaml.Controls.SwipeItem;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, WSwipeItem>
	{
		protected override WSwipeItem CreateNativeElement()
		{
			return new WSwipeItem();
		}

		public static void MapTextColor(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
			handler.PlatformView.UpdateTextColor(view);

		public static void MapCharacterSpacing(SwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapFont(SwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapText(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) 
		{
			handler.PlatformView.Text = view.Text;
		}

		public static void MapBackground(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
			handler.PlatformView.UpdateBackground(view.Background);

		public static void MapVisibility(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapSource(SwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.IconSource = view.Source?.ToIconSource(handler.MauiContext!);
		}

		protected override void ConnectHandler(WSwipeItem nativeView)
		{
			base.ConnectHandler(nativeView);
			PlatformView.Invoked += OnSwipeItemInvoked;
		}

		protected override void DisconnectHandler(WSwipeItem nativeView)
		{
			base.DisconnectHandler(nativeView);
			PlatformView.Invoked -= OnSwipeItemInvoked;
		}

		void OnSwipeItemInvoked(WSwipeItem sender, Microsoft.UI.Xaml.Controls.SwipeItemInvokedEventArgs args)
		{
			VirtualView.OnInvoked();
		}
	}
}
