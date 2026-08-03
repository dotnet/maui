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
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Handles iOS Shell flyout layout, item switching, and view-controller containment.
    /// </summary>
    public partial class ShellHandler : ViewHandler<Shell, UIView>, IShellContext, IAppearanceObserver, IFlyoutBehaviorObserver
    {
        UIView? _flyoutView;
        UIView? _detailView;
        UIView? _tapoffView;
        UIPanGestureRecognizer? _panGestureRecognizer;
        UIViewPropertyAnimator? _flyoutAnimation;
        bool _gestureActive;
        bool _isOpen;
        bool _layoutOccured;

        IShellFlyoutContentRenderer? _flyoutContentRenderer;
        Brush? _backdropBrush;
        FlyoutBehavior _flyoutBehavior;
        double _flyoutWidth = -1;
        double _flyoutHeight = -1;

        IShellItemRenderer? _currentShellItemRenderer;
        IShellItemRenderer? _incomingRenderer;
        Task _activeTransition = Task.CompletedTask;

        IShellController ShellController => VirtualView;

        bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen == value)
                {
                    return;
                }

                _isOpen = value;
                VirtualView?.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, value);
                UpdateFlyoutAccessibility();
            }
        }

        #region Handler Lifecycle

        protected override UIView CreatePlatformView()
        {
            var container = new ShellContainerView();
            container.SetHandler(this);

            // Use a real UIViewController so ToUIViewController() and containment-based nav-bar layout work.
            var hostVC = new ShellHostViewController(this);
            hostVC.View = container;
            ViewController = hostVC;

            return container;
        }

        protected override void ConnectHandler(UIView platformView)
        {
            base.ConnectHandler(platformView);

            ShellController.AddAppearanceObserver(this, VirtualView);

            SetupFlyout();

            if (VirtualView.CurrentItem is not null)
            {
                SwitchToItem(VirtualView.CurrentItem, animate: false);
            }
        }

        protected override void DisconnectHandler(UIView platformView)
        {
            ShellController.RemoveAppearanceObserver(this);
            ((IShellController)VirtualView).RemoveFlyoutBehaviorObserver(this);

            if (_flyoutAnimation is not null)
            {
                _flyoutAnimation.StopAnimation(true);
                _flyoutAnimation = null;
            }

            if (_panGestureRecognizer is not null)
            {
                platformView.RemoveGestureRecognizer(_panGestureRecognizer);
                _panGestureRecognizer = null;
            }

            if (_tapoffView is not null)
            {
                _tapoffView.RemoveFromSuperview();
                _tapoffView = null;
            }

            _flyoutView?.RemoveFromSuperview();
            _flyoutView = null;

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
                ShellSection? shellSection = VirtualView?.CurrentItem?.CurrentItem;

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
            var handler = new ShellItemHandler();
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
            var handler = new ShellSectionHandler();
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
            }
            else
            {
                _backdropBrush = appearance.FlyoutBackdrop;

                bool sizeChanged = false;
                if (_flyoutHeight != appearance.FlyoutHeight || _flyoutWidth != appearance.FlyoutWidth)
                {
                    _flyoutHeight = appearance.FlyoutHeight;
                    _flyoutWidth = appearance.FlyoutWidth;
                    sizeChanged = true;
                }

                if (sizeChanged && _layoutOccured)
                {
                    LayoutSidebar(false, true);
                }
            }

            UpdateTapoffViewBackgroundColor();
        }

        #endregion

        #region IFlyoutBehaviorObserver

        void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
        {
            _flyoutBehavior = behavior;
            OnFlyoutBehaviorChanged(behavior);
        }

        #endregion

        #region Flyout Setup

        void SetupFlyout()
        {
            var platformView = PlatformView;

            // Create detail container (will hold ShellItemHandler's view)
            var detailView = new UIView(platformView.Bounds);

            // Create flyout content via factory
            _flyoutContentRenderer = CreateShellFlyoutContentRenderer();
            var flyoutView = _flyoutContentRenderer.ViewController.View;

            if (flyoutView is null)
            {
                return;
            }

            // Setup flyout and detail views
            _detailView = detailView;
            _detailView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            platformView.AddSubview(_detailView);

            _flyoutView = flyoutView;
            platformView.AddSubview(_flyoutView);

            SetupPanGesture();

            // Register as flyout behavior observer
            ((IShellController)VirtualView).AddFlyoutBehaviorObserver(this);

            // Set initial flyout state
            IsOpen = VirtualView.FlyoutIsPresented;
            UpdateFlowDirection();
            UpdateFlyoutAccessibility();
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

            var originalDetailValue = _detailView.SemanticContentAttribute;
            var originalViewValue = PlatformView.SemanticContentAttribute;

            _detailView.UpdateFlowDirection(VirtualView);
            PlatformView.UpdateFlowDirection(VirtualView);

            bool update = originalDetailValue == _detailView.SemanticContentAttribute ||
                originalViewValue == PlatformView.SemanticContentAttribute;

            if (update && readdViews)
            {
                _detailView.RemoveFromSuperview();
                PlatformView.InsertSubview(_detailView, 0);
            }
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

            // Selection changed while the previous transition was finishing.
            if (_incomingRenderer != value ||
                value.ShellItem != VirtualView.CurrentItem)
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

            if (newView is not null)
            {
                // Add controller containment before the child view so nav-bar layout sees the full parent chain.
                ViewController?.AddChildViewController(newRenderer.ViewController);

                newView.Frame = _detailView.Bounds;
                newView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                _detailView.AddSubview(newView);

                if (ViewController is not null)
                {
                    newRenderer.ViewController.DidMoveToParentViewController(ViewController);
                }
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
            if (handler.PlatformView is null || shell.CurrentItem is null)
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
            handler.UpdateTapoffViewBackgroundColor();
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
            // Effective behavior already comes through IFlyoutBehaviorObserver, including page overrides.
        }

        public static void MapFlyoutWidth(ShellHandler handler, Shell shell)
        {
            if (handler._layoutOccured)
            {
                handler.LayoutSidebar(false, true);
            }
        }

        public static void MapIsPresented(ShellHandler handler, Shell shell)
        {
            if (handler.IsOpen != shell.FlyoutIsPresented)
            {
                handler.IsOpen = shell.FlyoutIsPresented;
                handler.LayoutSidebar(true, true);
            }
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
            ((IShellContext)handler).CurrentShellItemRenderer?.ViewController
                ?.SetNeedsUpdateOfHomeIndicatorAutoHidden();
        }

        public static void MapPrefersStatusBarHidden(ShellHandler handler, Shell shell)
        {
            ((IShellContext)handler).CurrentShellItemRenderer?.ViewController
                ?.SetNeedsStatusBarAppearanceUpdate();
        }

        public static void MapPreferredStatusBarUpdateAnimation(ShellHandler handler, Shell shell)
        {
            ((IShellContext)handler).CurrentShellItemRenderer?.ViewController
                ?.SetNeedsStatusBarAppearanceUpdate();
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
        /// Hosts Shell content and forwards status-bar/home-indicator preferences to the current page.
        /// </summary>
        sealed class ShellHostViewController : UIViewController
        {
            readonly WeakReference<ShellHandler> _handlerRef;

            public ShellHostViewController(ShellHandler handler)
            {
                _handlerRef = new WeakReference<ShellHandler>(handler);
            }

            Shell? Shell => _handlerRef.TryGetTarget(out var handler) ? handler.VirtualView : null;

            public override bool PrefersHomeIndicatorAutoHidden
                => Shell?.CurrentPage?.OnThisPlatform()?.PrefersHomeIndicatorAutoHidden() ?? base.PrefersHomeIndicatorAutoHidden;

            public override bool PrefersStatusBarHidden()
                => Shell?.CurrentPage?.OnThisPlatform()?.PrefersStatusBarHidden() == StatusBarHiddenMode.True;

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

        #region Container View

        /// <summary>
        /// Relayouts the flyout on bounds changes without retaining the handler.
        /// </summary>
        sealed class ShellContainerView : UIView
        {
            WeakReference<ShellHandler>? _handlerRef;

            public ShellContainerView() : base()
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            }

            internal void SetHandler(ShellHandler handler)
            {
                _handlerRef = new WeakReference<ShellHandler>(handler);
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                if (_handlerRef is not null &&
                    _handlerRef.TryGetTarget(out var handler) &&
                    handler._flyoutAnimation is null)
                {
                    handler.LayoutSidebar(false);
                }
            }
        }

        #endregion

        #region Flyout Layout

        bool IsRTL => PlatformView.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft;

        void SetupPanGesture()
        {
            _panGestureRecognizer = new UIPanGestureRecognizer(HandlePanGesture);

            _panGestureRecognizer.ShouldRecognizeSimultaneously += (gestureRecognizer, otherGestureRecognizer) =>
            {
                if (gestureRecognizer is UIPanGestureRecognizer panRecognizer &&
                    panRecognizer.State == UIGestureRecognizerState.Failed &&
                    otherGestureRecognizer is UITapGestureRecognizer &&
                    otherGestureRecognizer.State == UIGestureRecognizerState.Ended &&
                    IsOpen)
                {
                    IsOpen = false;
                    LayoutSidebar(true);
                }

                return false;
            };

            _panGestureRecognizer.ShouldReceiveTouch += (sender, touch) =>
            {
                if (!((IShellContext)this).AllowFlyoutGesture || _flyoutBehavior != FlyoutBehavior.Flyout)
                {
                    return false;
                }

                return ShouldReceiveTouch(touch, PlatformView);
            };

            PlatformView.AddGestureRecognizer(_panGestureRecognizer);
        }

        void HandlePanGesture(UIPanGestureRecognizer pan)
        {
            var translation = pan.TranslationInView(PlatformView).X;
            double openProgress = 0;
            double openLimit = _flyoutView?.Frame.Width ?? 0;

            if (openLimit <= 0)
            {
                return;
            }

            if (IsRTL)
            {
                translation = -translation;
            }

            if (IsOpen)
            {
                openProgress = 1 - (-translation / openLimit);
            }
            else
                openProgress = translation / openLimit;

            openProgress = Math.Min(Math.Max(openProgress, 0.0), 1.0);

            switch (pan.State)
            {
                case UIGestureRecognizerState.Changed:
                    _gestureActive = true;

                    if (_tapoffView is null)
                    {
                        AddTapoffView();
                    }

                    if (_flyoutAnimation is not null)
                    {
                        _tapoffView?.Layer.RemoveAllAnimations();
                        _flyoutAnimation.StopAnimation(true);
                        _flyoutAnimation = null;
                    }

                    if (_tapoffView is not null)
                    {
                        _tapoffView.Layer.Opacity = (float)openProgress;
                    }

                    LayoutFlyoutViews((nfloat)openProgress);
                    break;

                case UIGestureRecognizerState.Ended:
                    _gestureActive = false;

                    if (IsOpen)
                    {
                        if (openProgress < 0.8)
                        {
                            IsOpen = false;
                        }
                    }
                    else
                    {
                        if (openProgress > 0.2)
                        {
                            IsOpen = true;
                        }
                    }

                    LayoutSidebar(true);
                    break;
            }
        }

        void LayoutSidebar(bool animate, bool cancelExisting = false)
        {
            _layoutOccured = true;

            if (_gestureActive)
            {
                return;
            }

            if (cancelExisting && _flyoutAnimation is not null)
            {
                _flyoutAnimation.StopAnimation(true);
                _flyoutAnimation = null;
            }

            if (animate && _flyoutAnimation is not null)
            {
                return;
            }

            if (!animate && _flyoutAnimation is not null)
            {
                _flyoutAnimation.StopAnimation(true);
                _flyoutAnimation = null;
            }

            if (IsOpen)
            {
                UpdateTapoffView();
            }

            if (animate && _tapoffView is not null && _flyoutView is not null && _detailView is not null)
            {
                var tapOffViewAnimation = CABasicAnimation.FromKeyPath(@"opacity");
                tapOffViewAnimation.BeginTime = 0;
                tapOffViewAnimation.Duration = 0.25;
                tapOffViewAnimation.SetFrom(NSNumber.FromFloat(_tapoffView.Layer.Opacity));
                tapOffViewAnimation.SetTo(NSNumber.FromFloat(IsOpen ? 1 : 0));
                tapOffViewAnimation.FillMode = CAFillMode.Forwards;
                tapOffViewAnimation.RemovedOnCompletion = false;

                _flyoutAnimation = new UIViewPropertyAnimator(0.25, UIViewAnimationCurve.EaseOut, () =>
                {
                    LayoutFlyoutViews(IsOpen ? 1 : 0);
                    _tapoffView?.Layer.AddAnimation(tapOffViewAnimation, "opacity");
                });

                _flyoutAnimation.AddCompletion((p) =>
                {
                    if (p == UIViewAnimatingPosition.End)
                    {
                        if (_tapoffView is not null)
                        {
                            _tapoffView.Layer.Opacity = IsOpen ? 1 : 0;
                            _tapoffView.Layer.RemoveAllAnimations();
                        }

                        UpdateTapoffView();
                        _flyoutAnimation = null;

                        UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, null);
                    }
                });

                _flyoutAnimation.StartAnimation();
                PlatformView.LayoutIfNeeded();
            }
            else if (_flyoutAnimation is null && _flyoutView is not null && _detailView is not null)
            {
                LayoutFlyoutViews(IsOpen ? 1 : 0);
                UpdateTapoffView();

                if (_tapoffView is not null)
                {
                    _tapoffView.Layer.Opacity = IsOpen ? 1 : 0;
                }

                UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, null);
            }
        }

        void LayoutFlyoutViews(nfloat openPercent)
        {
            if (_flyoutView is null || _detailView is null)
            {
                return;
            }

            var bounds = PlatformView.Bounds;

            if (_flyoutBehavior == FlyoutBehavior.Locked)
            {
                openPercent = 1;
            }

            nfloat flyoutWidth = GetFlyoutWidth();
            nfloat flyoutHeight = _flyoutHeight >= 0
                ? (nfloat)_flyoutHeight
                : bounds.Height;

            nfloat openLimit = flyoutWidth;
            nfloat openPixels = openLimit * openPercent;

            if (_flyoutBehavior == FlyoutBehavior.Locked)
                // Match legacy ShellRenderer: locked RTL flyouts intentionally overlap the detail frame.
                _detailView.Frame = new CGRect(bounds.X + flyoutWidth, bounds.Y, bounds.Width - flyoutWidth, flyoutHeight);
            else
                _detailView.Frame = bounds;

            if (IsRTL)
            {
                // Use full bounds so locked RTL flyouts stay flush with the trailing edge.
                var positionX = bounds.Width - openPixels;
                _flyoutView.Frame = new CGRect(positionX, 0, flyoutWidth, flyoutHeight);
            }
            else
            {
                _flyoutView.Frame = new CGRect(-openLimit + openPixels, 0, flyoutWidth, flyoutHeight);
            }

            // Keep the backdrop sized to the container after rotations; AddTapoffView sizes it only once.
            if (_tapoffView is not null)
            {
                _tapoffView.Frame = bounds;
            }
        }

        nfloat GetFlyoutWidth()
        {
            if (_flyoutWidth >= 0)
            {
                return (nfloat)_flyoutWidth;
            }

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
            {
                return 320;
            }

            var bounds = PlatformView.Bounds;
            return (nfloat)(Math.Min(bounds.Width, bounds.Height) * 0.8);
        }

        void OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
        {
            if (behavior == FlyoutBehavior.Locked)
            {
                IsOpen = true;
            }
            else if (behavior == FlyoutBehavior.Disabled)
            {
                IsOpen = false;
            }

            LayoutSidebar(false);
            UpdateFlyoutAccessibility();
        }

        void UpdateTapoffView()
        {
            if (IsOpen && _flyoutBehavior == FlyoutBehavior.Flyout)
            {
                AddTapoffView();
            }
            else
                RemoveTapoffView();
        }

        void AddTapoffView()
        {
            if (_tapoffView is not null)
            {
                return;
            }

            _tapoffView = new UIView(PlatformView.Bounds);
            _tapoffView.Layer.Opacity = 0;

            if (_flyoutView is not null)
            {
                PlatformView.InsertSubviewBelow(_tapoffView, _flyoutView);
            }
            else
                PlatformView.AddSubview(_tapoffView);

            UpdateTapoffViewBackgroundColor();

            var recognizer = new UITapGestureRecognizer(t =>
            {
                IsOpen = false;
                LayoutSidebar(true);
            });

            _tapoffView.AddGestureRecognizer(recognizer);
        }

        void RemoveTapoffView()
        {
            if (_tapoffView is null)
            {
                return;
            }

            _tapoffView.RemoveFromSuperview();
            _tapoffView = null;
        }

        void UpdateTapoffViewBackgroundColor()
        {
            if (_tapoffView is null)
            {
                return;
            }

            _tapoffView.UpdateBackground(_backdropBrush);

            if (Brush.IsNullOrEmpty(_backdropBrush))
            {
                if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                {
                    _tapoffView.BackgroundColor = UIColor.Clear;
                }
                else
                {
                    _tapoffView.BackgroundColor = Microsoft.Maui.Platform.ColorExtensions.BackgroundColor.ColorWithAlpha(0.5f);
                }
            }
        }

        void UpdateFlyoutAccessibility()
        {
            bool flyoutElementsHidden = false;
            bool detailElementsHidden = false;

            switch (_flyoutBehavior)
            {
                case FlyoutBehavior.Flyout:
                    flyoutElementsHidden = !IsOpen;
                    detailElementsHidden = IsOpen;
                    break;
                case FlyoutBehavior.Locked:
                    flyoutElementsHidden = false;
                    detailElementsHidden = false;
                    break;
                case FlyoutBehavior.Disabled:
                    flyoutElementsHidden = true;
                    detailElementsHidden = false;
                    break;
            }

            if (_flyoutView is not null)
            {
                _flyoutView.AccessibilityElementsHidden = flyoutElementsHidden;
            }

            if (_detailView is not null)
            {
                // Wait until content is attached before hiding _detailView or accessibility can stay hidden forever.
                _detailView.AccessibilityElementsHidden = _currentShellItemRenderer is not null && detailElementsHidden;
            }
        }

        bool ShouldReceiveTouch(UITouch touch, UIView containerView)
        {
            CGPoint loc = touch.LocationInView(containerView);

            if (touch.View is UISlider ||
                touch.View is MPVolumeView ||
                IsSwipeView(touch.View) ||
                (loc.X > containerView.Frame.Width * 0.1 && !IsOpen))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
