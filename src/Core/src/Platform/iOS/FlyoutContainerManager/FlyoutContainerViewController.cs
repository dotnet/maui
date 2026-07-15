using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform;

/// <summary>
/// A minimal UIViewController that forwards lifecycle calls to <see cref="FlyoutContainerManager"/>.
/// The handler sets this as its ViewController so UIKit lifecycle events reach the manager.
/// </summary>
internal class FlyoutContainerViewController : UIViewController
{
    readonly WeakReference<FlyoutContainerManager> _managerRef;

    internal FlyoutContainerViewController(FlyoutContainerManager manager)
    {
        _managerRef = new WeakReference<FlyoutContainerManager>(manager);
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Set background so status bar area doesn't show black/clear behind the safe area offset
        View!.BackgroundColor = UIColor.SystemBackground;

        if (_managerRef.TryGetTarget(out var manager))
        {
            manager.SetupContainerViews(this);
        }
    }

    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);

        if (_managerRef.TryGetTarget(out var manager))
        {
            manager.OnViewDidAppear();
        }
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);

        if (_managerRef.TryGetTarget(out var manager))
        {
            manager.OnViewWillDisappear();
        }
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();

        if (_managerRef.TryGetTarget(out var manager))
        {
            manager.OnParentViewDidLayoutSubviews();
        }
    }

    public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
    {
        base.ViewWillTransitionToSize(toSize, coordinator);

        if (_managerRef.TryGetTarget(out var manager))
        {
            manager.OnParentViewWillTransitionToSize(toSize);
        }
    }

    public override UIViewController? ChildViewControllerForStatusBarHidden()
    {
        return GetActiveDetailViewController() ?? base.ChildViewControllerForStatusBarHidden();
    }

    public override UIViewController? ChildViewControllerForStatusBarStyle()
    {
        return GetActiveDetailViewController() ?? base.ChildViewControllerForStatusBarStyle();
    }

    public override UIViewController? ChildViewControllerForHomeIndicatorAutoHidden
    {
        get => GetActiveDetailViewController() ?? base.ChildViewControllerForHomeIndicatorAutoHidden;
    }

    // Always defer to the Detail VC, matching the legacy renderer — never the Flyout VC,
    // and never just "whichever child was added last" (that can be the Flyout VC if it's
    // re-added after Detail, silently breaking the visible page's status-bar/home-indicator
    // preferences).
    UIViewController? GetActiveDetailViewController() =>
        _managerRef.TryGetTarget(out var manager) ? manager.ActiveDetailViewController : null;
}
