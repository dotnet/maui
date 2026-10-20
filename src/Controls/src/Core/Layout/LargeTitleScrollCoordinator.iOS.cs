#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls;

sealed class LargeTitleScrollCoordinator
{
	IView _topChild;
	IView _host;
	Rect _safeBounds;
	bool _arranging;
	bool _adjusted;
	WeakReference<UIScrollView> _scrollView;
	WeakReference<UIViewController> _owner;
	WeakReference<UIViewController> _childController;
	WeakReference<UIScrollView> _previousContentScrollView;
	WeakReference<Layout> _layout;
	WeakReference<VisualElement> _registeredHost;
	UIScrollViewContentInsetAdjustmentBehavior _previousBehavior;

	public static bool IsLargeTitleScroller(IView view) =>
#pragma warning disable CS0618 // Compatibility list controls remain supported.
		view is ListView or TableView;
#pragma warning restore CS0618

	public static bool IsEligible(Layout layout, Rect bounds)
	{
		if (!OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalyst() ||
			layout.Parent is not ContentPage ||
			layout is not ISafeAreaView2 { HasExplicitSafeAreaEdges: false } safeArea ||
			safeArea.GetSafeAreaRegionsForEdge(1) != SafeAreaRegions.Container ||
			layout.Handler?.PlatformView is not UIView root ||
			Math.Abs(bounds.Top - root.SafeAreaInsets.Top) > 1)
			return false;

		var controller = root.FindResponder<UIViewController>();
		var navigation = controller?.NavigationController;
		return navigation is Microsoft.Maui.Handlers.INavigationViewHandler &&
			navigation.TopViewController?.View is { } pageView && root.IsDescendantOfView(pageView) &&
			navigation.NavigationBar is { Hidden: false, Translucent: true, PrefersLargeTitles: true } &&
			navigation.TopViewController?.NavigationItem.LargeTitleDisplayMode != UINavigationItemLargeTitleDisplayMode.Never;
	}

	public void Prepare(Layout layout, Rect bounds, IView host)
	{
		_host = host;
		_topChild = host;
		while (_topChild.Parent is IView parent && parent != layout)
			_topChild = parent;

		_safeBounds = bounds;
		_adjusted = false;
		_arranging = true;
	}

	public void AdjustBounds(IView child, ref Rect bounds)
	{
		if (!_arranging || child != _topChild ||
			Math.Abs(bounds.Top - _safeBounds.Top) > 1 ||
			bounds.Height < _safeBounds.Height / 2 || bounds.Height > _safeBounds.Height + 1)
			return;

		bounds = new Rect(bounds.X, 0, bounds.Width, bounds.Height + _safeBounds.Top);
		_adjusted = true;
	}

	public void CompleteArrange()
	{
		_arranging = false;
		var host = _host;
		var topChild = _topChild;
		_host = null;
		_topChild = null;
		if (!_adjusted || host?.Handler?.PlatformView is not UIScrollView scrollView ||
			topChild.Parent?.Handler?.PlatformView is not UIView root ||
			root.FindResponder<UIViewController>() is not { } pageController ||
			pageController.NavigationController?.TopViewController is not { } owner)
		{
			Reset();
			return;
		}

		if (_scrollView?.TryGetTarget(out var current) == true && current == scrollView &&
			_owner?.TryGetTarget(out var currentOwner) == true && currentOwner == owner &&
			(host.Handler is not IPlatformViewHandler { ViewController: { } controller } ||
				controller.ParentViewController == pageController))
			return;

		Reset();
		_scrollView = new(scrollView);
		_previousBehavior = scrollView.ContentInsetAdjustmentBehavior;
		_owner = new(owner);
		var previous = owner.GetContentScrollView(NSDirectionalRectEdge.Top);
		if (previous is not null && previous != scrollView)
			_previousContentScrollView = new(previous);

		// A UITableViewController needs containment to inherit the page's safe area.
		if (host.Handler is IPlatformViewHandler { ViewController: { ParentViewController: null } child })
		{
			pageController.AddChildViewController(child);
			child.DidMoveToParentViewController(pageController);
			_childController = new(child);
		}

		scrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Always;
		owner.SetContentScrollView(scrollView, NSDirectionalRectEdge.Top);
		if (topChild.Parent is Layout layout && host is VisualElement element)
		{
			_layout = new(layout);
			_registeredHost = new(element);
			layout.DescendantRemoved += OnDescendantRemoved;
			element.HandlerChanging += OnHostHandlerChanging;
		}
	}

	void OnDescendantRemoved(object sender, ElementEventArgs args)
	{
		if (_registeredHost?.TryGetTarget(out var host) == true && args.Element == host)
			Reset();
	}

	void OnHostHandlerChanging(object sender, HandlerChangingEventArgs args) => Reset();

	public void Reset()
	{
		if (_layout?.TryGetTarget(out var layout) == true)
			layout.DescendantRemoved -= OnDescendantRemoved;
		if (_registeredHost?.TryGetTarget(out var host) == true)
			host.HandlerChanging -= OnHostHandlerChanging;
		_layout = null;
		_registeredHost = null;

		if (_scrollView?.TryGetTarget(out var scrollView) == true && scrollView.Handle != IntPtr.Zero)
			scrollView.ContentInsetAdjustmentBehavior = _previousBehavior;

		if (_owner?.TryGetTarget(out var owner) == true && owner.Handle != IntPtr.Zero &&
			_scrollView?.TryGetTarget(out var registered) == true &&
			owner.GetContentScrollView(NSDirectionalRectEdge.Top) == registered)
		{
			var previous = _previousContentScrollView?.TryGetTarget(out var p) == true ? p : null;
			owner.SetContentScrollView(previous, NSDirectionalRectEdge.Top);
		}

		if (_childController?.TryGetTarget(out var child) == true && child.Handle != IntPtr.Zero)
		{
			child.WillMoveToParentViewController(null);
			child.RemoveFromParentViewController();
		}

		_scrollView = null;
		_owner = null;
		_childController = null;
		_previousContentScrollView = null;
		_host = null;
		_topChild = null;
		_arranging = false;
	}
}
