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
						// Match renderer's RemoveAsyncInner — fire lifecycle events
						// that NavigationFinished (stack sync) does not handle.
						navPage.FireDisappearing(page);

						// Fire NavigatedFrom on the popped page directly, bypassing
						// SendNavigatedFromHandler's HasNavigatedTo guard which blocks
						// subsequent pages in a multi-pop scenario.
						page.SendNavigatedFrom(new NavigatedFromEventArgs(navPage.CurrentPage, NavigationType.Pop));

						// Fire NavigatedTo + Appearing on CurrentPage only if not already done
						// (avoids duplicate events for multi-pop where this callback fires per page).
						if (!navPage.CurrentPage.HasNavigatedTo)
						{
							navPage.FireAppearing(navPage.CurrentPage);
							navPage.CurrentPage.SendNavigatedTo(new NavigatedToEventArgs(page, NavigationType.Pop));
						}

						navPage.Popped?.Invoke(navPage, new NavigationEventArgs(page));
					}
				},
				OnControllerAppeared = (navigationView) =>
				{
					if (navigationView is VisualElement ve)
					{
						ve.RefreshPlatformLoadedStatus();
					}
					(navigationView as Page)?.SendAppearing();

					// Fire deferred NavigatedTo if it was skipped in OnHandlerChangedCore
					// because NavigationProxy.Inner wasn't wired yet at handler init time.
					// By ViewDidAppear, the Window has parented the page and Inner is set.
					(navigationView as NavigationPage)?.FireDeferredNavigatedTo();
				},
				OnControllerDisappeared = (navigationView) =>
				{
					(navigationView as Page)?.SendDisappearing();
				},
				OnMidStackChanged = (topVC) =>
				{
					if (topVC is NavigationHandlerParentingViewController parentingVC)
					{
						parentingVC.NotifyStackChanged();
					}
				}
			};
#endif
		}
	}
}
