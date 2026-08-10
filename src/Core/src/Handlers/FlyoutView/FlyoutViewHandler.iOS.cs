using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, UIView>, IFlyoutContainerDelegate
	{
		internal FlyoutContainerManager? _manager;
		FlyoutContainerViewController? _containerVC;

		protected override UIView CreatePlatformView()
		{
			_manager = new FlyoutContainerManager(this);
			_containerVC = new FlyoutContainerViewController(_manager);

			// Force view load so SetupContainerViews is called
			_containerVC.LoadViewIfNeeded();
			return _containerVC.View!;
		}

		protected override void ConnectHandler(UIView platformView)
		{
			base.ConnectHandler(platformView);

			// Set the ViewController so this handler participates in the VC hierarchy
			ViewController = _containerVC;
		}

		protected override void DisconnectHandler(UIView platformView)
		{
			// TearDown() before OnHandlerDisconnected so the unsubscribe below runs last and sticks.
			_manager?.TearDown();
			_manager = null;
			_containerVC = null;
			ViewController = null;

			if (VirtualView is not null)
			{
				ControlsConfiguration?.OnHandlerDisconnected(VirtualView);
			}

			base.DisconnectHandler(platformView);
		}


		public static void MapFlyout(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler h && h._manager is { } manager)
			{
				var flyoutVC = flyoutView.Flyout?.ToUIViewController(handler.MauiContext!);
				manager.SetFlyoutViewController(flyoutVC);
			}
		}

		public static void MapDetail(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler h && h._manager is { } manager)
			{
				var detailVC = flyoutView.Detail?.ToUIViewController(handler.MauiContext!);
				manager.SetDetailViewController(detailVC);
			}
		}

		public static void MapIsPresented(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler h && h._manager is { } manager)
			{
				manager.UpdateIsPresented(flyoutView.IsPresented, animated: true);
			}
		}

		public static void MapFlyoutBehavior(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler h && h._manager is { } manager)
			{
				manager.UpdateFlyoutBehavior(flyoutView.FlyoutBehavior);
			}
		}

		public static void MapFlyoutWidth(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler h && h._manager is { } manager)
			{
				manager.UpdateFlyoutWidth(flyoutView.FlyoutWidth);
			}
		}

		public static void MapIsGestureEnabled(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler h && h._manager is { } manager)
			{
				manager.UpdateIsGestureEnabled(flyoutView.IsGestureEnabled);
			}
		}


		void IFlyoutContainerDelegate.OnPresentedChangedByGesture(bool isPresented)
		{
			if (VirtualView is null)
			{
				return;
			}

			ControlsConfiguration?.OnPresentedChangedByGesture(VirtualView, isPresented);
		}

		void IFlyoutContainerDelegate.OnLayoutBoundsChanged(Rect flyoutBounds, Rect detailBounds)
		{
			if (VirtualView is null)
			{
				return;
			}

			ControlsConfiguration?.OnLayoutBoundsChanged(VirtualView, flyoutBounds, detailBounds);
		}

		void IFlyoutContainerDelegate.OnLeftBarButtonNeedsUpdate()
		{
			if (VirtualView is null)
			{
				return;
			}

			ControlsConfiguration?.OnLeftBarButtonNeedsUpdate(VirtualView);
		}

		void IFlyoutContainerDelegate.OnViewDidAppear()
		{
			// Lifecycle: page appeared — let framework handle Appearing event
		}

		void IFlyoutContainerDelegate.OnViewWillDisappear()
		{
			// Lifecycle: page disappearing — let framework handle Disappearing event
		}

		bool IFlyoutContainerDelegate.GetCurrentIsPresented()
		{
			return VirtualView?.IsPresented ?? false;
		}

		bool IFlyoutContainerDelegate.GetIgnoreSafeArea()
		{
			return VirtualView is ISafeAreaView sav && sav.IgnoreSafeArea;
		}
	}
}
