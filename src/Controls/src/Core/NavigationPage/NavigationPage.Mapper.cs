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

			// Wire all Controls-layer integration in one place.
			// This connects the Core-layer NavigationViewHandler to Controls-layer
			// NavigationPage features (toolbar, lifecycle, nav bar type).
			NavigationViewHandler.ControlsConfiguration = new NavigationViewHandlerControlsConfiguration
			{
				NavigationBarType = typeof(Handlers.Compatibility.MauiNavigationBar),
				CreateViewControllerForPage = NavigationViewHandlerToolbarHelper.CreateViewControllerForPage,
				OnNativePopCompleted = (navigationView, poppedPage) =>
				{
					if (navigationView is NavigationPage navPage && poppedPage is Page page)
					{
						navPage.SendNavigatedFromHandler(page, NavigationType.Pop);
					}
				},
				OnControllerAppeared = (navigationView) =>
				{
					if (navigationView is VisualElement ve)
					{
						ve.RefreshPlatformLoadedStatus();
					}
					(navigationView as Page)?.SendAppearing();
				},
				OnControllerDisappeared = (navigationView) =>
				{
					(navigationView as Page)?.SendDisappearing();
				}
			};
#endif
		}
	}
}
