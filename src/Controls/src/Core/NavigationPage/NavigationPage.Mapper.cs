#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;
#if IOS || MACCATALYST
using Microsoft.Maui.Controls.Handlers.Compatibility;
#endif

namespace Microsoft.Maui.Controls
{
	public partial class NavigationPage
	{
		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.NavigationPage legacy behaviors
#if IOS || MACCATALYST
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.PrefersLargeTitlesProperty.PropertyName, MapPrefersLargeTitles);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(NavigationPage.BarBackgroundColorProperty.PropertyName, MapBarBackground);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(NavigationPage.BarBackgroundProperty.PropertyName, MapBarBackground);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(NavigationPage.BarTextColorProperty.PropertyName, MapBarTextColor);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.HideNavigationBarSeparatorProperty.PropertyName, MapHideNavigationBarSeparator);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.StatusBarTextColorModeProperty.PropertyName, MapStatusBarTextColorMode);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName, MapPrefersHomeIndicatorAutoHidden);
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName, MapPrefersStatusBarHidden);

#pragma warning disable CS0618 // Type or member is obsolete
			NavigationViewHandler.Mapper.ReplaceMapping<NavigationPage, NavigationViewHandler>(PlatformConfiguration.iOSSpecific.NavigationPage.IsNavigationBarTranslucentProperty.PropertyName, MapIsNavigationBarTranslucent);
#pragma warning restore CS0618 // Type or member is obsolete

			// Wire handler callbacks for Controls-layer integration.
			// These connect the Core-layer NavigationViewHandler to Controls-layer
			// NavigationPage features (toolbar, lifecycle, nav bar type).
			NavigationViewHandler.NavigationBarType = typeof(Handlers.Compatibility.MauiNavigationBar);
			NavigationViewHandler.CreateViewControllerForPage = NavigationViewHandlerToolbarHelper.CreateViewControllerForPage;

			NavigationViewHandler.OnNativePopCompleted = (navigationView, poppedPage) =>
			{
				if (navigationView is NavigationPage navPage && poppedPage is Page page)
				{
					navPage.SendNavigatedFromHandler(page, NavigationType.Pop);
				}
			};

			NavigationViewHandler.OnNavigationControllerDidAppear = (navigationView) =>
			{
				if (navigationView is VisualElement ve)
				{
					ve.RefreshPlatformLoadedStatus();
				}
				// Fire SendAppearing on the NavigationPage element, matching
				// NavigationRenderer.ViewDidAppear behavior. SendAppearing has
				// a _hasAppeared guard so it won't double-fire during push animations.
				// This is critical for tab switches: UITabBarController calls ViewDidAppear
				// on the newly selected tab's UINavigationController, which must propagate
				// Appearing to the NavigationPage and its CurrentPage.
				(navigationView as Page)?.SendAppearing();
			};

			NavigationViewHandler.OnNavigationControllerDidDisappear = (navigationView) =>
			{
				// Fire SendDisappearing on the NavigationPage element, matching
				// NavigationRenderer.ViewDidDisappear behavior. This is critical for
				// tab switches: when the user switches away from this tab, the
				// NavigationPage and its CurrentPage must fire Disappearing.
				(navigationView as Page)?.SendDisappearing();
			};
#endif
		}
	}
}
