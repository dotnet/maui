#nullable disable
using System;
using CoreGraphics;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls;

sealed class SafeAreaScrollViewCoordinator
{
	WeakReference<UIScrollView> _delegatedNativeScrollView;
	WeakReference<ISafeAreaScrollViewContainer> _delegatedScrollContainer;
	WeakReference<UIViewController> _contentScrollViewOwner;
	WeakReference<UIScrollView> _previousContentScrollView;
	WeakReference<UIScrollView> _registeredContentScrollView;
	UIScrollViewContentInsetAdjustmentBehavior _previousInsetAdjustmentBehavior;
	bool _hasAppliedNativeScrollInsetOwnership;
	bool _nativeScrollIsSafeAreaPinned;
	bool _uiKitOwnsSystemInset;
	nfloat _delegatedSystemTopInset;
	nfloat _delegatedTopInset;
	nfloat _delegatedScrollIndicatorTopInset;
	double _delegatedBoundsWidth = -1;
	double _delegatedBoundsHeight = -1;
	nfloat _delegatedStatusBarHeight = -1;
	string _delegatedContentSizeCategory;

	public bool TryDelegate(
		IView scrollHost,
		IView scrollContent,
		Rect bounds,
		double contentTopInset,
		double systemTopInset)
	{
		var nativeScrollView = ResolveNativeScrollView(scrollContent);
		if (nativeScrollView is null)
		{
			Reset();
			return false;
		}

		var coordinatorOwnsNativeScrollView =
			_delegatedNativeScrollView?.TryGetTarget(out var delegatedScrollView) == true &&
			delegatedScrollView == nativeScrollView;
		if (nativeScrollView is not ISafeAreaScrollView &&
			!coordinatorOwnsNativeScrollView &&
			nativeScrollView.ContentInsetAdjustmentBehavior == UIScrollViewContentInsetAdjustmentBehavior.Never)
		{
			Reset();
			return false;
		}

		var isFirstDelegation =
			_delegatedNativeScrollView?.TryGetTarget(out var currentDelegatedScrollView) != true ||
			currentDelegatedScrollView != nativeScrollView;
		if (isFirstDelegation)
		{
			ResetNativeScrollInsetOwnership();
			_delegatedNativeScrollView = new(nativeScrollView);

			if (nativeScrollView is not ISafeAreaScrollView)
				_previousInsetAdjustmentBehavior = nativeScrollView.ContentInsetAdjustmentBehavior;
		}

		var scrollFrame = scrollHost.Frame;
		var delegatedFrame = new Rect(
			scrollFrame.X,
			bounds.Top,
			scrollFrame.Width,
			scrollFrame.Bottom - bounds.Top);

		if (scrollHost.Handler is ISafeAreaScrollViewContainer scrollContainer)
		{
			ResetNativeScrollFrameOwnership(scrollContainer);
			scrollContainer.ApplyDelegatedFrame(scrollFrame, delegatedFrame);
			_delegatedScrollContainer = new(scrollContainer);
		}
		else
		{
			ResetNativeScrollFrameOwnership();
			scrollHost.Handler?.PlatformArrange(delegatedFrame);
		}

		UpdateLayoutEpoch(nativeScrollView, bounds, systemTopInset);

		// Whether the native scroll view is pinned to the safe area or extends to the edge is a
		// plain geometric observation, and nested vendor content only reaches its constraint-driven
		// position after a layout pass. Re-reading it every arrange keeps a late-resolving topology
		// from being locked into the wrong classification for the rest of the layout epoch.
		var hostPlatformView = scrollHost.Handler?.PlatformView as UIView;
		var nativeFrameInHostParent = nativeScrollView.Superview?.ConvertRectToView(
			nativeScrollView.Frame,
			hostPlatformView?.Superview);
		var classificationSystemTopInset = Math.Max(_delegatedSystemTopInset, systemTopInset);
		var nativeScrollIsSafeAreaPinned =
			scrollContent.Handler?.PlatformView != nativeScrollView &&
			nativeFrameInHostParent is CGRect hostParentFrame &&
			hostParentFrame.Top >= delegatedFrame.Top + classificationSystemTopInset - 0.5;

		if (!isFirstDelegation &&
			nativeScrollIsSafeAreaPinned != _nativeScrollIsSafeAreaPinned &&
			(nativeScrollView.Tracking || nativeScrollView.Dragging || nativeScrollView.Decelerating))
		{
			nativeScrollIsSafeAreaPinned = _nativeScrollIsSafeAreaPinned;
		}

		_nativeScrollIsSafeAreaPinned = nativeScrollIsSafeAreaPinned;

		if (_nativeScrollIsSafeAreaPinned)
		{
			// The container already supplies this region, so hand any inset MAUI had taken over
			// back before stepping aside; otherwise a topology that resolves its constraints late
			// keeps a manual inset stacked on top of the pinned position.
			if (_hasAppliedNativeScrollInsetOwnership)
				ReleaseNativeScrollInsetOwnership();

			_uiKitOwnsSystemInset = false;
			ResetContentScrollView();
			return true;
		}

		RegisterContentScrollView(nativeScrollView);

		// Registration only tells UIKit which scroll view drives the large title; it does not make
		// UIKit supply the system inset. Ownership has to be proven by observed safe-area
		// propagation, because the modern CollectionView establishes its controller containment
		// asynchronously: until UIKit reports the inset, the scroll view is already edge-extended,
		// so ceding ownership early leaves the first frames after a push with no top inset at all
		// and the first rows render underneath the navigation bar. Legacy CollectionView paths
		// never receive that propagation and always keep the manual fallback.
		//
		// This is only the requested ownership: TransferNativeScrollInsetOwnership decides when it
		// can be committed, because ownership and the delegated inset have to move together.
		var requestedUIKitOwnsSystemInset =
			nativeScrollView.SafeAreaInsets.Top > 0.5 &&
			nativeScrollView is not MauiCollectionView { UsesUIKitSystemInset: false };

		TransferNativeScrollInsetOwnership(
			nativeScrollView,
			contentTopInset,
			systemTopInset,
			isFirstDelegation,
			requestedUIKitOwnsSystemInset);

		return true;
	}

	public void Reset()
	{
		ResetNativeScrollFrameOwnership();
		ResetNativeScrollInsetOwnership();
		ResetContentScrollView();
	}

	bool RegisterContentScrollView(UIScrollView nativeScrollView)
	{
		var navigationController =
			nativeScrollView.FindResponder<UINavigationController>() ??
			nativeScrollView.Superview?.FindResponder<UINavigationController>();
		var owner = navigationController?.TopViewController;
		if (owner is null ||
			owner.Handle == IntPtr.Zero ||
			owner.View is null ||
			!nativeScrollView.IsDescendantOfView(owner.View))
		{
			ResetContentScrollView();
			return false;
		}

		if (_contentScrollViewOwner?.TryGetTarget(out var currentOwner) == true &&
			currentOwner == owner)
		{
			owner.SetContentScrollView(nativeScrollView, NSDirectionalRectEdge.Top);
			_registeredContentScrollView = new(nativeScrollView);
			return true;
		}

		ResetContentScrollView();
		var previousContentScrollView = owner.GetContentScrollView(NSDirectionalRectEdge.Top);
		if (previousContentScrollView is not null && previousContentScrollView != nativeScrollView)
			_previousContentScrollView = new(previousContentScrollView);

		owner.SetContentScrollView(nativeScrollView, NSDirectionalRectEdge.Top);
		_contentScrollViewOwner = new(owner);
		_registeredContentScrollView = new(nativeScrollView);
		return true;
	}

	void ResetContentScrollView()
	{
		if (_contentScrollViewOwner?.TryGetTarget(out var owner) == true &&
			_registeredContentScrollView?.TryGetTarget(out var registeredScrollView) == true &&
			owner.Handle != IntPtr.Zero &&
			owner.GetContentScrollView(NSDirectionalRectEdge.Top) == registeredScrollView)
		{
			var previousContentScrollView =
				_previousContentScrollView?.TryGetTarget(out var previous) == true
					? previous
					: null;
			owner.SetContentScrollView(previousContentScrollView, NSDirectionalRectEdge.Top);
		}

		_contentScrollViewOwner = null;
		_previousContentScrollView = null;
		_registeredContentScrollView = null;
	}

	void ResetNativeScrollInsetOwnership()
	{
		ReleaseNativeScrollInsetOwnership();
		ClearNativeScrollInsetOwnership();
	}

	void ReleaseNativeScrollInsetOwnership()
	{
		if (!_hasAppliedNativeScrollInsetOwnership)
			return;

		if (_delegatedNativeScrollView?.TryGetTarget(out var delegatedScrollView) != true)
		{
			ClearAppliedNativeScrollInsetOwnership();
			return;
		}

		if (delegatedScrollView.Handle != IntPtr.Zero)
		{
			if (delegatedScrollView is ISafeAreaScrollView safeAreaScrollView)
			{
				safeAreaScrollView.ResetDelegatedTopInset();
				ClearAppliedNativeScrollInsetOwnership();
				return;
			}

			var distanceFromTop = delegatedScrollView.ContentOffset.Y +
				delegatedScrollView.AdjustedContentInset.Top;
			var contentInset = delegatedScrollView.ContentInset;
			var indicatorInsets = delegatedScrollView.VerticalScrollIndicatorInsets;

			delegatedScrollView.ContentInset = new UIEdgeInsets(
				contentInset.Top - _delegatedTopInset,
				contentInset.Left,
				contentInset.Bottom,
				contentInset.Right);
			delegatedScrollView.VerticalScrollIndicatorInsets = new UIEdgeInsets(
				indicatorInsets.Top - _delegatedScrollIndicatorTopInset,
				indicatorInsets.Left,
				indicatorInsets.Bottom,
				indicatorInsets.Right);
			delegatedScrollView.ContentInsetAdjustmentBehavior = _previousInsetAdjustmentBehavior;

			if (!delegatedScrollView.Tracking &&
				!delegatedScrollView.Dragging &&
				!delegatedScrollView.Decelerating)
			{
				delegatedScrollView.ContentOffset = new CGPoint(
					delegatedScrollView.ContentOffset.X,
					distanceFromTop - delegatedScrollView.AdjustedContentInset.Top);
			}
		}

		ClearAppliedNativeScrollInsetOwnership();
	}

	public static IView FindVerticalScrollContent(IView view)
	{
		if (view.Visibility != Visibility.Visible || view.Opacity <= 0.01)
			return null;

		if (view is ISafeAreaView2 { HasExplicitSafeAreaEdges: true })
			return null;

#pragma warning disable CS0618 // ListView and TableView remain supported compatibility controls.
		if (view is ScrollView { Orientation: ScrollOrientation.Vertical or ScrollOrientation.Both } or
			ListView or
			TableView)
#pragma warning restore CS0618
		{
			return view;
		}

		if (view is CollectionView collectionView &&
			collectionView.ItemsLayout is not ItemsLayout { Orientation: ItemsLayoutOrientation.Horizontal })
		{
			return view;
		}

		if (view is CarouselView carouselView &&
			carouselView.ItemsLayout is ItemsLayout { Orientation: ItemsLayoutOrientation.Vertical })
		{
			return view;
		}

		if (view is not Layout &&
			view is not IContentView &&
			view is not WebView &&
			ResolveNativeScrollView(view) is UIScrollView nativeScrollView &&
			IsVerticallyScrollable(nativeScrollView))
		{
			return view;
		}

		if (view is not IVisualTreeElement visualElement)
			return null;

		IView visibleChild = null;
		foreach (var child in visualElement.GetVisualChildren())
		{
			if (child is not IView { Visibility: Visibility.Visible, Opacity: > 0.01 } childView)
				continue;

			if (visibleChild is not null)
				return null;

			visibleChild = childView;
		}

		return visibleChild is null ? null : FindVerticalScrollContent(visibleChild);
	}

	public static bool ContainsNativeScrollView(IView view)
	{
		if (view.Handler?.PlatformView is UIScrollView)
			return true;

		return view.Handler?.PlatformView is UIView platformView &&
			FindNativeScrollView(platformView, verticallyScrollableOnly: false) is not null;
	}

	void TransferNativeScrollInsetOwnership(
		UIScrollView nativeScrollView,
		double topInset,
		double systemTopInset,
		bool isFirstDelegation,
		bool requestedUIKitOwnsSystemInset)
	{
		// Reaching here means the scroll view is edge-extended; the safe-area-pinned topology
		// returns before delegating any inset.
		_delegatedSystemTopInset = (nfloat)Math.Max(_delegatedSystemTopInset, systemTopInset);
		var additionalTopInset = (nfloat)Math.Max(0, topInset - systemTopInset);
		var requestedTopInset = requestedUIKitOwnsSystemInset
			? additionalTopInset
			: _delegatedSystemTopInset + additionalTopInset;

		// Ownership and the delegated inset are one indivisible state: the delegated inset carries
		// the system region only while MAUI owns it, because ceding ownership switches the scroll
		// view to Always and lets UIKit add its own copy of that region on top. Committing the
		// ownership flip while holding the previous inset stacks both copies and doubles the
		// adjusted inset, so the stabilization below takes the requested pair whole or keeps the
		// current pair whole - it never mixes one side of a handoff with the other.
		var uiKitSystemTopInset = nativeScrollView.SafeAreaInsets.Top;
		var requestedEffectiveTopInset =
			requestedTopInset + (requestedUIKitOwnsSystemInset ? uiKitSystemTopInset : 0);
		var currentEffectiveTopInset =
			_delegatedTopInset + (_uiKitOwnsSystemInset ? uiKitSystemTopInset : 0);

		// Shrinking the inset mid-gesture pulls the content out from under the finger, so an active
		// interaction defers the whole pair to the next arrange. A handoff that merely changes which
		// side supplies the system region leaves the effective inset intact and stays safe to apply
		// immediately, which is what keeps a late safe-area propagation from stalling until the
		// gesture ends.
		if (!isFirstDelegation &&
			requestedEffectiveTopInset < currentEffectiveTopInset - 0.5 &&
			(nativeScrollView.Tracking || nativeScrollView.Dragging || nativeScrollView.Decelerating))
		{
			requestedUIKitOwnsSystemInset = _uiKitOwnsSystemInset;
			requestedTopInset = _delegatedTopInset;
		}

		var delegatedInsetChanged =
			isFirstDelegation ||
			requestedUIKitOwnsSystemInset != _uiKitOwnsSystemInset ||
			Math.Abs(_delegatedTopInset - requestedTopInset) > 0.5;

		_hasAppliedNativeScrollInsetOwnership = true;
		_uiKitOwnsSystemInset = requestedUIKitOwnsSystemInset;

		if (nativeScrollView is ISafeAreaScrollView safeAreaScrollView)
		{
			_delegatedTopInset = requestedTopInset;
			_delegatedScrollIndicatorTopInset = requestedTopInset;
			safeAreaScrollView.ApplyDelegatedTopInset(requestedTopInset, _uiKitOwnsSystemInset);
			return;
		}

		var distanceFromTop = nativeScrollView.ContentOffset.Y + nativeScrollView.AdjustedContentInset.Top;
		var contentInset = nativeScrollView.ContentInset;
		var indicatorInsets = nativeScrollView.VerticalScrollIndicatorInsets;
		var baseContentTop = contentInset.Top - _delegatedTopInset;
		var baseIndicatorTop = indicatorInsets.Top - _delegatedScrollIndicatorTopInset;

		_delegatedTopInset = requestedTopInset;
		_delegatedScrollIndicatorTopInset = requestedTopInset;
		nativeScrollView.ContentInsetAdjustmentBehavior = _uiKitOwnsSystemInset
			? UIScrollViewContentInsetAdjustmentBehavior.Always
			: UIScrollViewContentInsetAdjustmentBehavior.Never;
		nativeScrollView.ContentInset = new UIEdgeInsets(
			baseContentTop + _delegatedTopInset,
			contentInset.Left,
			contentInset.Bottom,
			contentInset.Right);
		nativeScrollView.VerticalScrollIndicatorInsets = new UIEdgeInsets(
			baseIndicatorTop + _delegatedScrollIndicatorTopInset,
			indicatorInsets.Left,
			indicatorInsets.Bottom,
			indicatorInsets.Right);

		if (delegatedInsetChanged &&
			!nativeScrollView.Tracking &&
			!nativeScrollView.Dragging &&
			!nativeScrollView.Decelerating)
		{
			nativeScrollView.ContentOffset = new CGPoint(
				nativeScrollView.ContentOffset.X,
				isFirstDelegation && nativeScrollView.ContentOffset.Y <= 0
					? -nativeScrollView.AdjustedContentInset.Top
					: distanceFromTop - nativeScrollView.AdjustedContentInset.Top);
		}
	}

	void UpdateLayoutEpoch(UIScrollView nativeScrollView, Rect bounds, double systemTopInset)
	{
		var statusBarHeight =
			nativeScrollView.Window?.WindowScene?.StatusBarManager?.StatusBarFrame.Height ?? -1;
		var contentSizeCategory =
			nativeScrollView.TraitCollection.PreferredContentSizeCategory;
		var layoutEnvironmentChanged =
			(_delegatedBoundsWidth >= 0 &&
				(Math.Abs(_delegatedBoundsWidth - bounds.Width) > 1 ||
				Math.Abs(_delegatedBoundsHeight - bounds.Height) > 1)) ||
			(_delegatedStatusBarHeight >= 0 && statusBarHeight >= 0 &&
				Math.Abs(_delegatedStatusBarHeight - statusBarHeight) > 0.5) ||
			(_delegatedContentSizeCategory is not null &&
				_delegatedContentSizeCategory != contentSizeCategory);

		if (layoutEnvironmentChanged)
			_delegatedSystemTopInset = (nfloat)systemTopInset;

		_delegatedBoundsWidth = bounds.Width;
		_delegatedBoundsHeight = bounds.Height;
		if (statusBarHeight >= 0)
			_delegatedStatusBarHeight = statusBarHeight;
		_delegatedContentSizeCategory = contentSizeCategory;
	}

	void ResetNativeScrollFrameOwnership(ISafeAreaScrollViewContainer currentContainer = null)
	{
		if (_delegatedScrollContainer?.TryGetTarget(out var delegatedContainer) == true &&
			delegatedContainer != currentContainer)
		{
			delegatedContainer.ResetDelegatedFrame();
			_delegatedScrollContainer = null;
		}
	}

	void ClearNativeScrollInsetOwnership()
	{
		_delegatedNativeScrollView = null;
		_nativeScrollIsSafeAreaPinned = false;
		ClearAppliedNativeScrollInsetOwnership();
		_delegatedSystemTopInset = 0;
		_delegatedBoundsWidth = -1;
		_delegatedBoundsHeight = -1;
		_delegatedStatusBarHeight = -1;
		_delegatedContentSizeCategory = null;
	}

	void ClearAppliedNativeScrollInsetOwnership()
	{
		_hasAppliedNativeScrollInsetOwnership = false;
		_uiKitOwnsSystemInset = false;
		_delegatedTopInset = 0;
		_delegatedScrollIndicatorTopInset = 0;
	}

	static UIScrollView ResolveNativeScrollView(IView view)
	{
		if (view.Handler?.PlatformView is UIScrollView scrollView)
			return scrollView;

		if (view.Handler?.PlatformView is UIView platformView)
		{
			return FindNativeScrollView(platformView, verticallyScrollableOnly: true) ??
				(view is WebView ? FindNativeScrollView(platformView, verticallyScrollableOnly: false) : null);
		}

		return null;
	}

	static UIScrollView FindNativeScrollView(UIView view, bool verticallyScrollableOnly)
	{
		foreach (var child in view.Subviews)
		{
			if (child is UIScrollView scrollView &&
				(!verticallyScrollableOnly || IsVerticallyScrollable(scrollView)))
			{
				return scrollView;
			}

			if (FindNativeScrollView(child, verticallyScrollableOnly) is UIScrollView nestedScrollView)
				return nestedScrollView;
		}

		return null;
	}

	static bool IsVerticallyScrollable(UIScrollView scrollView)
	{
		if (scrollView is UITableView)
			return true;

		if (scrollView is UICollectionView { CollectionViewLayout: UICollectionViewFlowLayout flowLayout })
			return flowLayout.ScrollDirection == UICollectionViewScrollDirection.Vertical;

		return scrollView.AlwaysBounceVertical ||
			(!scrollView.AlwaysBounceHorizontal && scrollView.ContentSize.Height > scrollView.Bounds.Height);
	}
}
