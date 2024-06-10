#nullable disable
using System;
using UIKit;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using PageUIStatusBarAnimation = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UIStatusBarAnimation;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		public static void MapPrefersLargeTitles(NavigationViewHandler handler, NavigationPage navigationPage) =>
			MapPrefersLargeTitles((INavigationViewHandler)handler, navigationPage);

		public static void MapIsNavigationBarTranslucent(NavigationViewHandler handler, NavigationPage navigationPage) =>
			MapIsNavigationBarTranslucent((INavigationViewHandler)handler, navigationPage);

		public static void MapHideNavigationBarSeparator(NavigationViewHandler handler, NavigationPage navigationPage) =>
			MapHideNavigationBarSeparator((INavigationViewHandler)handler, navigationPage);

		public static void MapPreferredStatusBarUpdateAnimation(NavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (navigationPage is not Page page)
			{
				return;
			}

			PageUIStatusBarAnimation animation = PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimation(
				page.OnThisPlatform());
			PlatformConfiguration.iOSSpecific.Page.SetPreferredStatusBarUpdateAnimation(navigationPage.OnThisPlatform(), animation);
		}

		public static void MapPrefersHomeIndicatorAutoHidden(NavigationViewHandler handler, NavigationPage navigationPage) =>
			MapPrefersHomeIndicatorAutoHidden((INavigationViewHandler)handler, navigationPage);

		public static void MapStatusBarTextColorMode(NavigationViewHandler viewHandler, NavigationPage navigationPage) =>
			MapStatusBarTextColorMode((INavigationViewHandler)viewHandler, navigationPage);

		public static void MapCurrentPage(NavigationViewHandler handler, NavigationPage navigationPage) =>
			MapCurrentPage((INavigationViewHandler)handler, navigationPage);

		public static void MapPrefersLargeTitles(INavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is IPlatformViewHandler nvh && nvh.ViewController is UINavigationController navigationController)
				Platform.NavigationPageExtensions.UpdatePrefersLargeTitles(navigationController, navigationPage);
		}

		public static void MapIsNavigationBarTranslucent(INavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is IPlatformViewHandler nvh && nvh.ViewController is UINavigationController navigationController)
				Platform.NavigationPageExtensions.UpdateIsNavigationBarTranslucent(navigationController, navigationPage);
		}

		public static void MapHideNavigationBarSeparator(INavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is IPlatformViewHandler nvh && nvh.ViewController is PlatformNavigationController navigationController)
			{
				navigationController.UpdateHideNavigationBarSeparator(navigationPage.OnThisPlatform().HideNavigationBarSeparator());
			}
		}

		public static void MapPrefersHomeIndicatorAutoHidden(INavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is IPlatformViewHandler nvh && nvh.ViewController is PlatformNavigationController navigationController)
			{
				navigationController.UpdateHomeIndicatorAutoHidden();
			}
		}

		public static void MapStatusBarTextColorMode(INavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is IPlatformViewHandler nvh && nvh.ViewController is UINavigationController navigationController && navigationPage.Toolbar != null)
			{
				navigationController.NavigationBar.UpdateBarTextColor(navigationPage.Toolbar);
				var barTextColor = navigationPage.BarTextColor;
				var statusBarColorMode = navigationPage.OnThisPlatform().GetStatusBarTextColorMode();
				navigationController.SetStatusBarStyle(statusBarColorMode, barTextColor);
			}
		}

		public static void MapCurrentPage(INavigationViewHandler handler, NavigationPage navigationPage)
		{
			if (handler is IPlatformViewHandler nvh && nvh.ViewController is PlatformNavigationController navigationController)
			{
				navigationController.ValidateNavBarExists(GetHasNavigationBar(navigationPage));
			}
		}

		// void UpdateLargeTitles()
		// 	{
		// 		var page = Child;
		// 		if (page != null && OperatingSystem.IsIOSVersionAtLeast(11))
		// 		{
		// 			var largeTitleDisplayMode = page.OnThisPlatform().LargeTitleDisplay();
		// 			switch (largeTitleDisplayMode)
		// 			{
		// 				case LargeTitleDisplayMode.Always:
		// 					NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Always;
		// 					break;
		// 				case LargeTitleDisplayMode.Automatic:
		// 					NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
		// 					break;
		// 				case LargeTitleDisplayMode.Never:
		// 					NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;
		// 					break;
		// 			}
		// 		}
		// 	}

		public static void MapToolbar(IElementHandler handler, IToolbarElement element)
		{
			if (handler.VirtualView is not IToolbarElement te || te.Toolbar == null)
			{
				return;
			}

			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by the base class.");

			// We don't need this return value but we need to realize the handler
			// otherwise the toolbar mapping doesn't work
			_ = te.Toolbar.ToHandler(handler.MauiContext);

			var navManager = handler.MauiContext.GetNavigationManager();
			navManager?.SetToolbarElement(te);
		}
	}
}