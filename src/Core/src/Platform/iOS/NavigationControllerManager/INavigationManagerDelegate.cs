#nullable enable
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui.Platform
{
    /// <summary>
    /// Delegate interface for consumer-specific navigation behavior.
    /// Currently implemented by NavigationViewHandler to customize
    /// <see cref="NavigationControllerManager"/>.
    /// </summary>
    /// <remarks>
    /// In the future, Shell's handler could also implement this interface
    /// to reuse the same UINavigationController management infrastructure.
    /// </remarks>
    internal interface INavigationManagerDelegate
    {
        /// <summary>
        /// Called when the navigation bar should be shown/hidden for a view controller.
        /// The consumer determines visibility based on its own rules
        /// (Shell: Shell.GetNavBarIsVisible, NavigationPage: HasNavigationBar).
        /// Returns (isHidden, animate).
        /// </summary>
        (bool isHidden, bool animate) GetNavigationBarVisibility(UIViewController viewController);

        /// <summary>
        /// Called when the user taps the back button or initiates an interactive pop.
        /// Return true to allow the pop, false to cancel it.
        /// </summary>
        bool ShouldPop();

        /// <summary>
        /// Called after a push navigation completes (DidShowViewController fires).
        /// Allows the consumer to perform post-push work (appearance updates, etc.).
        /// </summary>
        void OnNavigationComplete(UINavigationController navigationController, UIViewController viewController);

        /// <summary>
        /// Called before a view controller is shown (WillShowViewController fires).
        /// Allows the consumer to set toolbar items early to avoid flickering.
        /// </summary>
        void OnWillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated);

        /// <summary>
        /// Called when an interactive pop gesture completes (not cancelled).
        /// The consumer should update its virtual navigation stack accordingly.
        /// </summary>
        void OnInteractivePopCompleted();

        /// <summary>
        /// Called when the managed UINavigationController's ViewDidAppear fires.
        /// This indicates the VC is fully visible and its View is in the UIWindow.
        /// Used to re-evaluate loaded state on the NavigationPage element when hosted
        /// inside a parent container (UITabBarController, FlyoutPage) where the initial
        /// KVO-based loaded watcher may not fire reliably.
        /// </summary>
        void OnNavigationControllerDidAppear();

        /// <summary>
        /// Called when the managed UINavigationController's ViewDidDisappear fires.
        /// This indicates the VC is no longer visible (e.g., tab switched away).
        /// Used to fire Page.Disappearing on the NavigationPage element.
        /// </summary>
        void OnNavigationControllerDidDisappear();

        /// <summary>
        /// Called when the managed UINavigationController's ViewDidLayoutSubviews fires.
        /// The consumer should arrange its virtual view to match the UIKit layout bounds.
        /// This ensures the MAUI element's Frame reflects the actual platform layout,
        /// which is required for loaded gates that check Frame validity.
        /// </summary>
        void OnViewDidLayoutSubviews(CoreGraphics.CGRect bounds);
    }
}
