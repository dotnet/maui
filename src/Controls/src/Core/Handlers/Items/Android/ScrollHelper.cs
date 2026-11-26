#nullable disable
using System;
using AndroidX.RecyclerView.Widget;
using ARect = Android.Graphics.Rect;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class ScrollHelper : RecyclerView.OnScrollListener
	{
		readonly RecyclerView _recyclerView;
		Action _pendingScrollAdjustment;

		bool _undoNextScrollAdjustment;
		bool _maintainingScrollOffsets;
		bool _isAtScrollOrigin = true;
		int _lastScrollX;
		int _lastScrollY;
		int _lastDeltaX;
		int _lastDeltaY;

		public ScrollHelper(RecyclerView recyclerView)
		{
			_recyclerView = recyclerView;
		}

		// Used by the renderer to maintain scroll offset when using ItemsUpdatingScrollMode KeepScrollOffset
		public void UndoNextScrollAdjustment()
		{
			_undoNextScrollAdjustment = true;

			_lastScrollX = _recyclerView.ComputeHorizontalScrollOffset();
			_lastScrollY = _recyclerView.ComputeVerticalScrollOffset();
		}

		public void AdjustScroll()
		{
			// This is the layout that makes the jumps requested before it.
			IsJumpPending = false;
			_pendingScrollAdjustment?.Invoke();
		}

		// Called for a newer scroll request that doesn't go through this helper (the anchor's jump to an edge), and to stop a
		// requested scroll: the adjustment an earlier jump left for its layout no longer applies, and neither does an animated
		// scroll waiting for the view to be laid out.
		public void CancelPendingAdjustment()
		{
			_pendingScrollAdjustment = null;
		}

		public void AnimateScrollToPosition(int index, ScrollToPosition scrollToPosition)
		{
			// A newer request replaces an adjustment an earlier jump left for its layout.
			_pendingScrollAdjustment = null;

			if (scrollToPosition == ScrollToPosition.MakeVisible)
			{
				// MakeVisible matches the Android default of SnapAny, so we can just use the default
				_recyclerView.SmoothScrollToPosition(index);
			}
			else
			{
				// If we want a different ScrollToPosition, we need to create a SmoothScroller which can handle it
				var smoothScroller = new PositionalSmoothScroller(_recyclerView.Context, scrollToPosition)
				{
					TargetPosition = index
				};

				// And kick off the scroll operation
				_recyclerView.GetLayoutManager()?.StartSmoothScroll(smoothScroller);
			}
		}

		public void JumpScrollToPosition(int index, ScrollToPosition scrollToPosition, bool uniformSize = false)
		{
			// A newer request replaces an adjustment an earlier jump left for its layout.
			_pendingScrollAdjustment = null;
			LayoutAfterJump();
			IsJumpPending = true;

			if (scrollToPosition == ScrollToPosition.MakeVisible)
			{
				// MakeVisible is the default behavior, so we don't need to do anything special
				_recyclerView.ScrollToPosition(index);
				return;
			}

			if (!(_recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager))
			{
				// We don't have the ScrollToPositionWithOffset method available, so we don't have a way to 
				// handle the Forms ScrollToPosition; just default back to the MakeVisible behavior
				_recyclerView.ScrollToPosition(index);
				return;
			}

			// If ScrollToPosition is Start, then we can just use an offset of 0 and we're fine
			// (Though that may change in RTL situations or if we're stacking from the end)
			if (scrollToPosition == Microsoft.Maui.Controls.ScrollToPosition.Start)
			{
				linearLayoutManager.ScrollToPositionWithOffset(index, 0);
				return;
			}

			// For handling End or Center, things get more complicated because we need to know the size of
			// the View we're targeting. 

			// If we're using ItemSizingStrategy MeasureFirstItem, we can use any item size as a guide to figure
			// out the offset

			// TODO hartez 2018/10/03 14:00:32 Handle the MeasureFirstItem case to determine the offset and call	
			// `linearLayoutManager.ScrollToPositionWithOffset(index, offset);` here
			// Use the uniformSize parameter and pass that in based on ItemsView.ItemSizingStrategy

			// If we don't already know the size of the item, things get more complicated

			// The item may not actually exist; it may have never been realized, or it may have been recycled
			// So we need to get it on screen using ScrollToPosition, then once it's on screen we can use the 
			// width/height to make adjustments for Center/End.

			// ScrollToPosition queues up the scroll operation. It doesn't do it immediately; it requests a layout
			// After that layout is finished, the view will be available for measurement and then we can adjust
			// the scroll to get it into the right place

			// Set up our pending adjustment
			if (linearLayoutManager.CanScrollVertically())
			{
				_pendingScrollAdjustment = () => AdjustVerticalScroll(index, scrollToPosition);
			}
			else
			{
				_pendingScrollAdjustment = () => AdjustHorizontalScroll(index, scrollToPosition);
			}

			// Kick off the Scroll to get the item into view; once it's in view, the pending adjustment will kick in
			_recyclerView.ScrollToPosition(index);
		}

		// Scrolls, without animating, until the start of the first row meets the start of the viewport: the start of the
		// content, above the header. Scrolling to the first position lines it up with the start of the viewport whenever
		// it isn't wholly in view.
		public void JumpScrollToStart()
		{
			// A newer request replaces an adjustment an earlier jump left for its layout.
			_pendingScrollAdjustment = null;

			if (_recyclerView.GetLayoutManager() is { ItemCount: > 0 })
			{
				LayoutAfterJump();
				IsJumpPending = true;
				_recyclerView.ScrollToPosition(0);
			}
		}

		// Scrolls, without animating, until the end of the lowest row meets the end of the viewport: the end of the
		// content, past the footer.
		public void JumpScrollToEnd()
		{
			if (_recyclerView.GetLayoutManager() is not { ItemCount: > 0 } layoutManager)
			{
				return;
			}

			// Scrolling to the last position lays it out lined up with the end of the viewport when it's out of view, but
			// with the start when it's in view and taller than the viewport: once it's laid out, scroll the rest of the way.
			_pendingScrollAdjustment = AdjustScrollToEnd;
			LayoutAfterJump();
			IsJumpPending = true;
			_recyclerView.ScrollToPosition(layoutManager.ItemCount - 1);
		}

		/// <summary>
		/// Gets whether a jump this made (<see cref="JumpScrollToPosition"/>, <see cref="JumpScrollToStart"/>,
		/// <see cref="JumpScrollToEnd"/>) hasn't been laid out yet: RecyclerView scrolls to the position the jump set in the next
		/// layout, or in the one after when the jump was made during a layout.
		/// </summary>
		public bool IsJumpPending { get; private set; }

		/// <summary>
		/// Gets how many requests that scroll in or after the next layout have been made: a jump, which scrolls in that layout
		/// and then applies the adjustment it leaves, or an animated scroll that starts once the view is laid out. One made while
		/// the view lays out, as from the Scrolled event RecyclerView raises then, waits for the layout after that one (see
		/// MauiRecyclerView.OnLayout).
		/// </summary>
		public int Jumps { get; private set; }

		/// <summary>
		/// Starts <paramref name="animateScroll"/> once the view is laid out, as the adjustment a jump leaves for its layout:
		/// a newer request replaces it. RecyclerView's smooth scroller works out how far to go in the next frame's animation,
		/// which comes before that frame lays the view out, so a change the app made to the rows with the request (a row that
		/// grew) would move its target after it stopped. And a jump an earlier request left for that layout would land after
		/// the animation, which stops right away when it finds its target in place.
		/// </summary>
		public void AnimateScrollOnceLaidOut(Action animateScroll)
		{
			_pendingScrollAdjustment = () =>
			{
				_pendingScrollAdjustment = null;
				animateScroll();
			};

			LayoutAfterJump();
			if (!_recyclerView.IsInLayout)
			{
				_recyclerView.RequestLayout();
			}
		}

		// A jump scrolls in the layout it asks for. Asked for during a layout, as when the app scrolls from the Scrolled event
		// RecyclerView raises as it lays out, that layout is lost: ask again once the layout is over.
		void LayoutAfterJump()
		{
			Jumps++;
			if (_recyclerView.IsInLayout)
			{
				_recyclerView.Post(_recyclerView.RequestLayout);
			}
		}

		void AdjustScrollToEnd()
		{
			_pendingScrollAdjustment = null;

			if (_recyclerView.GetLayoutManager() is { ItemCount: > 0 } layoutManager
				&& layoutManager.FindViewByPosition(layoutManager.ItemCount - 1) is not null
				&& OrientationHelper.CreateVerticalHelper(layoutManager) is { } helper
				&& RecyclerViewScrollAnchor.GetDistanceToEnd(layoutManager, helper) is > 0 and var distance)
			{
				_recyclerView.ScrollBy(0, distance);
			}
		}

		ARect GetViewRect(int index)
		{
			var holder = _recyclerView.FindViewHolderForAdapterPosition(index);
			var view = holder?.ItemView;

			if (view == null)
			{
				return null;
			}

			var viewRect = new ARect();
			view.GetGlobalVisibleRect(viewRect);

			return viewRect;
		}

		void AdjustVerticalScroll(int index, ScrollToPosition scrollToPosition)
		{
			_pendingScrollAdjustment = null;

			var viewRect = GetViewRect(index);

			if (viewRect == null)
			{
				return;
			}

			var offset = 0;

			var rvRect = new ARect();
			_recyclerView.GetGlobalVisibleRect(rvRect);

			if (scrollToPosition == ScrollToPosition.Center)
			{
				offset = viewRect.CenterY() - rvRect.CenterY();
			}
			else if (scrollToPosition == ScrollToPosition.End)
			{
				offset = viewRect.Bottom - rvRect.Bottom;
			}

			_recyclerView.ScrollBy(0, offset);
		}

		void AdjustHorizontalScroll(int index, ScrollToPosition scrollToPosition)
		{
			_pendingScrollAdjustment = null;

			var viewRect = GetViewRect(index);

			if (viewRect == null)
			{
				return;
			}

			var offset = 0;

			var rvRect = new ARect();
			_recyclerView.GetGlobalVisibleRect(rvRect);

			if (scrollToPosition == ScrollToPosition.Center)
			{
				offset = viewRect.CenterX() - rvRect.CenterX();
			}
			else if (scrollToPosition == ScrollToPosition.End)
			{
				offset = viewRect.Right - rvRect.Right;
			}

			_recyclerView.ScrollBy(offset, 0);
		}

		void TrackOffsets()
		{
			var newXOffset = _recyclerView.ComputeHorizontalScrollOffset();
			var newYOffset = _recyclerView.ComputeVerticalScrollOffset();

			_lastDeltaX = Math.Max(newXOffset - _lastScrollX, 0);
			_lastDeltaY = Math.Max(newYOffset - _lastScrollY, 0);

			_lastScrollX = newXOffset;
			_lastScrollY = newYOffset;

			if (_undoNextScrollAdjustment)
			{
				// This last scroll adjustment happened because a new item was added and it caused the scroll
				// offset to shift; since the ItemsUpdatingScrollMode is set to KeepScrollOffset; we need to undo 
				// that shift and stay where we were before the item was added

				if (_isAtScrollOrigin)
				{
					_recyclerView.ScrollBy(-_lastDeltaX, -_lastDeltaY);
				}

				_undoNextScrollAdjustment = false;
				_lastDeltaX = 0;
				_lastDeltaY = 0;
			}
			else
			{
				_isAtScrollOrigin = newXOffset == 0
					&& newYOffset == 0;
			}
		}

		public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			base.OnScrolled(recyclerView, dx, dy);
			TrackOffsets();
		}

		internal void AddScrollListener()
		{
			// Set up scroll listener to track the scroll offsets when we're using KeepScrollOffset.
			if (!_maintainingScrollOffsets)
			{
				_maintainingScrollOffsets = true;
				_recyclerView.AddOnScrollListener(this);
			}
		}

		internal void RemoveScrollListener()
		{
			// Remove the scroll listener when we're done and no longer need to track the offsets.
			if (_maintainingScrollOffsets)
			{
				_maintainingScrollOffsets = false;
				_recyclerView.RemoveOnScrollListener(this);
			}
		}
	}
}
