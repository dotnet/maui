#nullable disable
using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
    /// <summary>
    /// Manages a UITabBarController for TabbedPage's handler architecture on iOS.
    /// Owns the UITabBarController and provides tab management operations,
    /// tab bar appearance, iOS 18 compatibility fixes, and lifecycle handling.
    /// </summary>
    internal class TabBarControllerManager : IDisposable
    {
        readonly ITabBarManagerDelegate _delegate;
        readonly MauiTabBarController _tabBarController;
        bool _disposed;

        /// <summary>
        /// Gets the managed UITabBarController instance.
        /// </summary>
        public UITabBarController TabBarController => _tabBarController;

        /// <summary>
        /// Gets the TabBar from the managed UITabBarController.
        /// </summary>
        public UITabBar TabBar => _tabBarController.TabBar;

        /// <summary>
        /// Gets the View from the managed UITabBarController.
        /// </summary>
        public UIView View => _tabBarController.View;

        /// <summary>
        /// Gets or sets the view controllers displayed as tabs.
        /// </summary>
        public UIViewController[] ViewControllers
        {
            get => _tabBarController.ViewControllers;
            set
            {
                _tabBarController.ViewControllers = value;

                // UIKit resets CustomizableViewControllers to all VCs on each assignment.
                // Disable tab reordering to match renderer behavior.
                _tabBarController.CustomizableViewControllers = null;
            }
        }

        /// <summary>
        /// Gets or sets the currently selected view controller.
        /// </summary>
        public UIViewController SelectedViewController
        {
            get => _tabBarController.SelectedViewController;
            set => _tabBarController.SelectedViewController = value;
        }

        /// <summary>
        /// Gets the index of the currently selected tab.
        /// </summary>
        public nint SelectedIndex => _tabBarController.SelectedIndex;

        /// <summary>
        /// Gets or sets the customizable view controllers (set to null to disable tab reordering).
        /// </summary>
        public UIViewController[] CustomizableViewControllers
        {
            get => _tabBarController.CustomizableViewControllers;
            set => _tabBarController.CustomizableViewControllers = value;
        }

        /// <summary>
        /// Gets the MoreNavigationController for tab overflow (>5 tabs).
        /// </summary>
        public UINavigationController MoreNavigationController => _tabBarController.MoreNavigationController;

        /// <summary>
        /// Gets the trait collection from the UITabBarController.
        /// </summary>
        public UITraitCollection TraitCollection => _tabBarController.TraitCollection;

        public TabBarControllerManager(ITabBarManagerDelegate managerDelegate)
        {
            _delegate = managerDelegate ?? throw new ArgumentNullException(nameof(managerDelegate));
            _tabBarController = new MauiTabBarController(this);
        }

        /// <summary>
        /// Raised when the UITabBarController's ViewDidAppear is called.
        /// The Controls layer subscribes to this to send Page.Appearing.
        /// </summary>
        internal event EventHandler ViewDidAppear;

        /// <summary>
        /// Raised when the UITabBarController's ViewDidDisappear is called.
        /// The Controls layer subscribes to this to send Page.Disappearing.
        /// </summary>
        internal event EventHandler ViewDidDisappear;

        /// <summary>
        /// Callback set by the Controls layer to return the current page's ViewController.
        /// Used for status bar and home indicator delegation.
        /// </summary>
        internal Func<UIViewController> GetCurrentPageViewControllerFunc { get; set; }

        /// <summary>
        /// Raised when tabs are reordered by the user.
        /// The Controls layer subscribes to update children order indices.
        /// </summary>
        internal event Action<UIViewController[]> TabsReordered;

        /// <summary>
        /// Ensures the tab bar remains visible on MacCatalyst 18+.
        /// DisableiOS18ToolbarTabs() sets Mode = TabSidebar which causes iOS
        /// to set TabBar.Hidden = true and Alpha = 0. This overrides that behavior.
        /// </summary>
        public void UpdateTabBarVisibility()
        {
            if (TabBar is null)
            {
                return;
            }

            if (OperatingSystem.IsMacCatalystVersionAtLeast(18) || OperatingSystem.IsIOSVersionAtLeast(18))
            {
#if MACCATALYST
				if (TabBar.Hidden || TabBar.Alpha != 1.0f)
				{
					TabBar.Alpha = 1.0f;
					TabBar.Hidden = false;
				}
#endif
            }
        }

        internal void RaiseViewDidAppear() => ViewDidAppear?.Invoke(this, EventArgs.Empty);
        internal void RaiseViewDidDisappear() => ViewDidDisappear?.Invoke(this, EventArgs.Empty);
        internal void RaiseTabsReordered(UIViewController[] viewControllers) => TabsReordered?.Invoke(viewControllers);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _tabBarController?.Dispose();
        }

        sealed class MauiTabBarController : UITabBarController
        {
            readonly WeakReference<TabBarControllerManager> _managerRef;

            public MauiTabBarController(TabBarControllerManager manager)
            {
                _managerRef = new WeakReference<TabBarControllerManager>(manager);

                // Apply iOS 18 tab bar fixes
                this.DisableiOS18ToolbarTabs();

                // Subscribe to tab reordering events
                FinishedCustomizingViewControllers += HandleFinishedCustomizingViewControllers;
            }

            public override UIViewController SelectedViewController
            {
                get => base.SelectedViewController;
                set
                {
                    base.SelectedViewController = value;

                    // If the selected view controller is the "More" navigation controller,
                    // do not update the current page
                    if (value == MoreNavigationController)
                    {
                        return;
                    }

                    if (_managerRef is not null && _managerRef.TryGetTarget(out var manager))
                    {
                        var index = (int)SelectedIndex;
                        manager._delegate.OnTabSelected(index);
                    }
                }
            }

            public override UIViewController ChildViewControllerForStatusBarHidden()
            {
                if (_managerRef is not null && _managerRef.TryGetTarget(out var manager))
                    return manager.GetCurrentPageViewControllerFunc?.Invoke()
                        ?? manager._delegate.GetCurrentPageViewController();

                return null;
            }

            public override UIViewController ChildViewControllerForHomeIndicatorAutoHidden
            {
                get
                {
                    if (_managerRef is not null && _managerRef.TryGetTarget(out var manager))
                        return manager.GetCurrentPageViewControllerFunc?.Invoke()
                            ?? manager._delegate.GetCurrentPageViewController();

                    return null;
                }
            }

            public override void ViewDidAppear(bool animated)
            {
                base.ViewDidAppear(animated);

                if (_managerRef is not null && _managerRef.TryGetTarget(out var manager))
                {
                    manager._delegate.OnViewDidAppear();
                }
            }

            public override void ViewDidDisappear(bool animated)
            {
                base.ViewDidDisappear(animated);

                if (_managerRef is not null && _managerRef.TryGetTarget(out var manager))
                {
                    manager._delegate.OnViewDidDisappear();
                }
            }

            public override void ViewDidLayoutSubviews()
            {
                base.ViewDidLayoutSubviews();

                if (_managerRef is not null && _managerRef.TryGetTarget(out var manager))
                {
                    manager._delegate.OnViewDidLayoutSubviews();
                }
            }

            public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
            {
#pragma warning disable CA1422 // Validate platform compatibility
                base.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422

                if (_managerRef is not null && _managerRef.TryGetTarget(out var manager))
                {
                    manager._delegate.OnTraitCollectionDidChange(previousTraitCollection);
                }
            }

            void HandleFinishedCustomizingViewControllers(object sender, UITabBarCustomizeChangeEventArgs e)
            {
                if (e.Changed && _managerRef is not null && _managerRef.TryGetTarget(out var manager))
                {
                    manager._delegate.OnTabsReordered(e.ViewControllers);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    FinishedCustomizingViewControllers -= HandleFinishedCustomizingViewControllers;
                }

                base.Dispose(disposing);
            }
        }
    }
}
