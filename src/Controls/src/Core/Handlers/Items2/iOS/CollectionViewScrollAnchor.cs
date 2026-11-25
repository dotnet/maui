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
	/// have settled. UIKit lays the collection view out as it scrolls and whenever its content or size changes, also
	/// outside a layout pass when the controller has it update the view for a new layout or for items added, removed or
	/// moved: the controller tells the anchor about those as it does about its layout passes. A scroll that happens
	/// outside a layout pass, and the end of a drag or a deceleration, ask for a layout pass, so the anchor looks after
	/// those too. It also looks right away at a scroll the user isn't making (an animation, the keyboard, tapping the
	/// status bar to scroll to the top): the layout pass that follows measures the rows the scroll brought into view, and
	/// the scroll would then look like UIKit laying out. That look judges the scroll as a new one, also right after a drag,
	/// a deceleration or an animated scroll ended, unless the content or the viewport changed size since the last look.
	/// </para>
	/// <para>
	/// The scroll view delegate reports every offset change. One made while UIKit lays the view out is UIKit's, even when
	/// the content ends up the size it was (UIKit can re-estimate rows it measured before and measure them again in one
	/// pass, and can move the offset before it updates the content size), so it never counts as a scroll away from an
	/// edge. The user is scrolling while the collection view is dragging or decelerating. An animated scroll the app
	/// requested, to an item or to an edge the view doesn't follow, scrolls as the user's does, as on Android, until it ends:
	/// it's judged by where it stops, and the content changing on its way doesn't scroll the view back to the edge it left.
	/// An animated scroll to the edge the view follows keeps following that edge on its way there, unless a scroll stops it
	/// short: the view is then judged by where it stopped.
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

		// Whether an animated scroll the app requested is on its way (see ScrollRequested).
		bool _animatingRequest;

		// The content offset the scroll view delegate last reported: UIKit can move it without reporting that (see
		// OffsetObserved).
		double _reportedOffsetY = double.NaN;

		// The animated scroll to an edge the anchor last started (see EdgeAnimation).
		(ScrollAnchorMode Edge, double Target)? _edgeAnimation;

		/// <summary>Initializes a new anchor that follows the edges <paramref name="mode"/> selects.</summary>
		public CollectionViewScrollAnchor(ScrollAnchorMode mode)
		{
			_tracker = new ScrollAnchorTracker(mode);
		}

		/// <summary>Gets the edges the view can follow.</summary>
		public ScrollAnchorMode Edges => _tracker.Edges;

		/// <summary>Gets whether the view can follow <paramref name="edge"/>.</summary>
		public bool CanFollow(ScrollAnchorMode edge) => _tracker.CanFollow(edge);

		/// <summary>
		/// Gets the animated scroll to an edge that <see cref="ScrollToStart"/> or <see cref="ScrollToEnd"/> started, while it's
		/// on its way there: the edge, and where the animation stops. The anchor follows the edge past that point as UIKit
		/// measures the rows the animation brings into view. A newer request, a drag, getting there, or the animation ending
		/// ends it (see <see cref="ScrollAnimationEnded"/>).
		/// </summary>
		public (ScrollAnchorMode Edge, double Target)? EdgeAnimation => _tracker.IsApproaching ? _edgeAnimation : null;

		/// <summary>Follows the edges <paramref name="mode"/> selects instead, starting over as for a new view.</summary>
		public void UpdateMode(UICollectionView collectionView, ScrollAnchorMode mode)
		{
			_tracker.SetEdges(mode);
			_offsetChangedDuringLayout = false;
			_animatingRequest = false;
			collectionView.SetNeedsLayout();
		}

		/// <summary>Starts following the edge a new view shows again, as for a new set of items.</summary>
		public void Reset(UICollectionView collectionView)
		{
			_tracker.Reset();
			_offsetChangedDuringLayout = false;
			_animatingRequest = false;
			collectionView.SetNeedsLayout();
		}

		/// <summary>
		/// Called before UIKit lays the view out: before the controller's view lays out its subviews, and before UIKit updates
		/// the view for a change the controller or its items source makes (a new layout, items added, removed or moved), as
		/// UIKit can move the content offset before it updates the content size then.
		/// </summary>
		public void LayoutStarting() => _layoutDepth++;

		/// <summary>Called once UIKit has laid the view out (see <see cref="LayoutStarting"/>): looks at where the view is.</summary>
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
			_reportedOffsetY = collectionView.ContentOffset.Y;
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
			var metrics = Measure(collectionView, inset);

			// An offset change UIKit made during the layout, or one it never reported, is UIKit's.
			var sizeChanged = false;
			if (afterLayout)
			{
				sizeChanged = _offsetChangedDuringLayout || !IsReported(collectionView);
				_offsetChangedDuringLayout = false;
				_reportedOffsetY = collectionView.ContentOffset.Y;
			}

			// A scroll the delegate reports outside a layout pass is the view scrolling, also right after a drag, a deceleration
			// or an animated scroll ended, unless the content or the viewport changed size since the last look: the offset moved
			// for that change, as UIKit measures the rows a scroll brings into view only in the layout pass that follows.
			var scrolled = !afterLayout && !_tracker.IsResized(metrics);

			// A scroll UIKit animates that the anchor didn't start, as code moving the native view or a tap on the status bar
			// make, scrolls as a fling does: scrolling to the edge would stop it, also for the content changing size just as it
			// starts, while the view is still at the edge. It's judged once it ends, as a request's animated scroll is.
			var userScrolling = collectionView.Dragging || collectionView.Decelerating || _animatingRequest
				|| (_edgeAnimation is null && IsScrollAnimating(collectionView));
			if (_tracker.Update(metrics, userScrolling, sizeChanged, scrolled) is double edge)
			{
				ApplyOffset(collectionView, edge - inset.Top);
			}

			Publish(collectionView, itemsView, _tracker.IsAnchored);
		}

		// Whether UIKit animates a scroll. Before iOS and Mac Catalyst 17, which don't tell, it says it doesn't.
		static bool IsScrollAnimating(UICollectionView collectionView) =>
			(OperatingSystem.IsIOSVersionAtLeast(17) || OperatingSystem.IsMacCatalystVersionAtLeast(17)) && collectionView.ScrollAnimating;

		// Where the view is, measured from the start of its content, above the content inset.
		static ScrollAnchorMetrics Measure(UICollectionView collectionView, UIEdgeInsets inset) =>
			new(collectionView.ContentOffset.Y + inset.Top, collectionView.ContentSize.Height + inset.Top + inset.Bottom, collectionView.Bounds.Height);

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

		/// <summary>
		/// Called when an observer of the content offset sees it change. Mac Catalyst can move it without telling the scroll view
		/// delegate or laying the view out, as it can when the window changes: look again once the view is laid out, counting
		/// the move as UIKit's.
		/// </summary>
		public void OffsetObserved(UICollectionView collectionView)
		{
			if (!_detached && !_applying && !IsReported(collectionView))
			{
				collectionView.SetNeedsLayout();
			}
		}

		// Whether the scroll view delegate reported the content offset as it is.
		bool IsReported(UICollectionView collectionView) =>
			double.IsNaN(_reportedOffsetY) || Math.Abs(collectionView.ContentOffset.Y - _reportedOffsetY) <= 0.5;

		/// <summary>Called before the handler scrolls for a <c>ScrollTo</c> request.</summary>
		public void ScrollRequesting() => _requestingScroll = true;

		/// <summary>
		/// Called after the handler scrolled for a <c>ScrollTo</c> request (animating, or not): the next look judges where
		/// it went, instead of scrolling back to the edge the view followed. Not when the app made a newer request from what
		/// the scroll raised (its <c>Scrolled</c> event), or the view started following an edge meanwhile: that wins.
		/// </summary>
		/// <param name="collectionView">The collection view.</param>
		/// <param name="isLatest">Whether the request is still the latest (see <c>ItemsViewController2.IsLatestScrollRequest</c>).</param>
		/// <param name="animating">
		/// Whether the request started an animated scroll that's still on its way, to an item or to an edge the view doesn't
		/// follow, which UIKit reports the end of (see <see cref="ScrollAnimationEnded"/>). Until then it scrolls as a fling
		/// does: the looks before it moves would otherwise judge it where it starts, at the edge it leaves, and scroll back
		/// there for the content changing, which stops the animation.
		/// </param>
		public void ScrollRequested(UICollectionView collectionView, bool isLatest, bool animating = false)
		{
			_requestingScroll = false;
			if (isLatest)
			{
				_tracker.RequestScroll();
				_animatingRequest = animating;
			}

			collectionView.SetNeedsLayout();
		}

		/// <summary>
		/// Called when a scroll animation ended, the user started dragging, or the app requested another scroll: an animated
		/// scroll a request started (see <see cref="ScrollRequested"/>) no longer scrolls as a fling does, and one to an edge
		/// (see <see cref="EdgeAnimation"/>) is no longer on its way.
		/// </summary>
		public void RequestedScrollEnded(UICollectionView collectionView)
		{
			_edgeAnimation = null;
			if (_animatingRequest)
			{
				_animatingRequest = false;
				collectionView.SetNeedsLayout();
			}
		}

		/// <summary>
		/// Called when UIKit reports that a scroll animation ended (see <see cref="RequestedScrollEnded"/>). An animated scroll
		/// to the edge the view follows (see <see cref="EdgeAnimation"/>) that ended short of where it was going was stopped by
		/// a scroll the user isn't making, as the app's code, the keyboard or a tap on the status bar make: the next look judges
		/// where the view stopped, instead of following the edge. Not when the anchor stopped it, scrolling to the edge, nor
		/// UIKit laying the view out, or for the content or the viewport changing size: the view still goes to the edge then.
		/// The next look also judges where an animation the anchor didn't start left the view, as a fling (see <c>Look</c>).
		/// </summary>
		public void ScrollAnimationEnded(UICollectionView collectionView)
		{
			if (EdgeAnimation is { } animation && !_applying && _layoutDepth == 0 && !HasArrived(collectionView, animation)
				&& !_tracker.IsResized(Measure(collectionView, collectionView.AdjustedContentInset)))
			{
				_tracker.RequestScroll();
			}

			RequestedScrollEnded(collectionView);
			collectionView.SetNeedsLayout();
		}

		// Whether the view got where an animated scroll to an edge was going, or past it.
		static bool HasArrived(UICollectionView collectionView, (ScrollAnchorMode Edge, double Target) animation) =>
			animation.Edge == ScrollAnchorMode.End
				? collectionView.ContentOffset.Y >= animation.Target - 0.5
				: collectionView.ContentOffset.Y <= animation.Target + 0.5;

		/// <summary>Scrolls to the start, and follows it again. The view must be able to follow its start.</summary>
		/// <remarks>
		/// The app can request another scroll from what this raises (the <c>Scrolled</c> event of a jump, the change of
		/// <see cref="StructuredItemsView.IsAnchored"/>): that request is newer, and wins.
		/// </remarks>
		public void ScrollToStart(UICollectionView collectionView, StructuredItemsView? itemsView, bool animated)
		{
			// A view at the start already has nowhere to animate to, and UIKit wouldn't report an end: it jumps there instead
			// (below), which judges where it is now. Following the start on the way to an animation that never runs would ignore
			// a scroll away from it right after.
			if (animated && collectionView.Window is not null && !IsAt(collectionView, GetStartOffset(collectionView)))
			{
				// Animate to the start as laid out (the controller lays the view out first): a ScrollTo the app requests as it
				// hears the view follows its start stops the animation.
				_tracker.Engage(ScrollAnchorMode.Start);
				_edgeAnimation = SetOffsetToStart(collectionView, animated: true)
					? (ScrollAnchorMode.Start, GetStartOffset(collectionView))
					: null;
				if (itemsView is not null)
				{
					Publish(collectionView, itemsView, true);
				}

				return;
			}

			// Look again as for a new view that follows its start, and jump there, which stops any deceleration. The look then
			// judges a ScrollTo the app requested from the Scrolled event of the jump.
			_tracker.Reset(ScrollAnchorMode.Start);
			_edgeAnimation = null;
			_offsetChangedDuringLayout = false;
			ApplyOffset(collectionView, GetStartOffset(collectionView));
			collectionView.SetNeedsLayout();
			collectionView.LayoutIfNeeded();
		}

		/// <summary>Scrolls to the end, and follows it again. The view must be able to follow its end.</summary>
		/// <remarks>
		/// The app can request another scroll from what this raises (the <c>Scrolled</c> event of a jump, the change of
		/// <see cref="StructuredItemsView.IsAnchored"/>): that request is newer, and wins.
		/// </remarks>
		public void ScrollToEnd(UICollectionView collectionView, StructuredItemsView? itemsView, bool animated)
		{
			// A view at the end already jumps there instead, as at the start (see ScrollToStart).
			if (animated && collectionView.Window is not null && !IsAt(collectionView, GetEndOffset(collectionView)))
			{
				// Animate to the end as laid out (the controller lays the view out first): a ScrollTo the app requests as it
				// hears the view follows its end stops the animation.
				_tracker.Engage(ScrollAnchorMode.End);
				_edgeAnimation = SetOffsetToEnd(collectionView, animated: true) is double end ? (ScrollAnchorMode.End, end) : null;
				if (itemsView is not null)
				{
					Publish(collectionView, itemsView, true);
				}

				return;
			}

			// Look again as for a new view, and jump to the end as laid out now, which stops any deceleration: the look scrolls
			// to the exact end once the rows there are measured, and before the view first appears when it isn't in a window
			// yet. It judges a ScrollTo the app requested from the Scrolled event of the jump instead.
			_tracker.Reset(ScrollAnchorMode.End);
			_edgeAnimation = null;
			_offsetChangedDuringLayout = false;
			ApplyOffset(collectionView, GetEndOffset(collectionView));
			collectionView.SetNeedsLayout();
			collectionView.LayoutIfNeeded();
		}

		/// <summary>Scrolls a collection view to the start as laid out, once.</summary>
		/// <returns>
		/// Whether it started an animated scroll. UIKit keeps the rows in view where they are as it measures the rows the
		/// animation brings into view above them, which moves where the animation stops: <see cref="ScrollToStartOnceEnded"/>
		/// goes the rest of the way.
		/// </returns>
		public static bool ScrollToStartOnce(UICollectionView collectionView, bool animated) =>
			SetOffsetToStart(collectionView, animated) && animated;

		/// <summary>
		/// Called when an animated scroll to the start that <see cref="ScrollToStartOnce"/> started has stopped: goes the rest
		/// of the way, unless a scroll stopped it and took the view away, as code moving the native view does. UIKit keeps the
		/// rows in view where they are as it measures the rows the animation brings into view above them, so the animation
		/// stops short of the start by as much as those rows grew, as the content did, or past it when they shrank. A view
		/// further from the start than the content grew meanwhile, by more than the anchor's threshold, was taken there by
		/// another scroll.
		/// </summary>
		/// <param name="collectionView">The collection view.</param>
		/// <param name="contentHeight">The height of the content when the animation started.</param>
		public static void ScrollToStartOnceEnded(UICollectionView collectionView, double contentHeight)
		{
			var grown = Math.Max(0, collectionView.ContentSize.Height - contentHeight);
			if (collectionView.ContentOffset.Y - GetStartOffset(collectionView) <= grown + ScrollAnchorTracker.DefaultThreshold)
			{
				SetOffsetToStart(collectionView, animated: false);
			}
		}

		/// <summary>Starts an animated scroll of a collection view to the end as laid out, once.</summary>
		/// <returns>
		/// Where the animation stops, or <see langword="null"/> when the view is at the end already. The rows the animation
		/// scrolls past are measured on the way, which can move the end beyond that point: the controller goes the rest of the
		/// way once the animation ends.
		/// </returns>
		public static double? AnimateToEndOnce(UICollectionView collectionView) => SetOffsetToEnd(collectionView, animated: true);

		// Returns whether it scrolls: not when the view is at the start already.
		static bool SetOffsetToStart(UICollectionView collectionView, bool animated)
		{
			var start = GetStartOffset(collectionView);
			if (IsAt(collectionView, start))
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
			if (IsAt(collectionView, end))
			{
				return null;
			}

			collectionView.SetContentOffset(new CGPoint(collectionView.ContentOffset.X, end), animated);
			return end;
		}

		// Whether the view is at an offset, give or take rounding.
		static bool IsAt(UICollectionView collectionView, double offsetY) => Math.Abs(collectionView.ContentOffset.Y - offsetY) <= 0.5;

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
#if MACCATALYST
				// Following an edge, the anchor keeps the view there, also after a move Mac Catalyst makes silently (see
				// OffsetObserved). A restore an earlier ScrollTo armed (see MauiCollectionView) would scroll back to its item as
				// the anchor moves the view up for the content shrinking. A drag clears it, but the view can get back to the edge
				// without one, as the keyboard or accessibility scroll it.
				if (isAnchored)
				{
					mauiCollectionView.ClearPendingScrollRestore();
				}
#endif
			}

			if (itemsView.IsAnchored != isAnchored)
			{
				itemsView.SetIsAnchored(isAnchored);
			}
		}
	}
}
