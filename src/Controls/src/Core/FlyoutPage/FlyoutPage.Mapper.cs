using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Page"/> that manages two panes of information: a flyout that presents a menu or navigation, and a detail that presents the selected content.</summary>
	public partial class FlyoutPage
	{
		internal new static void RemapForControls()
		{
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(FlyoutLayoutBehavior), MapFlyoutLayoutBehavior);
#if IOS
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty), MapPrefersHomeIndicatorAutoHiddenProperty);
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty), MapPrefersPrefersStatusBarHiddenProperty);
#endif
#if WINDOWS
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(PlatformConfiguration.WindowsSpecific.FlyoutPage.CollapsedPaneWidthProperty), MapCollapsedPaneWidth);
#endif
		}

		internal static void MapFlyoutLayoutBehavior(IFlyoutViewHandler handler, IFlyoutView view)
		{
			handler.UpdateValue(nameof(IFlyoutView.FlyoutBehavior));
		}

#if IOS
		internal static void MapPrefersHomeIndicatorAutoHiddenProperty(IFlyoutViewHandler handler, IFlyoutView view)
		{
			handler.UpdateValue(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty));
		}

		internal static void MapPrefersPrefersStatusBarHiddenProperty(IFlyoutViewHandler handler, IFlyoutView view)
		{
			handler.UpdateValue(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty));
		}
#endif

#if WINDOWS
		internal static void MapCollapsedPaneWidth(IFlyoutViewHandler handler, IFlyoutView view)
		{
			if (view is BindableObject bindable && handler.PlatformView is Microsoft.Maui.Platform.RootNavigationView navigationView)
			{
				var collapsedPaneWidth = PlatformConfiguration.WindowsSpecific.FlyoutPage.GetCollapsedPaneWidth(bindable);
				if (collapsedPaneWidth > 0)
				{
					navigationView.CompactPaneLength = collapsedPaneWidth;
				}
			}
		}
#endif
	}
}