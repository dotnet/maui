using System;
using System.Linq;
using Microsoft.Maui.Graphics;
using WSwipeControl = Microsoft.UI.Xaml.Controls.SwipeControl;
using WSwipeItem = Microsoft.UI.Xaml.Controls.SwipeItem;
using WSwipeItems = Microsoft.UI.Xaml.Controls.SwipeItems;

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
			_ = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");


			if (handler.VirtualView.PresentedContent is not IView presentedView)
				return;

			handler.PlatformView.Content = presentedView.ToPlatform(handler.MauiContext);
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
			handler.PlatformView.Close();
		}


		protected override void ConnectHandler(WSwipeControl platformView)
		{
			base.ConnectHandler(platformView);
			PlatformView.Loaded += OnLoaded;
		}

		protected override void DisconnectHandler(WSwipeControl platformView)
		{
			base.DisconnectHandler(platformView);
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
			if (!handler.PlatformView.IsLoaded)
				return;

			UpdateSwipeItems(SwipeDirection.Left, handler, view, (items) => handler.PlatformView.LeftItems = items, view.LeftItems, handler.PlatformView.LeftItems);
		}

		public static void MapTopItems(ISwipeViewHandler handler, ISwipeView view)
		{
			UpdateSwipeItems(SwipeDirection.Up, handler, view, (items) => handler.PlatformView.TopItems = items, view.TopItems, handler.PlatformView.TopItems);
		}

		public static void MapRightItems(ISwipeViewHandler handler, ISwipeView view)
		{
			if (!handler.PlatformView.IsLoaded)
				return;

			UpdateSwipeItems(SwipeDirection.Right, handler, view, (items) => handler.PlatformView.RightItems = items, view.RightItems, handler.PlatformView.RightItems);
		}

		public static void MapBottomItems(ISwipeViewHandler handler, ISwipeView view)
		{
			UpdateSwipeItems(SwipeDirection.Down, handler, view, (items) => handler.PlatformView.BottomItems = items, view.BottomItems, handler.PlatformView.BottomItems);
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
				if (wSwipeItems != null || items.Count > 0)
					setSwipeItems(items);
			}
			catch
			{
				//https://github.com/microsoft/microsoft-ui-xaml/issues/6571			
			}

			UpdateSwipeMode(swipeItems, view, handler.PlatformView);
			UpdateSwipeBehaviorOnInvoked(swipeItems, view, handler.PlatformView);
		}

		static void UpdateSwipeMode(ISwipeItems swipeItems, ISwipeView swipeView, WSwipeControl swipeControl)
		{
			var windowsSwipeItems = GetWindowsSwipeItems(swipeItems, swipeView, swipeControl);

			if (windowsSwipeItems != null)
				windowsSwipeItems.Mode = swipeItems.Mode.ToPlatform();
		}

		static void UpdateSwipeBehaviorOnInvoked(ISwipeItems swipeItems, ISwipeView swipeView, WSwipeControl swipeControl)
		{
			var windowsSwipeItems = GetWindowsSwipeItems(swipeItems, swipeView, swipeControl);

			if (windowsSwipeItems != null)
				foreach (var windowSwipeItem in windowsSwipeItems.ToList())
					windowSwipeItem.BehaviorOnInvoked = swipeItems.SwipeBehaviorOnInvoked.ToPlatform();
		}

		static bool IsValidSwipeItems(ISwipeItems? swipeItems)
		{
			return swipeItems?.Count > 0;
		}

		static WSwipeItems CreateSwipeItems(SwipeDirection swipeDirection, ISwipeViewHandler handler)
		{
			var swipeItems = new WSwipeItems();
			var swipeView = handler.VirtualView;

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

			swipeItems.Mode = items.Mode.ToPlatform();

			foreach (var item in items)
			{
				if (CanAddSwipeItems(swipeItems) && item is ISwipeItemMenuItem &&
					item.ToHandler(handler.MauiContext!).PlatformView is WSwipeItem swipeItem)
				{
					swipeItem.BehaviorOnInvoked = items.SwipeBehaviorOnInvoked.ToPlatform();
					swipeItems.Add(swipeItem);
				}
			}

			return swipeItems;
		}

		static bool CanAddSwipeItems(WSwipeItems swipeItems)
		{
			// On Windows, the SwipeItems can only contain one single SwipeItem using the Execute mode.
			if (swipeItems.Mode == UI.Xaml.Controls.SwipeMode.Execute && swipeItems.Count > 0)
				return false;

			return true;
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
