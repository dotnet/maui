using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Platform
{
	public static class FlyoutViewExtensions
	{
		public static void UpdateFlyout(this DrawerView platformDrawerView, IFlyoutView flyoutView, IMauiContext context)
		{
			platformDrawerView.Drawer = flyoutView.Flyout.ToPlatform(context);
		}

		public static void UpdateDetail(this DrawerView platformDrawerView, IFlyoutView flyoutView, IMauiContext context)
		{
			platformDrawerView.Content = flyoutView.Detail.ToPlatform(context);
		}

		public static void UpdateIsPresented(this DrawerView platformDrawerView, IFlyoutView flyoutView)
		{
			if (flyoutView.IsPresented)
				_ = platformDrawerView.OpenAsync(true);
			else
				_ = platformDrawerView.CloseAsync(true);
		}

		public static void UpdateFlyoutBehavior(this DrawerView platformDrawerView, IFlyoutView flyoutView)
		{
			platformDrawerView.DrawerBehavior = flyoutView.FlyoutBehavior.ToPlatform();

			if (platformDrawerView.DrawerBehavior == DrawerBehavior.Drawer)
			{
				_ = platformDrawerView.CloseAsync(false);
			}
		}

		public static void UpdateFlyoutWidth(this DrawerView platformDrawerView, IFlyoutView flyoutView)
		{
			platformDrawerView.DrawerWidth = flyoutView.FlyoutWidth.ToScaledPixel();
		}

		public static void UpdateIsGestureEnabled(this DrawerView platformDrawerView, IFlyoutView flyoutView)
		{
			platformDrawerView.IsGestureEnabled = flyoutView.IsGestureEnabled;
		}

		public static DrawerBehavior ToPlatform(this FlyoutBehavior behavior)
		{
			if (behavior == FlyoutBehavior.Disabled)
				return DrawerBehavior.Disabled;
			else if (behavior == FlyoutBehavior.Locked)
				return DrawerBehavior.Locked;
			else
				return DrawerBehavior.Drawer;
		}
	}
}
