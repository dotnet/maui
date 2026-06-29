#nullable disable
using System;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Handlers
{
    public partial class TabbedViewHandler : IPlatformViewHandler, ITabBarManagerDelegate
    {
        TabBarControllerManager _manager;

        /// <summary>
        /// Gets the UITabBarController managed by this handler.
        /// </summary>
        internal TabBarControllerManager Manager => _manager;

        /// <summary>
        /// Gets the TabBarController so the Controls layer can access UITabBar for appearance.
        /// </summary>
        internal UITabBarController TabBarController => _manager?.TabBarController;

        /// <summary>
        /// Sets the TabBarControllerManager. Called from the Controls layer PlatformViewFactory.
        /// </summary>
        internal void SetManager(TabBarControllerManager manager)
        {
            _manager = manager;
        }

        protected override void ConnectHandler(UIView platformView)
        {
            base.ConnectHandler(platformView);

            // Set the ViewController so this handler participates in the VC hierarchy
            ViewController = _manager?.TabBarController;
        }

        protected override void DisconnectHandler(UIView platformView)
        {
            _manager?.Dispose();
            _manager = null;
            ViewController = null;
            base.DisconnectHandler(platformView);
        }

        #region ITabBarManagerDelegate

        /// <summary>
        /// Indicates that the current MapCurrentPage call originated from a native tab tap,
        /// not from a programmatic CurrentPage change. Used by the Controls layer to
        /// determine sync direction (native→virtual vs virtual→native).
        /// </summary>
        internal bool NativeSelectionInProgress { get; private set; }

        void ITabBarManagerDelegate.OnTabSelected(int index)
        {
            // Set flag so MapCurrentPage knows to do native→virtual sync
            NativeSelectionInProgress = true;
            try
            {
                if (VirtualView is IElement element)
                {
                    element.Handler?.UpdateValue("CurrentPage");
                }
            }
            finally
            {
                NativeSelectionInProgress = false;
            }
        }

        void ITabBarManagerDelegate.OnTabsReordered(UIViewController[] viewControllers)
        {
            // Raise the manager's event so the Controls layer can update children order
            _manager?.RaiseTabsReordered(viewControllers);
        }

        UIViewController ITabBarManagerDelegate.GetCurrentPageViewController()
        {
            // Return the ViewController from the ITabbedView's current page
            // The Core layer can access ITabbedView but not Controls.TabbedPage directly
            return null;
        }

        void ITabBarManagerDelegate.OnViewDidAppear()
        {
            // Raise the manager's event so the Controls layer can call SendAppearing
            _manager?.RaiseViewDidAppear();
        }

        void ITabBarManagerDelegate.OnViewDidDisappear()
        {
            // Raise the manager's event so the Controls layer can call SendDisappearing
            _manager?.RaiseViewDidDisappear();
        }

        void ITabBarManagerDelegate.OnViewDidLayoutSubviews()
        {
            if (VirtualView is IView view && _manager?.View is UIView platformView)
                view.Arrange(platformView.Bounds.ToRectangle());
        }

        void ITabBarManagerDelegate.OnTraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            if (previousTraitCollection?.VerticalSizeClass == _manager?.TraitCollection.VerticalSizeClass)
                return;

            // Trigger icon resize by refreshing all tab bar items
            if (VirtualView is IElement element)
            {
                element.Handler?.UpdateValue("ItemsSource");
            }
        }

        #endregion

        #region IPlatformViewHandler

        UIView IPlatformViewHandler.PlatformView => PlatformView;

        UIView IPlatformViewHandler.ContainerView => ContainerView;

        UIViewController IPlatformViewHandler.ViewController => _manager?.TabBarController;

        #endregion
    }
}
