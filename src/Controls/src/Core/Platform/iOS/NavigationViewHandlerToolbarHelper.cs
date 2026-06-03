#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using CoreGraphics;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using UIKit;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Controls
{
    /// <summary>
    /// Wrapper VC used by NavigationViewHandler (handler architecture).
    /// Mirrors the renderer's ParentingViewController: manages toolbar items,
    /// nav bar visibility, back button, title, and per-page property changes.
    /// </summary>
    sealed class NavigationHandlerParentingViewController : UIViewController
    {
        WeakReference<Page>? _childRef;
        ToolbarTracker _tracker = new();
        List<ToolbarItem> _trackedToolbarItems = new();
        bool _toolbarUpdatePending;
        bool _disposed;

        public NavigationHandlerParentingViewController()
        {
        }

        public Page? Child
        {
            get => _childRef?.TryGetTarget(out var p) == true ? p : null;
            set
            {
                var old = Child;

                if (old == value)
                {
                    return;
                }

                old?.PropertyChanged -= HandleChildPropertyChanged;

                if (value is not null)
                {
                    _childRef = new WeakReference<Page>(value);
                    value.PropertyChanged += HandleChildPropertyChanged;
                }
                else
                {
                    _childRef = null;
                }

                UpdateHasBackButton();
                UpdateLargeTitles();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (Child is Page child)
            {
                var parentPages = child.GetParentPages();
                var flyoutPageWithToolbarItems = FindFlyoutPageWithToolbarItems(parentPages);

                if (flyoutPageWithToolbarItems is not null)
                {
                    _tracker.Target = flyoutPageWithToolbarItems.Flyout;
                    var additionalTargets = new List<Page>(parentPages) { child };
                    _tracker.AdditionalTargets = additionalTargets;
                }
                else
                {
                    _tracker.Target = child;
                    _tracker.AdditionalTargets = parentPages;
                }

                _tracker.CollectionChanged += TrackerOnCollectionChanged;

                NavigationItem.Title = child.Title;
                UpdateBackButtonTitle();
                UpdateToolbarItems();
            }
        }

        public override UIViewController ChildViewControllerForHomeIndicatorAutoHidden =>
            (Child?.Handler as IPlatformViewHandler)?.ViewController ?? this;

        public override UIViewController ChildViewControllerForStatusBarHidden() =>
            (Child?.Handler as IPlatformViewHandler)?.ViewController ?? this;

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            if (Child?.Handler is IPlatformViewHandler ivh)
                return ivh.ViewController!.GetSupportedInterfaceOrientations();
            return base.GetSupportedInterfaceOrientations();
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            if (Child?.Handler is IPlatformViewHandler ivh)
                return ivh.ViewController!.PreferredInterfaceOrientationForPresentation();
            return base.PreferredInterfaceOrientationForPresentation();
        }

#pragma warning disable CA1422 // ShouldAutorotate is deprecated on iOS 16+
        public override bool ShouldAutorotate()
        {
            if (Child?.Handler is IPlatformViewHandler ivh)
                return ivh.ViewController!.ShouldAutorotate();
            return base.ShouldAutorotate();
        }
#pragma warning restore CA1422

        [System.Runtime.Versioning.UnsupportedOSPlatform("ios6.0")]
        [System.Runtime.Versioning.UnsupportedOSPlatform("tvos")]
        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            if (Child?.Handler is IPlatformViewHandler ivh)
                return ivh.ViewController!.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
            return base.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
        }

        public override bool ShouldAutomaticallyForwardRotationMethods => true;

        public override void ViewWillAppear(bool animated)
        {
            UpdateNavigationBarVisibility(animated);

            // Match renderer behavior: when the nav bar is opaque, prevent content
            // from extending underneath it. When translucent, allow full extension.
            var isTranslucent = NavigationController?.NavigationBar.Translucent ?? false;
            EdgesForExtendedLayout = isTranslucent ? UIRectEdge.All : UIRectEdge.None;

            // Re-evaluate per-page IconColor when this page becomes visible
            // (push or pop-back). IconColor is already set before the push,
            // so HandleChildPropertyChanged won't fire — we need this trigger.
            UpdateIconColor();

            base.ViewWillAppear(animated);
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            var childView = (Child?.Handler as IPlatformViewHandler)?.ViewController?.View;
            childView?.Frame = View!.Bounds;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Force redraw for right toolbar items to prevent them being grayed out
            // after canceling swipe-to-go-back
            if (NavigationItem?.RightBarButtonItems is UIBarButtonItem[] items)
            {
                foreach (var item in items)
                {
                    if (item.Image is not null)
                    {
                        continue;
                    }

                    var tintColor = item.TintColor;
                    item.TintColor = tintColor is null ? UIColor.Clear : null;
                    item.TintColor = tintColor;
                }
            }
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);

            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad &&
                (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26)))
            {
                coordinator.AnimateAlongsideTransition(_ =>
                {
                    UpdateTitleViewFrameForOrientation();
                }, null);
            }
        }

#pragma warning disable CA1422 // TraitCollectionDidChange is deprecated on iOS 17+
        public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if ((OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26)) &&
                (previousTraitCollection?.VerticalSizeClass != TraitCollection.VerticalSizeClass ||
                 previousTraitCollection?.HorizontalSizeClass != TraitCollection.HorizontalSizeClass))
            {
                UpdateTitleViewFrameForOrientation();
            }
        }
#pragma warning restore CA1422

        void UpdateTitleViewFrameForOrientation()
        {
            if (NavigationItem?.TitleView is not UIView titleView)
            {
                return;
            }

            if (NavigationController?.NavigationBar is UINavigationBar navBar)
            {
                var frame = navBar.Frame;
                titleView.Frame = new RectangleF(0, 0, frame.Width, frame.Height);
                titleView.LayoutIfNeeded();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                ClearTitleViewContainer();
                CleanToolbarItems();

                if (Child is Page child)
                {
                    child.PropertyChanged -= HandleChildPropertyChanged;
                    _childRef = null;
                }

                if (_tracker is not null)
                {
                    _tracker.Target = null;
                    _tracker.CollectionChanged -= TrackerOnCollectionChanged;
                    _tracker = null!;
                }
            }

            base.Dispose(disposing);
        }

        void HandleChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == NavigationPage.HasNavigationBarProperty.PropertyName)
            {
                UpdateNavigationBarVisibility(true);
            }
            else if (e.PropertyName == Page.TitleProperty.PropertyName)
            {
                NavigationItem.Title = Child?.Title;
            }
            else if (e.PropertyName == NavigationPage.HasBackButtonProperty.PropertyName)
            {
                UpdateHasBackButton();
            }
            else if (e.PropertyName == NavigationPage.BackButtonTitleProperty.PropertyName)
            {
                UpdateBackButtonTitle();
            }
            else if (e.PropertyName == PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty.PropertyName)
            {
                UpdateLargeTitles();
            }
            else if (e.PropertyName == NavigationPage.IconColorProperty.PropertyName)
            {
                UpdateIconColor();
            }
            else if (e.PropertyName == NavigationPage.BackButtonAccessibilityLabelProperty.PropertyName)
            {
                UpdateBackButtonTitle();
            }
            else if (e.PropertyName == NavigationPage.TitleViewProperty.PropertyName ||
                     e.PropertyName == NavigationPage.TitleIconImageSourceProperty.PropertyName)
            {
                UpdateTitleArea();
            }
        }

        void UpdateNavigationBarVisibility(bool animated)
        {
            if (Child is not Page current || NavigationController is null)
            {
                return;
            }

            var hasNavBar = NavigationPage.GetHasNavigationBar(current);

            if (NavigationController.NavigationBarHidden == hasNavBar)
            {
                current.IgnoresContainerArea = !hasNavBar;
                NavigationController.SetNavigationBarHidden(!hasNavBar, animated);
            }
        }

        static FlyoutPage? FindFlyoutPageWithToolbarItems(IEnumerable<Page> parentPages)
        {
            foreach (var page in parentPages)
            {
                if (page is FlyoutPage flyoutPage && flyoutPage.Flyout?.ToolbarItems?.Count > 0)
                {
                    return flyoutPage;
                }
            }

            return null;
        }

        void UpdateHasBackButton()
        {
            if (Child is not Page child)
            {
                return;
            }

            NavigationItem.HidesBackButton = !NavigationPage.GetHasBackButton(child);
        }

        void UpdateBackButtonTitle()
        {
            if (Child is not Page child)
            {
                return;
            }

            var backButtonTitle = NavigationPage.GetBackButtonTitle(child);
            var backButtonAccessibilityLabel = NavigationPage.GetBackButtonAccessibilityLabel(child);

            if (backButtonTitle is not null || !string.IsNullOrEmpty(backButtonAccessibilityLabel))
            {
                var barButtonItem = new UIBarButtonItem { Title = backButtonTitle, Style = UIBarButtonItemStyle.Plain };

                if (!string.IsNullOrEmpty(backButtonAccessibilityLabel))
                {
                    barButtonItem.AccessibilityLabel = backButtonAccessibilityLabel;
                }

                NavigationItem.BackBarButtonItem = barButtonItem;
            }
            else
            {
                NavigationItem.BackBarButtonItem = null;
            }
        }

        void UpdateLargeTitles()
        {
            if (Child is not Page page || !OperatingSystem.IsIOSVersionAtLeast(11))
            {
                return;
            }

            var mode = PlatformConfiguration.iOSSpecific.Page.GetLargeTitleDisplay(page);

            NavigationItem.LargeTitleDisplayMode = mode switch
            {
                PlatformConfiguration.iOSSpecific.LargeTitleDisplayMode.Always => UINavigationItemLargeTitleDisplayMode.Always,
                PlatformConfiguration.iOSSpecific.LargeTitleDisplayMode.Never => UINavigationItemLargeTitleDisplayMode.Never,
                _ => UINavigationItemLargeTitleDisplayMode.Automatic,
            };
        }

        void UpdateIconColor()
        {
            // Per-page IconColor changes the nav bar tint. Re-trigger the NavigationPage's
            // BarTextColor mapper which handles both BarTextColor and per-page IconColor.
            if (Child?.Parent is NavigationPage navPage && navPage.Handler is IElementHandler handler)
            {
                handler.UpdateValue(NavigationPage.BarTextColorProperty.PropertyName);
            }
        }

        void UpdateTitleArea()
        {
            if (Child is not Page page)
            {
                return;
            }

            var titleView = NavigationPage.GetTitleView(page);
            var titleIcon = NavigationPage.GetTitleIconImageSource(page);
            bool needContainer = titleView is not null || titleIcon is not null;

            ClearTitleViewContainer();

            if (needContainer)
            {
                // Try the VC's NavigationController first; fall back to the
                // NavigationPage handler's controller (available before push).
                var navBar = NavigationController?.NavigationBar;
                if (navBar is null &&
                    page.Parent is NavigationPage navPage &&
                    navPage.Handler is IPlatformViewHandler pvh &&
                    pvh.ViewController is UINavigationController nc)
                {
                    navBar = nc.NavigationBar;
                }

                if (navBar is null)
                {
                    return;
                }

                var container = new TitleViewContainer(titleView, navBar);

                if (titleIcon is not null && !titleIcon.IsEmpty)
                {
                    var mauiContext = page.FindMauiContext();
                    if (mauiContext is not null)
                    {
                        titleIcon.LoadImage(mauiContext, result =>
                        {
                            var image = result?.Value;

                            if (image is not null)
                            {
                                container.Icon = new UIImageView(image);
                            }
                        });
                    }
                }

                NavigationItem.TitleView = container;
            }
        }

        void ClearTitleViewContainer()
        {
            if (NavigationItem.TitleView is TitleViewContainer container)
            {
                container.Dispose();
                NavigationItem.TitleView = null;
            }
        }

        void TrackerOnCollectionChanged(object? sender, EventArgs e) => UpdateToolbarItems();

        void OnToolbarItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName ||
                e.PropertyName == MenuItem.TextProperty.PropertyName ||
                e.PropertyName == MenuItem.IconImageSourceProperty.PropertyName)
            {
                if (!_toolbarUpdatePending)
                {
                    _toolbarUpdatePending = true;
                    BeginInvokeOnMainThread(() =>
                    {
                        _toolbarUpdatePending = false;

                        if (!_disposed)
                        {
                            UpdateToolbarItems();
                        }
                    });
                }
            }
        }

        void CleanToolbarItems()
        {
            foreach (var item in _trackedToolbarItems)
            {
                item.PropertyChanged -= OnToolbarItemPropertyChanged;
            }

            _trackedToolbarItems.Clear();
        }

        void UpdateToolbarItems()
        {
            CleanToolbarItems();

            if (NavigationItem.RightBarButtonItems is UIBarButtonItem[] oldItems)
            {
                foreach (var item in oldItems)
                {
                    item.Dispose();
                }
            }

            if (ToolbarItems is UIBarButtonItem[] oldToolbar)
            {
                foreach (var item in oldToolbar)
                {
                    item.Dispose();
                }
            }

            List<UIBarButtonItem>? primaries = null;
            List<UIMenuElement>? secondaries = null;
            var toolbarItems = _tracker.ToolbarItems;

            foreach (var item in toolbarItems)
            {
                item.PropertyChanged += OnToolbarItemPropertyChanged;
                _trackedToolbarItems.Add(item);

                if (item.Order == ToolbarItemOrder.Secondary)
                {
                    (secondaries ??= new()).Add(item.ToSecondarySubToolbarItem().PlatformAction);
                }
                else
                {
                    (primaries ??= new()).Add(item.ToUIBarButtonItem());
                }
            }

            primaries?.Reverse();

            if (secondaries is not null && secondaries.Count > 0)
            {
                var menuIcon = UIImage.GetSystemImage("ellipsis.circle");
                var menu = UIMenu.Create(string.Empty, null, UIMenuIdentifier.Edit,
                    UIMenuOptions.DisplayInline, secondaries.ToArray());
                var menuButton = new UIBarButtonItem(menuIcon, menu)
                {
                    AccessibilityIdentifier = "SecondaryToolbarMenuButton"
                };

                primaries ??= new();
                primaries.Insert(0, menuButton);
            }

            NavigationItem.SetRightBarButtonItems(
                primaries is not null ? primaries.ToArray() : Array.Empty<UIBarButtonItem>(), false);

            // iOS 26+ tint fix
            if ((OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
                && primaries is not null
                && NavigationController?.NavigationBar?.TintColor is UIColor tintColor)
            {
                foreach (var item in primaries)
                {
                    item.TintColor = tintColor;
                }
            }
        }

        /// <summary>
        /// Factory method: creates a ParentingViewController wrapping the given page.
        /// Called from NavigationViewHandler.ConfigureViewController.
        /// </summary>
        internal static UIViewController CreateForPage(Page page, IMauiContext mauiContext)
        {
            _ = page.ToPlatform(mauiContext);

            var pack = new NavigationHandlerParentingViewController { Child = page };

            pack.UpdateTitleArea();

            var pageHandler = (IPlatformViewHandler)page.Handler!;
            pack.View!.AddSubview(pageHandler.ViewController!.View!);
            pack.AddChildViewController(pageHandler.ViewController);
            pageHandler.ViewController.DidMoveToParentViewController(pack);

            return pack;
        }
    }

    /// <summary>
    /// Controls-layer bridge: provides the CreateViewControllerForPage callback to NavigationViewHandler.
    /// </summary>
    static class NavigationViewHandlerToolbarHelper
    {
        internal static UIViewController CreateViewControllerForPage(IView view, IMauiContext context)
        {
            if (view is Page page)
            {
                return NavigationHandlerParentingViewController.CreateForPage(page, context);
            }

            // Fallback for non-Page views
            var handler = view.ToHandler(context);
            return handler.ViewController ?? new Maui.Handlers.NavigationViewHandler.ContainerViewController(view, (IPlatformViewHandler)handler);
        }
    }

    /// <summary>
    /// UIView wrapper that hosts a MAUI TitleView (and optional TitleIcon) as UINavigationItem.TitleView.
    /// Mirrors the renderer's Container class with simplified layout logic.
    /// </summary>
    sealed class TitleViewContainer : UIView
    {
        View? _view;
        IPlatformViewHandler? _child;
        UIImageView? _icon;
        bool _disposed;

        internal TitleViewContainer(View? view, UINavigationBar bar) : base(bar.Bounds)
        {
            if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
            {
                TranslatesAutoresizingMaskIntoConstraints = true;
                AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
                var frame = bar.Frame;

                if (frame != CGRect.Empty)
                {
                    Frame = new RectangleF(0, 0, frame.Width, frame.Height);
                }
            }
            else if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
            {
                TranslatesAutoresizingMaskIntoConstraints = false;
            }
            else
            {
                TranslatesAutoresizingMaskIntoConstraints = true;
                AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            }

            ClipsToBounds = true;

            if (view is not null)
            {
                _view = view;
                if (_view.Parent is null)
                {
                    _view.ParentSet += OnTitleViewParentSet;
                }
                else
                {
                    SetupTitleView();
                }
            }
        }

        internal UIImageView? Icon
        {
            get => _icon;
            set
            {
                _icon?.RemoveFromSuperview();
                _icon?.Dispose();
                _icon = value;

                if (_icon is not null)
                {
                    AddSubview(_icon);
                }

                SetNeedsLayout();
            }
        }

        void OnTitleViewParentSet(object? sender, EventArgs e)
        {
            if (sender is View view)
            {
                view.ParentSet -= OnTitleViewParentSet;
            }

            SetupTitleView();
        }

        void SetupTitleView()
        {
            var mauiContext = _view?.FindMauiContext();
            if (_view is not null && mauiContext is not null)
            {
                var platformView = _view.ToPlatform(mauiContext);
                _child = (IPlatformViewHandler?)_view.Handler;
                AddSubview(platformView);
            }
        }

        nfloat ToolbarHeight
        {
            get
            {
                if (Superview?.Bounds.Height > 0)
                {
                    return Superview.Bounds.Height;
                }

                return (Devices.DeviceInfo.Idiom == Devices.DeviceIdiom.Phone && Devices.DeviceDisplay.MainDisplayInfo.Orientation.IsLandscape()) ? 32 : 44;
            }
        }

        nfloat IconHeight => _icon?.Frame.Height ?? 0;
        nfloat IconWidth => _icon?.Frame.Width ?? 0;

        public override CGSize IntrinsicContentSize => UILayoutFittingExpandedSize;

        public override CGSize SizeThatFits(CGSize size)
        {
            return new CGSize(size.Width, ToolbarHeight);
        }

        public override UIEdgeInsets AlignmentRectInsets
        {
            get
            {
                if (_child?.VirtualView is IView view)
                {
                    var margin = view.Margin;
                    return new UIEdgeInsets(-(nfloat)margin.Top, -(nfloat)margin.Left, -(nfloat)margin.Bottom, -(nfloat)margin.Right);
                }

                return base.AlignmentRectInsets;
            }
        }

        public override CGRect Frame
        {
            get => base.Frame;
            set
            {
                if (Superview is not null)
                {
                    if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26) ||
                        !(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)))
                    {
                        value.Y = Superview.Bounds.Y;
                    }

                    value.Height = ToolbarHeight;

                    if (_child?.VirtualView is IView marginView)
                    {
                        var verticalMargin = (nfloat)(marginView.Margin.Top + marginView.Margin.Bottom);
                        value.Height = (nfloat)Math.Max(0, value.Height - verticalMargin);
                    }
                }

                base.Frame = value;
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (Frame == CGRect.Empty || Frame.Width >= 10000 || Frame.Height >= 10000)
            {
                return;
            }

            nfloat toolbarHeight = ToolbarHeight;
            double height = Math.Min(toolbarHeight, Bounds.Height);
            nfloat iconWidth = IconWidth;

            if (_icon is not null)
            {
                _icon.Frame = new RectangleF(0, 0, IconWidth, (nfloat)Math.Min(toolbarHeight, IconHeight));
            }

            if (_child?.VirtualView is IView view)
            {
                var layoutBounds = new Rect(iconWidth, 0, Bounds.Width - iconWidth, height);

                if (view.HorizontalLayoutAlignment != Primitives.LayoutAlignment.Fill ||
                    view.VerticalLayoutAlignment != Primitives.LayoutAlignment.Fill)
                {
                    view.Measure(Bounds.Width, Bounds.Height);
                    layoutBounds = view.ComputeFrame(new Rect(0, 0, Bounds.Width, Bounds.Height));
                }

                _child.PlatformArrangeHandler(layoutBounds);
            }
            else if (_icon is not null && Superview is not null)
            {
                _icon.Center = new PointF(Superview.Frame.Width / 2 - Frame.X, Superview.Frame.Height / 2);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                if (_child?.IsConnected() == true)
                {
                    (_child.ContainerView ?? _child.PlatformView)?.RemoveFromSuperview();
                    _child.DisconnectHandler();
                    _child = null;
                }

                if (_view is not null)
                {
                    _view.ParentSet -= OnTitleViewParentSet;
                }
                _view = null;

                _icon?.Dispose();
                _icon = null;
            }

            base.Dispose(disposing);
        }
    }
}
