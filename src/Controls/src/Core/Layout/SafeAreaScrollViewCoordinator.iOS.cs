#nullable disable
using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls;

sealed class SafeAreaScrollViewCoordinator
{
	WeakReference<UIScrollView> _delegatedNativeScrollView;
	WeakReference<ISafeAreaScrollViewContainer> _delegatedScrollContainer;
	UIScrollViewContentInsetAdjustmentBehavior _previousInsetAdjustmentBehavior;
	nfloat _delegatedTopInset;
	nfloat _delegatedScrollIndicatorTopInset;

	public bool TryDelegate(IView scrollHost, IView scrollContent, Rect bounds, double contentTopInset)
	{
		var nativeScrollView = ResolveNativeScrollView(scrollContent);
		if (nativeScrollView is null)
		{
			Reset();
			return false;
		}

		var scrollFrame = scrollHost.Frame;
		var delegatedFrame = new Rect(
			scrollFrame.X,
			bounds.Top,
			scrollFrame.Width,
			scrollFrame.Bottom - bounds.Top);

		TransferNativeScrollInsetOwnership(nativeScrollView, contentTopInset);

		if (scrollHost.Handler is ISafeAreaScrollViewContainer scrollContainer &&
			scrollHost.Handler.PlatformView == nativeScrollView &&
			(scrollContainer is not UIView containerView || containerView != nativeScrollView))
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

		return true;
	}

	public void Reset()
	{
		ResetNativeScrollFrameOwnership();

		if (_delegatedNativeScrollView?.TryGetTarget(out var delegatedScrollView) != true)
		{
			ClearNativeScrollInsetOwnership();
			return;
		}

		if (delegatedScrollView.Handle != IntPtr.Zero)
		{
			if (delegatedScrollView is ISafeAreaScrollView safeAreaScrollView)
			{
				safeAreaScrollView.ResetDelegatedTopInset();
				ClearNativeScrollInsetOwnership();
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

			if (!delegatedScrollView.Dragging && !delegatedScrollView.Decelerating)
			{
				delegatedScrollView.ContentOffset = new CGPoint(
					delegatedScrollView.ContentOffset.X,
					distanceFromTop - delegatedScrollView.AdjustedContentInset.Top);
			}
		}

		ClearNativeScrollInsetOwnership();
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

	void TransferNativeScrollInsetOwnership(UIScrollView nativeScrollView, double topInset)
	{
		var isFirstDelegation =
			_delegatedNativeScrollView?.TryGetTarget(out var delegatedScrollView) != true ||
			delegatedScrollView != nativeScrollView;

		if (isFirstDelegation)
		{
			Reset();
			_delegatedNativeScrollView = new(nativeScrollView);

			if (nativeScrollView is not ISafeAreaScrollView)
				_previousInsetAdjustmentBehavior = nativeScrollView.ContentInsetAdjustmentBehavior;
		}

		if (nativeScrollView is ISafeAreaScrollView safeAreaScrollView)
		{
			safeAreaScrollView.ApplyDelegatedTopInset(topInset);
			return;
		}

		var distanceFromTop = nativeScrollView.ContentOffset.Y + nativeScrollView.AdjustedContentInset.Top;
		var contentInset = nativeScrollView.ContentInset;
		var indicatorInsets = nativeScrollView.VerticalScrollIndicatorInsets;
		var baseContentTop = contentInset.Top - _delegatedTopInset;
		var baseIndicatorTop = indicatorInsets.Top - _delegatedScrollIndicatorTopInset;

		_delegatedTopInset = (nfloat)topInset;
		_delegatedScrollIndicatorTopInset = (nfloat)topInset;
		nativeScrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
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

		if (!nativeScrollView.Dragging && !nativeScrollView.Decelerating)
		{
			nativeScrollView.ContentOffset = new CGPoint(
				nativeScrollView.ContentOffset.X,
				isFirstDelegation && nativeScrollView.ContentOffset.Y <= 0
					? -nativeScrollView.AdjustedContentInset.Top
					: distanceFromTop - nativeScrollView.AdjustedContentInset.Top);
		}
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
