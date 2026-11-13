#nullable enable
using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	/// <summary>
	/// Keeps an edge of a <see cref="StructuredItemsView"/>'s <see cref="UICollectionView"/> in view for
	/// <see cref="ScrollAnchorMode.Start"/> and <see cref="ScrollAnchorMode.End"/>, grouped or not, with or without a
	/// header and footer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// It looks at the collection view after each of its layout passes, once the content size and any offset
	/// adjustments UIKit made while laying out (self-sizing rows above the viewport, clamping after items are removed)
	/// have settled. UIKit lays the collection view out as it scrolls and whenever its content or size changes. A scroll
	/// that happens outside a layout pass, and the end of a drag or a deceleration, ask for a layout pass, so the anchor
	/// looks after those too. It also looks right away at a scroll the user isn't making (an animation, the keyboard,
	/// tapping the status bar to scroll to the top): the layout pass that follows measures the rows the scroll brought
	/// into view, and the scroll would then look like UIKit laying out.
	/// </para>
	/// <para>
	/// The scroll view delegate reports every offset change. One made during a layout pass is UIKit laying out, even when
	/// the content ends up the size it was (UIKit can re-estimate rows it measured before and measure them again in one
	/// pass), so it never counts as a scroll away from an edge. The user is scrolling while the collection view is
	/// dragging or decelerating.
	/// </para>
	/// <para>
	/// It holds no references: the controller passes its views in. It is vertical only.
	/// </para>
	/// </remarks>
	sealed class CollectionViewScrollAnchor
	{
		readonly ScrollAnchorTracker _tracker;
		int _layoutDepth;
		bool _offsetChangedDuringLayout;
		bool _requestingScroll;
		bool _applying;
		bool _wasVertical = true;
		bool _detached;

		/// <summary>Initializes a new anchor that follows the edges <paramref name="mode"/> selects.</summary>
		public CollectionViewScrollAnchor(ScrollAnchorMode mode)
		{
			_tracker = new ScrollAnchorTracker(mode);
		}

		/// <summary>Gets the edges the view can follow.</summary>
		public ScrollAnchorMode Edges => _tracker.Edges;

		/// <summary>Gets whether the view can follow <paramref name="edge"/>.</summary>
		public bool CanFollow(ScrollAnchorMode edge) => _tracker.CanFollow(edge);

		/// <summary>Follows the edges <paramref name="mode"/> selects instead, starting over as for a new view.</summary>
		public void UpdateMode(UICollectionView collectionView, ScrollAnchorMode mode)
		{
			_tracker.SetEdges(mode);
			_offsetChangedDuringLayout = false;
			collectionView.SetNeedsLayout();
		}

		/// <summary>Starts following the edge a new view shows again, as for a new set of items.</summary>
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

			Look(collectionView, itemsView, isVertical, afterLayout: true);
		}

		/// <summary>Called by the scroll view delegate whenever the content offset changes.</summary>
		public void Scrolled(UICollectionView collectionView, StructuredItemsView? itemsView, bool isVertical)
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

			// A scroll the user isn't making: look before the layout it causes measures the rows it brought into view. A view
			// that just started over looks once it's laid out, as its sizes may not be current yet.
			if (!_requestingScroll && _tracker.HasPosition && itemsView is not null && !collectionView.Dragging && !collectionView.Decelerating)
			{
				Look(collectionView, itemsView, isVertical, afterLayout: false);
			}

			// Look again once the layout this scroll causes is done.
			collectionView.SetNeedsLayout();
		}

		void Look(UICollectionView collectionView, StructuredItemsView itemsView, bool isVertical, bool afterLayout)
		{
			if (isVertical != _wasVertical)
			{
				// A new layout that scrolls vertically again starts over, as a new view does.
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

			var sizeChanged = false;
			if (afterLayout)
			{
				sizeChanged = _offsetChangedDuringLayout;
				_offsetChangedDuringLayout = false;
			}

			if (_tracker.Update(metrics, collectionView.Dragging || collectionView.Decelerating, sizeChanged) is double edge)
			{
				ApplyOffset(collectionView, edge - inset.Top);
			}

			Publish(collectionView, itemsView, _tracker.IsAnchored);
		}

		// Scrolls without the scroll counting as one: the anchor knows where it put the view. It takes the offset, not a
		// callback, as the anchor looks after every layout pass: a callback that captures would allocate on each.
		void ApplyOffset(UICollectionView collectionView, double offsetY)
		{
			_applying = true;
			try
			{
				collectionView.SetContentOffset(new CGPoint(collectionView.ContentOffset.X, offsetY), false);
			}
			finally
			{
				_applying = false;
			}
		}

		/// <summary>Called when a drag ends without decelerating, or a deceleration ends.</summary>
		public void UserScrollEnded(UICollectionView collectionView) => collectionView.SetNeedsLayout();

		/// <summary>Called before the handler scrolls for a <c>ScrollTo</c> request.</summary>
		public void ScrollRequesting() => _requestingScroll = true;

		/// <summary>
		/// Called after the handler scrolled for a <c>ScrollTo</c> request (animating, or not): the next look judges where
		/// it went, instead of scrolling back to the edge the view followed.
		/// </summary>
		public void ScrollRequested(UICollectionView collectionView)
		{
			_requestingScroll = false;
			_tracker.RequestScroll();
			collectionView.SetNeedsLayout();
		}

		/// <summary>Scrolls to the start, and follows it again. The view must be able to follow its start.</summary>
		public void ScrollToStart(UICollectionView collectionView, StructuredItemsView? itemsView, bool animated)
		{
			if (animated && collectionView.Window is not null)
			{
				_tracker.Engage(ScrollAnchorMode.Start);
				if (itemsView is not null)
				{
					Publish(collectionView, itemsView, true);
				}

				collectionView.LayoutIfNeeded();
				SetOffsetToStart(collectionView, animated: true);
				return;
			}

			// Jump there, which stops any deceleration, then look again as for a new view that follows its start.
			ApplyOffset(collectionView, GetStartOffset(collectionView));
			_tracker.Reset(ScrollAnchorMode.Start);
			_offsetChangedDuringLayout = false;
			collectionView.SetNeedsLayout();
			collectionView.LayoutIfNeeded();
		}

		/// <summary>Scrolls to the end, and follows it again. The view must be able to follow its end.</summary>
		public void ScrollToEnd(UICollectionView collectionView, StructuredItemsView? itemsView, bool animated)
		{
			if (animated && collectionView.Window is not null)
			{
				_tracker.Engage(ScrollAnchorMode.End);
				if (itemsView is not null)
				{
					Publish(collectionView, itemsView, true);
				}

				collectionView.LayoutIfNeeded();
				SetOffsetToEnd(collectionView, animated: true);
				return;
			}

			// Jump to the end as laid out now, which stops any deceleration, then look again as for a new view: that scrolls to
			// the exact end once the rows there are measured, and before the view first appears when it isn't in a window yet.
			ApplyOffset(collectionView, GetEndOffset(collectionView));
			_tracker.Reset(ScrollAnchorMode.End);
			_offsetChangedDuringLayout = false;
			collectionView.SetNeedsLayout();
			collectionView.LayoutIfNeeded();
		}

		/// <summary>Scrolls a collection view to the start, once.</summary>
		/// <returns>
		/// Whether it started an animated scroll. UIKit keeps the rows in view where they are as it measures the rows the
		/// animation brings into view above them, which moves where the animation stops: <see cref="ScrollToStartOnceEnded"/>
		/// goes the rest of the way.
		/// </returns>
		public static bool ScrollToStartOnce(UICollectionView collectionView, bool animated)
		{
			collectionView.LayoutIfNeeded();
			return SetOffsetToStart(collectionView, animated) && animated;
		}

		/// <summary>Called when the animated scroll <see cref="ScrollToStartOnce"/> started has stopped: goes the rest of the way.</summary>
		public static void ScrollToStartOnceEnded(UICollectionView collectionView) => SetOffsetToStart(collectionView, animated: false);

		/// <summary>Scrolls a collection view to the end, once.</summary>
		/// <returns>
		/// Where an animated scroll stops, or <see langword="null"/> when it didn't animate. The rows the animation scrolls
		/// past are measured on the way, which can move the end beyond that point: the controller goes the rest of the way
		/// once the animation ends.
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

		// Returns whether it scrolls: not when the view is at the start already.
		static bool SetOffsetToStart(UICollectionView collectionView, bool animated)
		{
			var start = GetStartOffset(collectionView);
			if (Math.Abs(collectionView.ContentOffset.Y - start) <= 0.5)
			{
				return false;
			}

			collectionView.SetContentOffset(new CGPoint(collectionView.ContentOffset.X, start), animated);
			return true;
		}

		/// <summary>Scrolls to the end of the content as laid out now.</summary>
		/// <returns>The offset it scrolls to, or <see langword="null"/> when the view is at the end already.</returns>
		internal static double? SetOffsetToEnd(UICollectionView collectionView, bool animated)
		{
			var end = GetEndOffset(collectionView);
			if (Math.Abs(collectionView.ContentOffset.Y - end) <= 0.5)
			{
				return null;
			}

			collectionView.SetContentOffset(new CGPoint(collectionView.ContentOffset.X, end), animated);
			return end;
		}

		// The offset that shows the start of the content, above the header.
		static double GetStartOffset(UICollectionView collectionView) => -collectionView.AdjustedContentInset.Top;

		// The offset that shows the end of the content as laid out now, past the footer.
		static double GetEndOffset(UICollectionView collectionView)
		{
			var inset = collectionView.AdjustedContentInset;
			return Math.Max(-inset.Top, collectionView.ContentSize.Height + inset.Bottom - collectionView.Bounds.Height);
		}

		/// <summary>Stops anchoring: the view no longer follows an edge.</summary>
		public void Detach(UICollectionView collectionView, StructuredItemsView? itemsView)
		{
			_detached = true;
			if (itemsView is not null)
			{
				Publish(collectionView, itemsView, false);
			}
			else if (collectionView is Items.MauiCollectionView mauiCollectionView)
			{
				mauiCollectionView.IsFollowingEdge = false;
			}
		}

		void Publish(UICollectionView collectionView, StructuredItemsView itemsView, bool isAnchored)
		{
			// The app can turn anchoring off from an event a scroll of this anchor raises, such as Scrolled: once detached, it
			// no longer follows an edge, whatever the look it was in the middle of found.
			isAnchored &= !_detached;

			if (collectionView is Items.MauiCollectionView mauiCollectionView)
			{
				mauiCollectionView.IsFollowingEdge = isAnchored;
			}

			if (itemsView.IsAnchored != isAnchored)
			{
				itemsView.SetIsAnchored(isAnchored);
			}
		}
	}
}
