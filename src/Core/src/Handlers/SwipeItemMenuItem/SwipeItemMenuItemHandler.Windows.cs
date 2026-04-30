using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WSwipeItem = Microsoft.UI.Xaml.Controls.SwipeItem;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, WSwipeItem>
	{
		protected override WSwipeItem CreatePlatformElement()
		{
			return new WSwipeItem();
		}

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
			handler.PlatformView.UpdateTextColor(view);

		public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			handler.PlatformView.Text = view.Text;
		}

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) =>
			handler.PlatformView.UpdateBackground(view.Background);

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view)
		{
			// WinUI SwipeItem does not support a Visibility property, so we need to
			// rebuild the parent SwipeView's swipe items to reflect the visibility change.
			var swipeView = view.Parent?.Parent as ISwipeView;
			if (swipeView?.Handler is ISwipeViewHandler swipeViewHandler)
			{
				if (swipeView.LeftItems?.Contains(view) == true)
					swipeViewHandler.UpdateValue(nameof(ISwipeView.LeftItems));
				else if (swipeView.RightItems?.Contains(view) == true)
					swipeViewHandler.UpdateValue(nameof(ISwipeView.RightItems));
				else if (swipeView.TopItems?.Contains(view) == true)
					swipeViewHandler.UpdateValue(nameof(ISwipeView.TopItems));
				else if (swipeView.BottomItems?.Contains(view) == true)
					swipeViewHandler.UpdateValue(nameof(ISwipeView.BottomItems));
			}
		}

		protected override void ConnectHandler(WSwipeItem platformView)
		{
			base.ConnectHandler(platformView);
			PlatformView.Invoked += OnSwipeItemInvoked;
		}

		protected override void DisconnectHandler(WSwipeItem platformView)
		{
			base.DisconnectHandler(platformView);
			PlatformView.Invoked -= OnSwipeItemInvoked;
		}

		void OnSwipeItemInvoked(WSwipeItem sender, Microsoft.UI.Xaml.Controls.SwipeItemInvokedEventArgs args)
		{
			VirtualView.OnInvoked();
		}

		partial class SwipeItemMenuItemImageSourcePartSetter
		{
			public override void SetImageSource(ImageSource? platformImage)
			{
				if (Handler?.PlatformView is not WSwipeItem button)
					return;

				button.IconSource = new ImageIconSource { ImageSource = platformImage };
			}
		}
	}
}
