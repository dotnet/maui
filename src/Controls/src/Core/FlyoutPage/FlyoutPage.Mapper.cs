using System;
using Microsoft.Maui.Handlers;
#if IOS || MACCATALYST
using UIKit;
#endif

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Page"/> that manages two panes of information: a flyout that presents a menu or navigation, and a detail that presents the selected content.</summary>
	public partial class FlyoutPage
	{
		internal new static void RemapForControls()
		{
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(FlyoutLayoutBehavior), MapFlyoutLayoutBehavior);
#if IOS || MACCATALYST
			// Fill configuration record (Core → Controls bridge)
			FlyoutViewHandler.ControlsConfiguration = new(
				OnPresentedChangedByGesture: FlyoutPage.OnPresentedChangedByGesture,
				OnLayoutBoundsChanged: FlyoutPage.OnLayoutBoundsChanged,
				OnLeftBarButtonNeedsUpdate: FlyoutPage.OnLeftBarButtonNeedsUpdate,
				OnHandlerDisconnected: FlyoutPage.OnHandlerDisconnected
			);

			// iOS-specific property mappers
			FlyoutViewHandler.Mapper.AppendToMapping(
				PlatformConfiguration.iOSSpecific.FlyoutPage.ApplyShadowProperty.PropertyName,
				MapApplyShadow);
			FlyoutViewHandler.Mapper.AppendToMapping(nameof(IView.FlowDirection), MapFlowDirection);
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty), MapPrefersHomeIndicatorAutoHiddenProperty);
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty), MapPrefersPrefersStatusBarHiddenProperty);
#endif
#if WINDOWS
			FlyoutViewHandler.Mapper.ReplaceMapping<IFlyoutView, IFlyoutViewHandler>(nameof(PlatformConfiguration.WindowsSpecific.FlyoutPage.CollapseStyleProperty), MapCollapseStyle);
#endif
		}

		internal static void MapFlyoutLayoutBehavior(IFlyoutViewHandler handler, IFlyoutView view)
		{
			handler.UpdateValue(nameof(IFlyoutView.FlyoutBehavior));
		}

#if IOS || MACCATALYST
		internal static void MapPrefersHomeIndicatorAutoHiddenProperty(IFlyoutViewHandler handler, IFlyoutView view)
		{
			if (handler is IPlatformViewHandler { ViewController: { } vc })
			{
				vc.SetNeedsUpdateOfHomeIndicatorAutoHidden();
			}
		}

		internal static void MapPrefersPrefersStatusBarHiddenProperty(IFlyoutViewHandler handler, IFlyoutView view)
		{
			if (handler is IPlatformViewHandler { ViewController: { } vc })
			{
				vc.SetNeedsStatusBarAppearanceUpdate();
			}
		}
#endif

#if WINDOWS
		internal static void MapCollapseStyle(IFlyoutViewHandler handler, IFlyoutView view)
		{
			var flyoutLayoutBehavior = (view as FlyoutPage)?.FlyoutLayoutBehavior;
			if (view is BindableObject bindable && handler.PlatformView is Microsoft.Maui.Platform.RootNavigationView navigationView && flyoutLayoutBehavior is FlyoutLayoutBehavior.Popover)
			{
				var collapseStyle = PlatformConfiguration.WindowsSpecific.FlyoutPage.GetCollapseStyle(bindable);
				switch (collapseStyle)
				{
					case PlatformConfiguration.WindowsSpecific.CollapseStyle.Partial:
						navigationView.FlyoutPaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftCompact;
						navigationView.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftCompact;
						break;
					case PlatformConfiguration.WindowsSpecific.CollapseStyle.Full:
					default:
						navigationView.FlyoutPaneDisplayMode = null;
						navigationView.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
						break;
				}
			}
		}
#endif
	}
}
