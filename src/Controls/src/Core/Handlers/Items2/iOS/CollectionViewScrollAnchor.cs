#nullable enable
using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	/// <summary>
	/// Keeps the end of a <see cref="StructuredItemsView"/>'s <see cref="UICollectionView"/> in view for
	/// <see cref="ScrollAnchorMode.End"/>, grouped or not, with or without a header and footer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// It looks at the collection view after each of its layout passes, once the content size and any offset
	/// adjustments UIKit made while laying out (self-sizing rows above the viewport, clamping after items are removed)
	/// have settled. UIKit lays the collection view out as it scrolls and whenever its content or size changes. A scroll
	/// that happens outside a layout pass, and the end of a drag or a deceleration, ask for a layout pass, so the anchor
	/// looks after those too.
	/// </para>
	/// <para>
	/// The scroll view delegate reports every offset change. One made during a layout pass is UIKit laying out, even when
	/// the content ends up the size it was (UIKit can re-estimate rows it measured before and measure them again in one
	/// pass), so it never counts as a scroll away from the end. The user is scrolling while the collection view is
	/// dragging or decelerating.
	/// </para>
	/// <para>
	/// It holds no references: the controller passes its views in. It is vertical only.
	/// </para>
	/// </remarks>
	sealed class CollectionViewScrollAnchor
	{
		readonly ScrollAnchorTracker _tracker = new();
		int _layoutDepth;
		bool _offsetChangedDuringLayout;
		bool _requestingScroll;
		bool _applying;
		bool _wasVertical = true;

		/// <summary>Starts following the end again, as for a new set of items.</summary>
		public void Reset(UICollectionView collectionView)
		{
			_tracker.Reset();
			_offsetChangedDuringLayout = false;
			collectionView.SetNeedsLayout();
		}

		/// <summary>Called when the controller's view is about to lay out its subviews.</summary>
		public void LayoutStarting() => _layoutDepth++;

		/// <summary>Called when the controller's view has laid out its subviews: looks at where the view is.</summary>
		public void LayoutFinished(UICollectionView collectionView, StructuredItemsView? itemsView, bool isVertical)
		{
			if (_layoutDepth > 0)
			{
				_layoutDepth--;
			}

			// UIKit can lay the view out from inside a scroll the app requested: judge the request once it has landed.
			if (_layoutDepth > 0 || _requestingScroll || itemsView is null)
			{
				return;
			}

			if (isVertical != _wasVertical)
			{
				// A new layout that scrolls vertically again follows the end from the start, as a new view does.
				_wasVertical = isVertical;
				_tracker.Reset();
			}

			// MAUI measures the view before it's in a window by laying it out in a temporary frame: that isn't where it is.
			if (!isVertical || collectionView.Window is null)
			{
				Publish(collectionView, itemsView, isVertical && _tracker.IsAnchored);
				return;
			}

			var inset = collectionView.AdjustedContentInset;
			var metrics = new ScrollAnchorMetrics(
				collectionView.ContentOffset.Y + inset.Top,
				collectionView.ContentSize.Height + inset.Top + inset.Bottom,
				collectionView.Bounds.Height);

			var sizeChanged = _offsetChangedDuringLayout;
			_offsetChangedDuringLayout = false;

			if (_tracker.Update(metrics, collectionView.Dragging || collectionView.Decelerating, sizeChanged) is double end)
			{
				_applying = true;
				try
				{
					collectionView.SetContentOffset(new CGPoint(collectionView.ContentOffset.X, end - inset.Top), false);
				}
				finally
				{
					_applying = false;
				}
			}

			Publish(collectionView, itemsView, _tracker.IsAnchored);
		}

		/// <summary>Called by the scroll view delegate whenever the content offset changes.</summary>
		public void Scrolled(UICollectionView collectionView)
		{
			if (_applying)
			{
				return;
			}

			if (_layoutDepth > 0)
			{
				_offsetChangedDuringLayout = true;
				return;
			}

			// Look again once the layout this scroll causes is done.
			collectionView.SetNeedsLayout();
		}

		/// <summary>Called when a drag ends without decelerating, or a deceleration ends.</summary>
		public void UserScrollEnded(UICollectionView collectionView) => collectionView.SetNeedsLayout();

		/// <summary>Called before the handler scrolls for a <c>ScrollTo</c> request.</summary>
		public void ScrollRequesting() => _requestingScroll = true;

		/// <summary>
		/// Called after the handler scrolled for a <c>ScrollTo</c> request (animating, or not): the next look judges where
		/// it went, instead of scrolling back to the end.
		/// </summary>
		public void ScrollRequested(UICollectionView collectionView)
		{
			_requestingScroll = false;
			_tracker.RequestScroll();
			collectionView.SetNeedsLayout();
		}

		/// <summary>Scrolls to the end, and follows it again.</summary>
		public void ScrollToEnd(UICollectionView collectionView, StructuredItemsView? itemsView, bool animated)
		{
			if (animated && collectionView.Window is not null)
			{
				_tracker.Engage();
				if (itemsView is not null)
				{
					Publish(collectionView, itemsView, true);
				}

				collectionView.LayoutIfNeeded();
				SetOffsetToEnd(collectionView, animated: true);
				return;
			}

			// Stop any deceleration where it is, then look again as for a new view: that scrolls to the exact end once the
			// rows there are measured, and before the view first appears when it isn't in a window yet.
			SetOffsetToEnd(collectionView, animated: false);
			_tracker.Reset();
			_offsetChangedDuringLayout = false;
			collectionView.SetNeedsLayout();
			collectionView.LayoutIfNeeded();
		}

		/// <summary>Scrolls a collection view that doesn't anchor its end to the end, once.</summary>
		/// <returns>
		/// Where an animated scroll stops, or <see langword="null"/> when it didn't animate. The rows the animation scrolls
		/// past are measured on the way, which can move the end beyond that point: <see cref="ScrollToEndOnceEnded"/> goes
		/// the rest of the way.
		/// </returns>
		public static double? ScrollToEndOnce(UICollectionView collectionView, bool animated)
		{
			collectionView.LayoutIfNeeded();
			var target = SetOffsetToEnd(collectionView, animated);
			if (animated)
			{
				return target;
			}

			// The rows at the end are measured once they're in view, which can move the end: go the rest of the way.
			collectionView.LayoutIfNeeded();
			SetOffsetToEnd(collectionView, animated: false);
			return null;
		}

		/// <summary>
		/// Called when the animated scroll <see cref="ScrollToEndOnce"/> started has stopped: goes the rest of the way to
		/// the end, unless the view was scrolled away from where the animation stopped.
		/// </summary>
		public static void ScrollToEndOnceEnded(UICollectionView collectionView, double target)
		{
			if (Math.Abs(collectionView.ContentOffset.Y - target) <= 0.5)
			{
				ScrollToEndOnce(collectionView, animated: false);
			}
		}

		// Returns the offset it scrolls to, or null when the view is at the end already.
		static double? SetOffsetToEnd(UICollectionView collectionView, bool animated)
		{
			var inset = collectionView.AdjustedContentInset;
			var end = Math.Max(-inset.Top, collectionView.ContentSize.Height + inset.Bottom - collectionView.Bounds.Height);
			if (Math.Abs(collectionView.ContentOffset.Y - end) <= 0.5)
			{
				return null;
			}

			collectionView.SetContentOffset(new CGPoint(collectionView.ContentOffset.X, end), animated);
			return end;
		}

		/// <summary>Stops anchoring: the view no longer follows its end.</summary>
		public void Detach(UICollectionView collectionView, StructuredItemsView? itemsView)
		{
			if (itemsView is not null)
			{
				Publish(collectionView, itemsView, false);
			}
			else if (collectionView is Items.MauiCollectionView mauiCollectionView)
			{
				mauiCollectionView.IsFollowingEnd = false;
			}
		}

		void Publish(UICollectionView collectionView, StructuredItemsView itemsView, bool isAnchored)
		{
			if (collectionView is Items.MauiCollectionView mauiCollectionView)
			{
				mauiCollectionView.IsFollowingEnd = isAnchored;
			}

			if (itemsView.IsAnchored != isAnchored)
			{
				itemsView.SetIsAnchored(isAnchored);
			}
		}
	}
}
