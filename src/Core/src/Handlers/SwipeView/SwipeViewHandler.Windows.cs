using System;
using System.Linq;
using WSwipeControl = Microsoft.UI.Xaml.Controls.SwipeControl;
using WSwipeItems = Microsoft.UI.Xaml.Controls.SwipeItems;
using WSwipeItem = Microsoft.UI.Xaml.Controls.SwipeItem;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeViewHandler : ViewHandler<ISwipeView, WSwipeControl>
	{
		protected override WSwipeControl CreatePlatformView() => new();

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		}

		public static void MapContent(ISwipeViewHandler handler, ISwipeView view)
		{
			_ = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = handler.TypedVirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");


			if (handler.TypedVirtualView.PresentedContent is not IView presentedView)
				return;

			handler.TypedPlatformView.Content = presentedView.ToPlatform(handler.MauiContext);
		}

		public static void MapSwipeTransitionMode(ISwipeViewHandler handler, ISwipeView swipeView)
		{
		}

		public static void MapRequestOpen(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			if (args is not SwipeViewOpenRequest)
			{
				return;
			}
		}

		public static void MapRequestClose(ISwipeViewHandler handler, ISwipeView swipeView, object? args)
		{
			handler.TypedPlatformView.Close();
		}


		protected override void ConnectHandler(WSwipeControl nativeView)
		{
			base.ConnectHandler(nativeView);
			PlatformView.Loaded += OnLoaded;
		}

		protected override void DisconnectHandler(WSwipeControl nativeView)
		{
			base.DisconnectHandler(nativeView);
			PlatformView.Loaded -= OnLoaded;
		}

		void OnLoaded(object sender, UI.Xaml.RoutedEventArgs e)
		{
			if (!PlatformView.IsLoaded)
				return;
				
			// Setting the Left/Right Items before the view has loaded causes the Swipe Control
			// to crash on the first layout pass. So we wait until the control has been loaded
			// before propagating our Left/Right Items
			UpdateValue(nameof(ISwipeView.LeftItems));
			UpdateValue(nameof(ISwipeView.RightItems));
			PlatformView.Loaded -= OnLoaded;
		}

		public static void MapLeftItems(ISwipeViewHandler handler, ISwipeView view)
		{
			if (!handler.TypedPlatformView.IsLoaded)
				return;

			UpdateSwipeItems(SwipeDirection.Left, handler, view, (items) => handler.TypedPlatformView.LeftItems = items, view.LeftItems, handler.TypedPlatformView.LeftItems);
		}

		public static void MapTopItems(ISwipeViewHandler handler, ISwipeView view)
		{
			UpdateSwipeItems(SwipeDirection.Up, handler, view, (items) => handler.TypedPlatformView.TopItems = items, view.TopItems, handler.TypedPlatformView.TopItems);
		}

		public static void MapRightItems(ISwipeViewHandler handler, ISwipeView view)
		{
			if (!handler.TypedPlatformView.IsLoaded)
				return;

			UpdateSwipeItems(SwipeDirection.Right, handler, view, (items) => handler.TypedPlatformView.RightItems = items, view.RightItems, handler.TypedPlatformView.RightItems);
		}

		public static void MapBottomItems(ISwipeViewHandler handler, ISwipeView view)
		{
			UpdateSwipeItems(SwipeDirection.Down, handler, view, (items) => handler.TypedPlatformView.BottomItems = items, view.BottomItems, handler.TypedPlatformView.BottomItems);
		}

		static void UpdateSwipeItems(
			SwipeDirection swipeDirection,
			ISwipeViewHandler handler,
			ISwipeView view,
			Action<WSwipeItems> setSwipeItems,
			ISwipeItems swipeItems,
			WSwipeItems wSwipeItems)
		{
			var items = CreateSwipeItems(swipeDirection, handler);

			try
			{
				if(wSwipeItems != null || items.Count > 0)
					setSwipeItems(items);
			}
			catch
			{
				//https://github.com/microsoft/microsoft-ui-xaml/issues/6571			
			}

			UpdateSwipeMode(swipeItems, view, handler.TypedPlatformView);
			UpdateSwipeBehaviorOnInvoked(swipeItems, view, handler.TypedPlatformView);
		}

		static void UpdateSwipeMode(ISwipeItems swipeItems, ISwipeView swipeView, WSwipeControl swipeControl)
		{
			var windowsSwipeItems = GetWindowsSwipeItems(swipeItems, swipeView, swipeControl);

			if (windowsSwipeItems != null)
				windowsSwipeItems.Mode = swipeItems.Mode.ToNative();
		}

		static void UpdateSwipeBehaviorOnInvoked(ISwipeItems swipeItems, ISwipeView swipeView, WSwipeControl swipeControl)
		{
			var windowsSwipeItems = GetWindowsSwipeItems(swipeItems, swipeView, swipeControl);

			if (windowsSwipeItems != null)
				foreach (var windowSwipeItem in windowsSwipeItems.ToList())
					windowSwipeItem.BehaviorOnInvoked = swipeItems.SwipeBehaviorOnInvoked.ToNative();
		}

		static bool IsValidSwipeItems(ISwipeItems? swipeItems)
		{
			return swipeItems?.Count > 0;
		}

		static WSwipeItems CreateSwipeItems(SwipeDirection swipeDirection, ISwipeViewHandler handler)
		{
			var swipeItems = new WSwipeItems();
			var swipeView = handler.TypedVirtualView;

			ISwipeItems? items = null;

			switch (swipeDirection)
			{
				case SwipeDirection.Left:
					items = swipeView.LeftItems;
					break;
				case SwipeDirection.Right:
					items = swipeView.RightItems;
					break;
				case SwipeDirection.Up:
					items = swipeView.TopItems;
					break;
				case SwipeDirection.Down:
					items = swipeView.BottomItems;
					break;
			}

			if (items == null)
				return swipeItems;

			swipeItems.Mode = items.Mode.ToNative();

			foreach (var item in items)
			{
				if (item is ISwipeItemMenuItem &&
					item.ToHandler(handler.MauiContext!).PlatformView is WSwipeItem swipeItem)
				{
					swipeItem.BehaviorOnInvoked = items.SwipeBehaviorOnInvoked.ToNative();
					swipeItems.Add(swipeItem);
				}
			}

			return swipeItems;
		}

		static WSwipeItems? GetWindowsSwipeItems(ISwipeItems swipeItems, ISwipeView swipeView, WSwipeControl swipeControl)
		{
			if (swipeItems == swipeView.LeftItems)
				return swipeControl.LeftItems;

			if (swipeItems == swipeView.RightItems)
				return swipeControl.RightItems;

			if (swipeItems == swipeView.TopItems)
				return swipeControl.TopItems;

			if (swipeItems == swipeView.BottomItems)
				return swipeControl.BottomItems;

			return null;
		}
	}
}
