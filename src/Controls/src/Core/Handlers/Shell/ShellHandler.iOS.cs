#nullable enable
using System;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using MediaPlayer;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Handles iOS Shell flyout layout, item switching, and view-controller containment.
    /// </summary>
    public partial class ShellHandler : ViewHandler<Shell, UIView>, IShellContext, IAppearanceObserver, IFlyoutBehaviorObserver, IFlyoutContainerDelegate
    {
        FlyoutContainerManager? _flyoutManager;
        UIViewController? _detailContainerVC;
        UIView? _detailView;

        IShellFlyoutContentRenderer? _flyoutContentRenderer;
        Brush? _backdropBrush;
        double _flyoutWidth = -1;
        double _flyoutHeight = -1;

        IShellItemRenderer? _currentShellItemRenderer;
        IShellItemRenderer? _incomingRenderer;
        Task _activeTransition = Task.CompletedTask;

        IShellController ShellController => VirtualView;

        #region Handler Lifecycle

        protected override UIView CreatePlatformView()
        {
            _flyoutManager = new FlyoutContainerManager(containerDelegate: this);
            var hostVC = new ShellHostViewController(this, _flyoutManager);
            ViewController = hostVC;
            // Accessing .View forces ViewDidLoad — FlyoutContainerViewController sets up manager containers there.
            return hostVC.View!;
        }

        protected override void ConnectHandler(UIView platformView)
        {
            base.ConnectHandler(platformView);

            ShellController.AddAppearanceObserver(this, VirtualView);
            VirtualView.Navigated += OnShellNavigated;

            SetupFlyout();

            // Don't explicitly switch to the initial item here: Mapper.UpdateProperties() (called
            // right after ConnectHandler) invokes MapCurrentItem for the already-set CurrentItem,
            // which performs the switch. Calling SwitchToItem here too would race with that.
        }

        protected override void DisconnectHandler(UIView platformView)
        {
            ShellController.RemoveAppearanceObserver(this);
            VirtualView.Navigated -= OnShellNavigated;
            ((IShellController)VirtualView).RemoveFlyoutBehaviorObserver(this);

            _flyoutManager?.TearDown();
            _flyoutManager = null;
            _detailContainerVC = null;

            _detailView?.RemoveFromSuperview();
            _detailView = null;

            (_currentShellItemRenderer as IDisconnectable)?.Disconnect();
            if (_currentShellItemRenderer is not null)
            {
                // Remove the child controller before disposal to match SetCurrentShellItemRendererAsync.
                _currentShellItemRenderer.ViewController.WillMoveToParentViewController(null);
                _currentShellItemRenderer.ViewController.View?.RemoveFromSuperview();
                _currentShellItemRenderer.ViewController.RemoveFromParentViewController();
            }
            _currentShellItemRenderer?.Dispose();
            _currentShellItemRenderer = null;

            if (_flyoutContentRenderer is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _flyoutContentRenderer = null;

            ViewController = null;

            base.DisconnectHandler(platformView);
        }

        #endregion

        #region IShellContext Implementation

        bool IShellContext.AllowFlyoutGesture
        {
            get
            {
                // VirtualView throws instead of returning null when disconnected, so `VirtualView?.`
                // never actually short-circuits. Cast the untyped, nullable interface accessor instead.
                ShellSection? shellSection = (((IElementHandler)this).VirtualView as Shell)?.CurrentItem?.CurrentItem;

                if (shellSection is null)
                {
                    return true;
                }

                return shellSection.Stack.Count <= 1;
            }
        }

        IShellItemRenderer IShellContext.CurrentShellItemRenderer => _currentShellItemRenderer!;

        Shell IShellContext.Shell => VirtualView;

        IShellNavBarAppearanceTracker IShellContext.CreateNavBarAppearanceTracker()
        {
            return CreateNavBarAppearanceTracker();
        }

        IShellPageRendererTracker IShellContext.CreatePageRendererTracker()
        {
            return CreatePageRendererTracker();
        }

        IShellFlyoutContentRenderer IShellContext.CreateShellFlyoutContentRenderer()
        {
            return CreateShellFlyoutContentRenderer();
        }

        IShellSearchResultsRenderer IShellContext.CreateShellSearchResultsRenderer()
        {
            return CreateShellSearchResultsRenderer();
        }

        IShellSectionRenderer IShellContext.CreateShellSectionRenderer(ShellSection shellSection)
        {
            return CreateShellSectionRenderer(shellSection);
        }

        IShellTabBarAppearanceTracker IShellContext.CreateTabBarAppearanceTracker()
        {
            return CreateTabBarAppearanceTracker();
        }

        #endregion

        #region Virtual Factory Methods

        protected virtual IShellNavBarAppearanceTracker CreateNavBarAppearanceTracker()
        {
            return new SafeShellNavBarAppearanceTracker();
        }

        protected virtual IShellPageRendererTracker CreatePageRendererTracker()
        {
            return new ShellPageRendererTracker(this);
        }

        protected virtual IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer()
        {
            return new ShellFlyoutContentRenderer(this);
        }

        protected virtual IShellItemRenderer CreateShellItemRenderer(ShellItem item)
        {
            // Resolve through the handler registry so custom ShellItemHandler subclasses
            // registered via AddHandler<ShellItem, THandler>() are honored, matching Android.
            var handler = MauiContext!.Handlers.GetHandler(item.GetType()) as ShellItemHandler
                 ?? new ShellItemHandler();
            handler.SetMauiContext(item.FindMauiContext()!);
            handler.SetVirtualView(item);
            return new ShellItemHandler.ShellItemHandlerAdapter(handler);
        }

        protected virtual IShellItemTransition CreateShellItemTransition()
        {
            return new ShellItemTransition();
        }

        protected virtual IShellSearchResultsRenderer CreateShellSearchResultsRenderer()
        {
            return new ShellSearchResultsRenderer(this);
        }

        protected virtual IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
        {
            // Resolve through the handler registry so custom ShellSectionHandler subclasses
            // registered via AddHandler<ShellSection, THandler>() are honored, matching Android.
            var handler = MauiContext!.Handlers.GetHandler(shellSection.GetType()) as ShellSectionHandler
                 ?? new ShellSectionHandler();
            handler.SetMauiContext(shellSection.FindMauiContext()!);
            handler.SetVirtualView(shellSection);
            return new ShellSectionHandler.ShellSectionHandlerAdapter(handler);
        }

        protected virtual IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
        {
            return new ShellTabBarAppearanceTracker();
        }

        #endregion

        #region IAppearanceObserver

        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            if (appearance is null)
            {
                _backdropBrush = Brush.Default;
                if (_flyoutHeight != -1)
                { _flyoutHeight = -1; _flyoutManager?.UpdateFlyoutHeight(-1); }
            }
            else
            {
                _backdropBrush = appearance.FlyoutBackdrop;
                ApplyFlyoutBackdrop(this, _backdropBrush);

                if (_flyoutWidth != appearance.FlyoutWidth)
                {
                    _flyoutWidth = appearance.FlyoutWidth;
                    _flyoutManager?.UpdateFlyoutWidth(_flyoutWidth);
                }

                if (_flyoutHeight != appearance.FlyoutHeight)
                {
                    _flyoutHeight = appearance.FlyoutHeight;
                    _flyoutManager?.UpdateFlyoutHeight(_flyoutHeight);
                }
            }
        }

        #endregion

        #region IFlyoutBehaviorObserver

        void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
        {
            _flyoutManager?.UpdateFlyoutBehavior(behavior);
        }

        #endregion

        #region IFlyoutContainerDelegate

        void IFlyoutContainerDelegate.OnPresentedChangedByGesture(bool isPresented)
        {
            (((IElementHandler)this).VirtualView as Shell)?.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, isPresented);
        }

        void IFlyoutContainerDelegate.OnLayoutBoundsChanged(Rect flyoutBounds, Rect detailBounds)
        {
            // Shell does not mirror layout bounds into the virtual model.
        }

        void IFlyoutContainerDelegate.OnLeftBarButtonNeedsUpdate()
        {
            // Shell nav bar buttons are managed by ShellSectionHandler.
        }

        void IFlyoutContainerDelegate.OnViewDidAppear()
        {
        }

        void IFlyoutContainerDelegate.OnViewWillDisappear()
        {
        }

        bool IFlyoutContainerDelegate.GetCurrentIsPresented()
            => (((IElementHandler)this).VirtualView as Shell)?.FlyoutIsPresented ?? false;

        bool IFlyoutContainerDelegate.GetIgnoreSafeArea()
            => true;

        // Shell always uses overlay mode, regardless of device idiom.
        bool IFlyoutContainerDelegate.GetFlyoutOverlapsDetail()
            => true;

        // Shell doesn't dim the detail view in split/Locked mode; FlyoutPage's default (always dim) stays unchanged.
        bool IFlyoutContainerDelegate.GetSkipShadowInSplitMode()
            => true;

        // Shell keeps a manually-opened flyout presented across rotation and behavior changes; FlyoutPage always force-closes.
        bool IFlyoutContainerDelegate.GetPreservePresentedStateOnTransition()
            => true;

        #endregion

        #region Flyout Setup

        void SetupFlyout()
        {
            _flyoutContentRenderer = CreateShellFlyoutContentRenderer();
            _flyoutManager?.SetFlyoutViewController(_flyoutContentRenderer.ViewController);

            // Create the detail container — Shell manages what goes inside it.
            _detailView = new UIView();
            _detailView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            _detailContainerVC = new UIViewController();
            _detailContainerVC.View = _detailView;
            _flyoutManager?.SetDetailViewController(_detailContainerVC);
            _flyoutManager?.UpdateApplyShadow(true);
            _flyoutManager?.SetShadowBackgroundColor(UIColor.SystemBackground);
            _flyoutManager?.UpdateIsGestureEnabled(((IShellContext)this).AllowFlyoutGesture);

            ((IShellController)VirtualView).AddFlyoutBehaviorObserver(this);

            _flyoutManager?.UpdateIsPresented(VirtualView.FlyoutIsPresented, animated: false);
            UpdateFlowDirection();
        }

        void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            _flyoutManager?.UpdateIsGestureEnabled(((IShellContext)this).AllowFlyoutGesture);
        }

        static bool IsSwipeView(UIView? view)
        {
            if (view is null)
            {
                return false;
            }

            if (view is MauiSwipeView)
            {
                return true;
            }

            return IsSwipeView(view.Superview);
        }

        #endregion

        #region Flow Direction

        void UpdateFlowDirection(bool readdViews = false)
        {
            if (_detailView is null)
            {
                return;
            }

            _detailView.UpdateFlowDirection(VirtualView);
            PlatformView.UpdateFlowDirection(VirtualView);

            var flowDirection = VirtualView.FlowDirection == FlowDirection.RightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
            _flyoutManager?.UpdateFlowDirection(flowDirection);
        }

        #endregion

        #region Shell Item Switching

        async void SwitchToItem(ShellItem newItem, bool animate)
        {
            try
            {
                await SwitchToItemAsync(newItem, animate);
            }
            catch (Exception exc)
            {
                MauiContext?.CreateLogger<ShellHandler>()?.LogWarning(exc, "Failed on changing current item");
            }
        }

        async Task SwitchToItemAsync(ShellItem newItem, bool animate)
        {
            var oldLayer = _currentShellItemRenderer
                ?.ViewController
                ?.View
                ?.Layer;

            if (oldLayer?.AnimationKeys?.Length > 0)
            {
                oldLayer.RemoveAllAnimations();
            }

            await _activeTransition;

            if (_currentShellItemRenderer?.ShellItem != newItem)
            {
                var newRenderer = CreateShellItemRenderer(newItem);
                await SetCurrentShellItemRendererAsync(newRenderer, animate);
            }
        }

        async Task SetCurrentShellItemRendererAsync(IShellItemRenderer value, bool animate)
        {
            _incomingRenderer = value;
            await _activeTransition;

            // Selection changed (or the handler disconnected) while the previous transition was finishing.
            // Check the interface member first - the typed VirtualView throws instead of returning null.
            if (((IElementHandler)this).VirtualView is not Shell shell ||
                _incomingRenderer != value ||
                value.ShellItem != shell.CurrentItem)
            {
                (value as IDisconnectable)?.Disconnect();
                value?.Dispose();
                return;
            }

            var oldRenderer = _currentShellItemRenderer;
            (oldRenderer as IDisconnectable)?.Disconnect();
            var newRenderer = value;

            _currentShellItemRenderer = value;

            if (_detailView is null)
            {
                // Disconnect happened while awaiting the previous transition; neither renderer will attach.
                (newRenderer as IDisconnectable)?.Disconnect();
                newRenderer.Dispose();
                oldRenderer?.Dispose();
                _currentShellItemRenderer = null;
                return;
            }

            var newView = newRenderer.ViewController.View;

            // iOS 26's Liquid Glass tab bar fails to update/render its icons correctly if we attach the
            // incoming tab bar's view while a modal is still being dismissed. Deferring the attach by one
            // run loop turn lets the in-flight dismiss finish first, avoiding the ghosted tab-icon glitch.
            if ((OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
                && ViewController?.PresentedViewController is not null
                && newRenderer.ViewController is UITabBarController)
            {
                // Post to the main queue and await it instead of running inline: this defers our
                // continuation to the next run loop turn, letting the modal dismiss's already-queued
                // completion work run first.
                var pendingAction = new TaskCompletionSource();
                CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() => pendingAction.TrySetResult());
                await pendingAction.Task;

                // While we were waiting, another call to this method may have already run and become
                // the current renderer. If so, attaching our now-stale newRenderer would be wrong — bail out.
                if (_currentShellItemRenderer != value)
                {
                    return;
                }
            }

            if (newView is not null)
            {
                // Shell item VC is a child of the detail container VC.
                _detailContainerVC?.AddChildViewController(newRenderer.ViewController);

                newView.Frame = _detailView.Bounds;
                newView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                _detailView.AddSubview(newView);

                newRenderer.ViewController.DidMoveToParentViewController(_detailContainerVC);
            }

            if (oldRenderer is not null)
            {
                var transition = CreateShellItemTransition();
                _activeTransition = transition.Transition(oldRenderer, newRenderer);
                await _activeTransition;

                oldRenderer.ViewController?.WillMoveToParentViewController(null);
                oldRenderer.ViewController?.View?.RemoveFromSuperview();
                oldRenderer.ViewController?.RemoveFromParentViewController();
                oldRenderer.Dispose();
            }

            if (_currentShellItemRenderer == value)
            {
                UpdateBackgroundColor();
                UpdateFlowDirection();
            }
        }

        void UpdateBackgroundColor()
        {
            var color = VirtualView.BackgroundColor?.ToPlatform();

            if (color is null)
            {
                color = ColorExtensions.BackgroundColor;
            }

            PlatformView.BackgroundColor = color;
        }

        #endregion

        #region Static Map Methods

        ShellFlyoutContentRenderer? GetFlyoutContentRenderer()
            => _flyoutContentRenderer as ShellFlyoutContentRenderer;

        public static void MapCurrentItem(ShellHandler handler, Shell shell)
        {
            // PlatformView throws instead of returning null when disconnected, so `handler.PlatformView is null`
            // never actually short-circuits. Cast the untyped, nullable interface accessor instead.
            if (((IElementHandler)handler).PlatformView is null || shell.CurrentItem is null)
            {
                return;
            }

            handler.SwitchToItem(shell.CurrentItem, animate: true);
        }

        public static void MapFlyoutBackground(ShellHandler handler, Shell shell)
        {
            handler.UpdateBackgroundColor();
            handler.GetFlyoutContentRenderer()?.UpdateBackground();
        }

        public static void MapFlyoutBackdrop(ShellHandler handler, Shell shell)
        {
            handler._backdropBrush = shell.FlyoutBackdrop;
            ApplyFlyoutBackdrop(handler, shell.FlyoutBackdrop);
        }

        /// <summary>Applies a Shell.FlyoutBackdrop brush to the scrim view and routes solid colors through
        /// <see cref="FlyoutContainerManager.SetScrimColor"/> so the color is cached and correctly
        /// re-applied if the manager's containers are ever torn down and re-packed (previously this API
        /// had no callers, so the cache was always empty).</summary>
        static void ApplyFlyoutBackdrop(ShellHandler handler, Brush? backdropBrush)
        {
            var scrimView = handler._flyoutManager?.ScrimView;
            if (scrimView is null)
            {
                return;
            }

            scrimView.UpdateBackground(backdropBrush);

            if (Brush.IsNullOrEmpty(backdropBrush))
            {
                scrimView.BackgroundColor = UIColor.Clear;
                handler._flyoutManager?.SetScrimColor(null);
            }
            else if (backdropBrush is SolidColorBrush solidColorBrush)
            {
                handler._flyoutManager?.SetScrimColor(solidColorBrush.Color?.ToPlatform());
            }
            else
            {
                // Gradient/image brushes aren't representable as a single UIColor — clear any stale
                // cached solid color so a future re-pack doesn't incorrectly reapply an old solid backdrop.
                handler._flyoutManager?.SetScrimColor(null);
            }
        }

        public static void MapFlyoutHeader(ShellHandler handler, Shell shell)
        {
            handler.GetFlyoutContentRenderer()?.UpdateFlyoutHeader();
        }

        public static void MapFlyoutFooter(ShellHandler handler, Shell shell)
        {
            handler.GetFlyoutContentRenderer()?.UpdateFlyoutFooter();
        }

        public static void MapFlyoutHeaderBehavior(ShellHandler handler, Shell shell)
        {
            handler.GetFlyoutContentRenderer()?.UpdateFlyoutHeaderBehavior();
        }

        public static void MapFlyoutBehavior(ShellHandler handler, Shell shell)
        {
            // Effective behavior comes through IFlyoutBehaviorObserver, including page overrides.
        }

        public static void MapFlyoutWidth(ShellHandler handler, Shell shell)
        {
            // Width is applied through appearance tracking (IAppearanceObserver).
        }

        public static void MapIsPresented(ShellHandler handler, Shell shell)
        {
            handler._flyoutManager?.UpdateIsPresented(shell.FlyoutIsPresented, animated: true);
        }

        public static void MapFlyout(ShellHandler handler, Shell shell)
        {
            handler.GetFlyoutContentRenderer()?.UpdateFlyoutContent();
        }

        public static void MapFlowDirection(ShellHandler handler, Shell shell)
        {
            handler.UpdateFlowDirection(true);
            handler.GetFlyoutContentRenderer()?.UpdateFlowDirection();
        }

        public static void MapFlyoutBackgroundImage(ShellHandler handler, Shell shell)
        {
            handler.GetFlyoutContentRenderer()?.UpdateBackground();
        }

        public static void MapFlyoutVerticalScrollMode(ShellHandler handler, Shell shell)
        {
            handler.GetFlyoutContentRenderer()?.UpdateVerticalScrollMode();
        }

        public static void MapPrefersHomeIndicatorAutoHidden(ShellHandler handler, Shell shell)
        {
            handler.ViewController?.SetNeedsUpdateOfHomeIndicatorAutoHidden();
        }

        public static void MapPrefersStatusBarHidden(ShellHandler handler, Shell shell)
        {
            handler.ViewController?.SetNeedsStatusBarAppearanceUpdate();
        }

        public static void MapPreferredStatusBarUpdateAnimation(ShellHandler handler, Shell shell)
        {
            handler.ViewController?.SetNeedsStatusBarAppearanceUpdate();
        }

        public static void MapFlyoutIcon(ShellHandler handler, Shell shell)
            => TriggerLeftBarButtonUpdate(handler, shell);

        public static void MapForegroundColor(ShellHandler handler, Shell shell)
            => TriggerLeftBarButtonUpdate(handler, shell);

        // Refresh toolbar items so FlyoutIcon/ForegroundColor changes reach the current page.
        internal static void TriggerLeftBarButtonUpdate(ShellHandler handler, Shell shell)
        {
            var section = shell.CurrentItem?.CurrentItem;
            if (section?.Handler is not ShellSectionHandler sectionHandler)
            {
                return;
            }

            var displayedPage = section.DisplayedPage;
            if (displayedPage is null)
            {
                return;
            }

            if (sectionHandler._trackers.TryGetValue(displayedPage, out var tracker) &&
                tracker is ShellPageRendererTracker shellRendererTracker)
            {
                shellRendererTracker.UpdateToolbarItemsInternal();
            }
        }

        #endregion

        #region Host View Controller

        /// <summary>
        /// Hosts Shell content; subclasses FlyoutContainerViewController so the manager wires itself in ViewDidLoad.
        /// </summary>
        sealed class ShellHostViewController : FlyoutContainerViewController
        {
            readonly WeakReference<ShellHandler> _handlerRef;

            public ShellHostViewController(ShellHandler handler, FlyoutContainerManager manager)
                : base(manager)
            {
                _handlerRef = new WeakReference<ShellHandler>(handler);
            }

            // VirtualView throws instead of returning null when disconnected, and this view controller
            // can legitimately outlive the handler's disconnect during UIKit transitions/rotation.
            // Cast the untyped, nullable interface accessor instead.
            Shell? Shell => _handlerRef.TryGetTarget(out var handler) ? ((IElementHandler)handler).VirtualView as Shell : null;

            public override bool PrefersHomeIndicatorAutoHidden
                => Shell?.CurrentPage?.OnThisPlatform()?.PrefersHomeIndicatorAutoHidden() ?? base.PrefersHomeIndicatorAutoHidden;

            public override bool PrefersStatusBarHidden()
                => Shell?.CurrentPage?.OnThisPlatform()?.PrefersStatusBarHidden() == StatusBarHiddenMode.True;

            // null cancels the base class's redirect to _detailContainerVC (an empty layout-only
            // VC), so iOS asks this VC directly and reaches PrefersStatusBarHidden()/
            // PrefersHomeIndicatorAutoHidden above instead.
            public override UIViewController? ChildViewControllerForStatusBarHidden() => null;
            public override UIViewController? ChildViewControllerForHomeIndicatorAutoHidden => null;

#if !MACCATALYST
            public override UIViewController? ChildViewControllerForStatusBarStyle()
            {
                if (Shell?.Window?.StatusBarTheme == StatusBarTheme.Default)
                {
                    return base.ChildViewControllerForStatusBarStyle();
                }

                return null;
            }

            public override UIStatusBarStyle PreferredStatusBarStyle()
            {
                var theme = Shell?.Window?.StatusBarTheme ?? StatusBarTheme.Default;

                return theme switch
                {
                    StatusBarTheme.Light => UIStatusBarStyle.DarkContent,
                    StatusBarTheme.Dark => UIStatusBarStyle.LightContent,
                    _ => base.PreferredStatusBarStyle()
                };
            }
#endif

            public override UIKit.UIStatusBarAnimation PreferredStatusBarUpdateAnimation
            {
                get
                {
                    var mode = Shell?.CurrentPage?.OnThisPlatform()?.PreferredStatusBarUpdateAnimation();
                    return mode switch
                    {
                        PlatformConfiguration.iOSSpecific.UIStatusBarAnimation.None => UIKit.UIStatusBarAnimation.None,
                        PlatformConfiguration.iOSSpecific.UIStatusBarAnimation.Fade => UIKit.UIStatusBarAnimation.Fade,
                        PlatformConfiguration.iOSSpecific.UIStatusBarAnimation.Slide => UIKit.UIStatusBarAnimation.Slide,
                        _ => base.PreferredStatusBarUpdateAnimation,
                    };
                }
            }
        }

        #endregion
    }
}
