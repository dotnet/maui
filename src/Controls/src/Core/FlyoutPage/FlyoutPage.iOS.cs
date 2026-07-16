using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls
{
    public partial class FlyoutPage
    {
        // Track the flyout page this specific FlyoutPage instance is subscribed to
        // for icon/title property changes. Instance-scoped (not static) so multiple
        // FlyoutPage instances (multi-window, modal FlyoutPage, etc.) don't clobber
        // each other's subscriptions.
        WeakReference<Page>? _subscribedFlyout;

        // Cached delegate instance so we can unsubscribe the exact same handler
        // we subscribed with.
        PropertyChangedEventHandler? _flyoutPropertyChangedHandler;


        internal static void OnPresentedChangedByGesture(IFlyoutView view, bool isPresented)
        {
            if (view is FlyoutPage fp)
            {
                // Guard: during rotation, ShouldShowSplitMode may still return true while
                // orientation hasn't settled. Writing false triggers InvalidOperationException
                // in OnIsPresentedPropertyChanging validation.
                if (!isPresented && ((IFlyoutPageController)fp).ShouldShowSplitMode)
                {
                    return;
                }

                fp.IsPresented = isPresented;
            }
            else
            {
                view.IsPresented = isPresented;
            }
        }

        internal static void OnLayoutBoundsChanged(IFlyoutView view, Rect flyoutBounds, Rect detailBounds)
        {
            if (view is IFlyoutPageController controller)
            {
                controller.FlyoutBounds = flyoutBounds;
                controller.DetailBounds = detailBounds;
            }
        }

        internal static void OnLeftBarButtonNeedsUpdate(IFlyoutView view)
        {
            if (view is not FlyoutPage fp)
            {
                return;
            }

            fp.SubscribeToFlyoutPropertyChanges();

            if (fp.Detail?.Handler is not IPlatformViewHandler detailHandler)
            {
                return;
            }

            var detailVC = detailHandler.ViewController;
            if (detailVC is null)
            {
                return;
            }

            // If detail VC is a UINavigationController, use its root VC
            var targetVC = detailVC is UINavigationController nav
                ? nav.ViewControllers?.FirstOrDefault() ?? detailVC
                : detailVC;

            UpdateFlyoutLeftBarButton(targetVC, fp);
        }

        /// <summary>
        /// Called when this FlyoutPage's handler is disconnected, so its flyout
        /// icon/title subscription doesn't outlive the handler.
        /// </summary>
        internal static void OnHandlerDisconnected(IFlyoutView view)
        {
            if (view is FlyoutPage fp)
            {
                fp.UnsubscribeFlyoutPropertyChanges();
            }
        }

        void SubscribeToFlyoutPropertyChanges()
        {
            var flyout = Flyout;
            if (flyout is null)
            {
                return;
            }

            // Unsubscribe from this instance's previous flyout if it changed
            if (_subscribedFlyout is not null && _subscribedFlyout.TryGetTarget(out var oldFlyout))
            {
                if (ReferenceEquals(oldFlyout, flyout))
                {
                    return; // Already subscribed to this flyout
                }

                if (_flyoutPropertyChangedHandler is not null)
                {
                    oldFlyout.PropertyChanged -= _flyoutPropertyChangedHandler;
                }
            }

            _flyoutPropertyChangedHandler = OnFlyoutPagePropertyChanged;
            flyout.PropertyChanged += _flyoutPropertyChangedHandler;
            _subscribedFlyout = new WeakReference<Page>(flyout);
        }

        /// <summary>
        /// Unsubscribes from the currently-tracked flyout's property changes.
        /// Called when this FlyoutPage's handler is disconnected.
        /// </summary>
        void UnsubscribeFlyoutPropertyChanges()
        {
            if (_subscribedFlyout is not null &&
                _subscribedFlyout.TryGetTarget(out var flyout) &&
                _flyoutPropertyChangedHandler is not null)
            {
                flyout.PropertyChanged -= _flyoutPropertyChangedHandler;
            }

            _flyoutPropertyChangedHandler = null;
            _subscribedFlyout = null;
        }

        void OnFlyoutPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Page.IconImageSourceProperty.PropertyName ||
                e.PropertyName == Page.TitleProperty.PropertyName)
            {
                if (sender is Page flyoutPage && flyoutPage.Parent is FlyoutPage fp)
                {
                    OnLeftBarButtonNeedsUpdate(fp);
                }
            }
        }

        static void UpdateFlyoutLeftBarButton(UIViewController targetVC, FlyoutPage flyoutPage)
        {
            if (!flyoutPage.ShouldShowToolbarButton())
            {
                targetVC.NavigationItem.LeftBarButtonItem = null;
                return;
            }

            EventHandler onItemTapped = (sender, e) =>
            {
                flyoutPage.IsPresented = !flyoutPage.IsPresented;
            };

            var mauiContext = flyoutPage.FindMauiContext();
            if (mauiContext is null)
            {
                return;
            }

            flyoutPage.Flyout.IconImageSource.LoadImage(mauiContext, result =>
            {
                var icon = result?.Value;

                if (icon is not null)
                {
                    // Scale icon to fit nav bar (max 44pt height)
                    var originalSize = icon.Size;
                    if (originalSize.Height > 44)
                    {
                        if (flyoutPage.Flyout.IconImageSource is not FontImageSource fontImageSource ||
                            !fontImageSource.IsSet(FontImageSource.SizeProperty))
                        {
                            icon = icon.ResizeImageSource(originalSize.Width, 44f, originalSize);
                        }
                    }

                    try
                    {
                        targetVC.NavigationItem.LeftBarButtonItem =
                            new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, onItemTapped);
                    }
                    catch (Exception)
                    {
                        // UIBarButtonItem creation can throw
                    }
                }

                if (icon is null || targetVC.NavigationItem.LeftBarButtonItem is null)
                {
                    // Fallback: use Flyout.Title as text button
                    targetVC.NavigationItem.LeftBarButtonItem =
                        new UIBarButtonItem(flyoutPage.Flyout?.Title ?? string.Empty, UIBarButtonItemStyle.Plain, onItemTapped);
                }

                // Set AutomationId and VoiceOver label/hint on the hamburger button.
                if (!string.IsNullOrEmpty(flyoutPage.AutomationId))
                {
                    targetVC.NavigationItem.LeftBarButtonItem.AccessibilityIdentifier = $"btn_{flyoutPage.AutomationId}";
                }

                // Apply FlyoutPage's SemanticProperties (Description/Hint), if set.
                var semantics = SemanticProperties.UpdateSemantics(flyoutPage, null);
                if (semantics is not null)
                {
                    targetVC.NavigationItem.LeftBarButtonItem.UpdateSemantics(semantics);
                }
            });
        }


        internal static void MapApplyShadow(IFlyoutViewHandler handler, IFlyoutView view)
        {
            if (handler is FlyoutViewHandler h && h._manager is { } manager && view is BindableObject bo)
            {
                var applyShadow = PlatformConfiguration.iOSSpecific.FlyoutPage.GetApplyShadow(bo);
                manager.UpdateApplyShadow(applyShadow);
            }
        }

        internal static void MapFlowDirection(IFlyoutViewHandler handler, IFlyoutView view)
        {
            if (handler is FlyoutViewHandler h && h._manager is { } manager && view is IView v)
            {
                // Use the effective/inherited flow direction rather than the raw
                // FlowDirection. A FlyoutPage left at the default MatchParent should
                // follow an RTL app/window, not be treated as LTR.
                var flowDirection = (view as IVisualElementController)?.EffectiveFlowDirection.ToFlowDirection()
                    ?? v.FlowDirection;
                manager.UpdateFlowDirection(flowDirection);
            }
        }
    }
}
