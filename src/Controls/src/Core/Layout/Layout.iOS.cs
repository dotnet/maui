#nullable disable
using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class Layout : ISafeAreaLayout
	{
		WeakReference<UIScrollView> _delegatedNativeScrollView;
		UIScrollViewContentInsetAdjustmentBehavior _previousInsetAdjustmentBehavior;
		nfloat _delegatedTopInset;
		nfloat _delegatedScrollIndicatorTopInset;

		Size ISafeAreaLayout.CrossPlatformArrange(Rect bounds, Thickness safeArea, bool delegateTopInset)
		{
			var safeBounds = new Rect(
				bounds.X + safeArea.Left,
				bounds.Y + safeArea.Top,
				bounds.Width - safeArea.HorizontalThickness,
				bounds.Height - safeArea.VerticalThickness);

			var arranged = CrossPlatformArrange(safeBounds);

			if (!delegateTopInset || safeArea.Top <= 0)
			{
				ResetNativeScrollInsetOwnership();
				return arranged;
			}

			const double tolerance = 1;
			IView scrollHost = null;
			IView scrollContent = null;
			double contentTopInset = 0;
			var candidateCount = 0;

			foreach (var child in this)
			{
				if (child.Visibility != Visibility.Visible ||
					child.Opacity <= 0.01 ||
					FindVerticalScrollContent(child) is not IView foundScrollContent)
					continue;

				var frame = child.Frame;
				var occupiesMostOfSafeHeight = frame.Height >= safeBounds.Height / 2;
				var beginsWithinSafeBounds = frame.Top >= safeBounds.Top - tolerance;
				var fixedTopSiblingsAreOverlays = FixedTopSiblingsAreOverlays(child, frame, safeBounds, tolerance);

				if (beginsWithinSafeBounds && occupiesMostOfSafeHeight && fixedTopSiblingsAreOverlays)
				{
					candidateCount++;
					scrollHost = child;
					scrollContent = foundScrollContent;
					contentTopInset = frame.Top - bounds.Top;

					if (candidateCount > 1)
						break;
				}
			}

			if (candidateCount != 1)
			{
				ResetNativeScrollInsetOwnership();
				return arranged;
			}

			var nativeScrollView = ResolveNativeScrollView(scrollContent);
			if (nativeScrollView is null)
			{
				ResetNativeScrollInsetOwnership();
				return arranged;
			}

			var scrollFrame = scrollHost.Frame;
			scrollHost.Handler?.PlatformArrange(new Rect(
				scrollFrame.X,
				bounds.Top,
				scrollFrame.Width,
				scrollFrame.Bottom - bounds.Top));

			TransferNativeScrollInsetOwnership(nativeScrollView, contentTopInset);
			return arranged;
		}

		void ISafeAreaLayout.DisconnectSafeArea() => ResetNativeScrollInsetOwnership();

		void TransferNativeScrollInsetOwnership(UIScrollView nativeScrollView, double topInset)
		{
			if (_delegatedNativeScrollView?.TryGetTarget(out var delegatedScrollView) != true ||
				delegatedScrollView != nativeScrollView)
			{
				ResetNativeScrollInsetOwnership();
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
					distanceFromTop - nativeScrollView.AdjustedContentInset.Top);
			}
		}

		void ResetNativeScrollInsetOwnership()
		{
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

		bool FixedTopSiblingsAreOverlays(IView scrollHost, Rect scrollFrame, Rect safeBounds, double tolerance)
		{
			foreach (var sibling in this)
			{
				if (ReferenceEquals(sibling, scrollHost) ||
					sibling.Visibility != Visibility.Visible ||
					sibling.Opacity <= 0.01)
					continue;

				var siblingFrame = sibling.Frame;
				var isFixedAboveScrollHost =
					siblingFrame.Top >= safeBounds.Top - tolerance &&
					siblingFrame.Bottom <= scrollFrame.Top + tolerance;

				if (isFixedAboveScrollHost && sibling.ZIndex <= scrollHost.ZIndex)
					return false;
			}

			return true;
		}

		static IView FindVerticalScrollContent(IView view)
		{
			if (view.Visibility != Visibility.Visible || view.Opacity <= 0.01)
				return null;

			if (view is ISafeAreaView2 { HasExplicitSafeAreaEdges: true })
				return null;

#pragma warning disable CS0618 // ListView and TableView remain supported compatibility controls.
			if (view is ScrollView { Orientation: ScrollOrientation.Vertical or ScrollOrientation.Both } or
				ListView or
				TableView or
				WebView)
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

			if (view is not Microsoft.Maui.Controls.Layout &&
				view is not IContentView &&
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

		static bool IsVerticallyScrollable(UIScrollView scrollView)
		{
			if (scrollView is UITableView)
				return true;

			if (scrollView is UICollectionView { CollectionViewLayout: UICollectionViewFlowLayout flowLayout })
				return flowLayout.ScrollDirection == UICollectionViewScrollDirection.Vertical;

			return scrollView.AlwaysBounceVertical ||
				(!scrollView.AlwaysBounceHorizontal && scrollView.ContentSize.Height > scrollView.Bounds.Height);
		}

		/// <summary>
		/// Maps the abstract InputTransparent property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="Layout"/> instance.</param>
		[Obsolete]
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) { }

		/// <summary>
		/// Maps the abstract InputTransparent property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="Layout"/> instance.</param>
		[Obsolete]
		public static void MapInputTransparent(ILayoutHandler handler, Layout layout) { }
	}
}
