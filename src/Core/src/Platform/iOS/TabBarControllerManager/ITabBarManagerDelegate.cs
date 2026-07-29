#nullable disable
using UIKit;

namespace Microsoft.Maui.Platform
{
    /// <summary>
    /// Delegate interface for consumer-specific tab bar behavior.
    /// Implemented by TabbedPage's handler to customize
    /// <see cref="TabBarControllerManager"/> behavior.
    /// </summary>
    internal interface ITabBarManagerDelegate
    {
        /// <summary>
        /// Called when a tab is selected by the user.
        /// The consumer should update its virtual view's current page.
        /// </summary>
        void OnTabSelected(int index);

        /// <summary>
        /// Called when the tab bar finishes customization (reordering).
        /// The consumer should update children order indices.
        /// </summary>
        void OnTabsReordered(UIViewController[] viewControllers);

        /// <summary>
        /// Returns the current page's view controller for status bar/home indicator delegation.
        /// </summary>
        UIViewController GetCurrentPageViewController();

        /// <summary>
        /// Called when the UITabBarController's view has appeared.
        /// </summary>
        void OnViewDidAppear();

        /// <summary>
        /// Called when the UITabBarController's view has disappeared.
        /// </summary>
        void OnViewDidDisappear();

        /// <summary>
        /// Called when the UITabBarController's view needs layout.
        /// </summary>
        void OnViewDidLayoutSubviews();

        /// <summary>
        /// Called when the trait collection changes (e.g. iPad rotation).
        /// Used to resize tab bar icons.
        /// </summary>
        void OnTraitCollectionDidChange(UITraitCollection previousTraitCollection);
    }
}
