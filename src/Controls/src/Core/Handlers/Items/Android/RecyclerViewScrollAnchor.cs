#nullable enable
using System;
using AndroidX.RecyclerView.Widget;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	/// <summary>
	/// Keeps the end of a <see cref="StructuredItemsView"/>'s <see cref="RecyclerView"/> in view for
	/// <see cref="ScrollAnchorMode.End"/>, grouped or not, with or without a header and footer (the adapter lays those
	/// out as items too).
	/// </summary>
	/// <remarks>
	/// <para>
	/// The recycler view reports to it after each layout (<see cref="LayoutFinished"/>), after it scrolled
	/// (<see cref="Scrolled"/>), and when its scroll state changes (<see cref="ScrollStateChanged"/>): it looks at the
	/// view then, and scrolls it to the end, without animating, when the content changed size while it followed the end.
	/// </para>
	/// <para>
	/// RecyclerView reports its scroll offset and range as estimates: LinearLayoutManager extrapolates them from the
	/// average size of the rows on screen, so they jump whenever rows of another size scroll in, and can even move back
	/// while the view scrolls forward. The anchor measures from the layout instead. The offset is exact while the first row
	/// is laid out, and otherwise moves by exactly as much as a row that stayed laid out (at the same position, in the same
	/// view) moved since the last look, or, when the view scrolled past every row it had laid out (a fast fling or animated
	/// scroll), by exactly as much as it reported scrolling. The distance to the end is exact whenever the last row is laid
	/// out: the end of the lowest row against the end of the viewport.
	/// </para>
	/// <para>
	/// The user is scrolling while the scroll state isn't idle: dragging, or settling after a fling (or after an animated
	/// scroll the app requested, which the anchor judges like a fling). It is vertical only.
	/// </para>
	/// </remarks>
	sealed class RecyclerViewScrollAnchor
	{
		readonly RecyclerView _recyclerView;
		readonly StructuredItemsView _itemsView;
		readonly ScrollAnchorTracker _tracker = new();
		readonly float _density;
		RecyclerView.LayoutManager? _layoutManager;
		OrientationHelper? _helper;
		AView? _reference;
		int _referencePosition;
		int _referenceStart;
		int _estimatedOffset;
		int _scrolledSinceLook;
		double _offset;
		bool _hasOffset;
		int _layoutDepth;
		bool _finishingLayout;
		bool _updating;
		bool _requestPending;
		bool _wasVertical;
		bool _detached;
		Java.Lang.IRunnable? _postedRunnable;
		bool _updatePosted;
		bool _jumpPosted;

		public RecyclerViewScrollAnchor(RecyclerView recyclerView, StructuredItemsView itemsView)
		{
			_recyclerView = recyclerView;
			_itemsView = itemsView;
			var density = recyclerView.Resources?.DisplayMetrics?.Density ?? 1f;
			_density = density > 0 ? density : 1f;
			_wasVertical = recyclerView.GetLayoutManager()?.CanScrollVertically() ?? true;
			Publish();
			JumpToEnd();
		}

		/// <summary>Gets whether the view follows the end of its content right now.</summary>
		public bool IsFollowingEnd => !_detached && _wasVertical && _tracker.IsAnchored;

		bool IsUserScrolling => _recyclerView.ScrollState != RecyclerView.ScrollStateIdle;

		/// <summary>Starts following the end again, as for a new set of items.</summary>
		public void Reset()
		{
			_tracker.Reset();
			_requestPending = false;
			ResetMeasurement();
			Publish();
			JumpToEnd();
		}

		/// <summary>Called before the recycler view lays out its children.</summary>
		public void LayoutStarting() => _layoutDepth++;

		/// <summary>Called after the recycler view laid out its children (and any scroll adjustment that follows).</summary>
		public void LayoutFinished()
		{
			if (_layoutDepth > 0)
			{
				_layoutDepth--;
			}

			if (_layoutDepth > 0)
			{
				return;
			}

			// A ScrollTo that doesn't animate moves the view in this layout: judge it by where it landed.
			if (_requestPending)
			{
				_requestPending = false;
				_tracker.RequestScroll();
			}

			_finishingLayout = true;
			try
			{
				Update();
			}
			finally
			{
				_finishingLayout = false;
			}
		}

		/// <summary>Called after the recycler view scrolled.</summary>
		public void Scrolled(int dx, int dy)
		{
			// Every scroll moves the content, whoever made it: the measurement counts it even when this doesn't look now.
			_scrolledSinceLook += dy;

			if (_updating || _layoutDepth > 0 || _finishingLayout)
			{
				return;
			}

			// RecyclerView reports a layout it ran on its own (to consume pending updates as a scroll starts) as a scroll
			// by nothing: look after it, outside of that layout.
			if (dx == 0 && dy == 0)
			{
				PostUpdate();
				return;
			}

			Update();
		}

		/// <summary>Called when the scroll state changes: a drag or fling that ends may need settling on the end.</summary>
		public void ScrollStateChanged(int state)
		{
			if (state == RecyclerView.ScrollStateIdle)
			{
				PostUpdate();
			}
		}

		/// <summary>
		/// Called when the app requests a scroll (<c>ScrollTo</c>): it is judged by where it lands, instead of being scrolled
		/// back to the end.
		/// </summary>
		public void ScrollRequested(bool animated)
		{
			if (animated)
			{
				// It scrolls like a fling: judge it as one.
				_tracker.RequestScroll();
			}
			else
			{
				// It scrolls in the next layout.
				_requestPending = true;
			}
		}

		/// <summary>Scrolls to the end, and follows it again.</summary>
		public void ScrollToEnd(bool animate)
		{
			if (_detached)
			{
				return;
			}

			_requestPending = false;
			if (animate && GetLayout() is var (layoutManager, _) && layoutManager.ItemCount > 0)
			{
				// It scrolls like a fling toward the end, which never disengages the anchor; once it settles, the anchor
				// scrolls the rest of the way if the end moved meanwhile.
				_tracker.Engage();
				Publish();
				_recyclerView.SmoothScrollToPosition(layoutManager.ItemCount - 1);
				return;
			}

			// Look again as for a new view: the look after the jump scrolls to the exact end.
			_recyclerView.StopScroll();
			_tracker.Reset();
			Publish();
			JumpToEnd();
		}

		/// <summary>Stops anchoring: the view no longer follows its end.</summary>
		public void Detach()
		{
			_detached = true;
			if (_postedRunnable is not null)
			{
				_recyclerView.RemoveCallbacks(_postedRunnable);
				_postedRunnable.Dispose();
				_postedRunnable = null;
			}

			Publish();
		}

		void Update()
		{
			if (_updating || _detached)
			{
				return;
			}

			var isVertical = _recyclerView.GetLayoutManager()?.CanScrollVertically() == true;
			if (isVertical != _wasVertical)
			{
				// A new layout that scrolls vertically again follows the end from the start, as a new view does.
				_wasVertical = isVertical;
				_tracker.Reset();
				ResetMeasurement();
			}

			if (isVertical && ReadMetrics() is ScrollAnchorMetrics metrics)
			{
				_updating = true;
				try
				{
					if (_tracker.Update(metrics, IsUserScrolling) is not null)
					{
						CorrectToEnd();
					}
				}
				finally
				{
					_updating = false;
				}
			}

			Publish();
		}

		void Publish()
		{
			var isAnchored = IsFollowingEnd;
			if (_itemsView.IsAnchored != isAnchored)
			{
				_itemsView.SetIsAnchored(isAnchored);
			}
		}

		void ResetMeasurement()
		{
			_layoutManager = null;
			_helper = null;
			_hasOffset = false;
			_reference = null;
			_scrolledSinceLook = 0;
		}

		ScrollAnchorMetrics? ReadMetrics()
		{
			if (GetLayout() is not var (layoutManager, helper))
			{
				return null;
			}

			var viewport = helper.TotalSpace;
			var itemCount = layoutManager.ItemCount;
			var childCount = layoutManager.ChildCount;
			if (itemCount == 0)
			{
				_hasOffset = false;
				_reference = null;
				_scrolledSinceLook = 0;
				return new ScrollAnchorMetrics(0, 0, viewport / _density);
			}

			if (childCount == 0)
			{
				return null;
			}

			AView? first = null;
			var firstPosition = 0;
			var firstStart = 0;
			var referenceStart = 0;
			var referenceLaidOut = false;
			int? startOffset = null;
			var end = int.MinValue;
			var lastLaidOut = false;
			for (var index = 0; index < childCount; index++)
			{
				if (layoutManager.GetChildAt(index) is not { } child)
				{
					continue;
				}

				var start = helper.GetDecoratedStart(child);
				var position = layoutManager.GetPosition(child);
				if (first is null || start < firstStart)
				{
					first = child;
					firstPosition = position;
					firstStart = start;
				}

				// The view alone doesn't identify the row: RecyclerView reuses the view of a row that scrolled away for
				// another row, even within one frame of a fast scroll.
				if (ReferenceEquals(child, _reference) && position == _referencePosition)
				{
					referenceLaidOut = true;
					referenceStart = start;
				}

				if (position == 0)
				{
					startOffset = helper.StartAfterPadding - start;
				}

				if (position == itemCount - 1)
				{
					lastLaidOut = true;
				}

				// The lowest row ends the content: in a grid the last item can sit beside a taller one.
				end = Math.Max(end, helper.GetDecoratedEnd(child));
			}

			var estimatedOffset = _recyclerView.ComputeVerticalScrollOffset();
			var scrolled = _scrolledSinceLook;
			_scrolledSinceLook = 0;
			if (startOffset is int exact)
			{
				_offset = exact;
			}
			else if (_hasOffset && referenceLaidOut)
			{
				_offset += _referenceStart - referenceStart;
			}
			else if (_hasOffset && scrolled != 0)
			{
				_offset += scrolled;
			}
			else if (_hasOffset)
			{
				_offset += estimatedOffset - _estimatedOffset;
			}
			else
			{
				_offset = estimatedOffset;
			}

			_hasOffset = true;
			_estimatedOffset = estimatedOffset;
			_reference = first;
			_referencePosition = firstPosition;
			_referenceStart = firstStart;

			// Before the last row is laid out, all that's certain is that the end is beyond the viewport.
			double distance = lastLaidOut
				? end - helper.EndAfterPadding
				: Math.Max(
					_recyclerView.ComputeVerticalScrollRange() - estimatedOffset - _recyclerView.ComputeVerticalScrollExtent(),
					viewport);

			var offset = _offset / _density;
			var viewportUnits = viewport / (double)_density;
			return new ScrollAnchorMetrics(offset, offset + viewportUnits + distance / _density, viewportUnits);
		}

		void CorrectToEnd()
		{
			if (GetLayout() is not var (layoutManager, helper))
			{
				return;
			}

			var last = layoutManager.ItemCount - 1;
			if (last < 0)
			{
				return;
			}

			if (layoutManager.FindViewByPosition(last) is null)
			{
				// Lay the last row out first; the look after that layout scrolls the rest of the way. Scrolling to a
				// position asks for a layout, which is lost if asked for during one: ask once it's over.
				if (_layoutDepth > 0 || _finishingLayout)
				{
					PostJumpToEnd();
				}
				else
				{
					_recyclerView.ScrollToPosition(last);
				}

				return;
			}

			var end = int.MinValue;
			for (var index = 0; index < layoutManager.ChildCount; index++)
			{
				if (layoutManager.GetChildAt(index) is { } child)
				{
					end = Math.Max(end, helper.GetDecoratedEnd(child));
				}
			}

			var distance = end - helper.EndAfterPadding;
			if (distance > 0)
			{
				_recyclerView.ScrollBy(0, distance);
			}
		}

		// Lays the last row out at the end in the next layout, so a view that starts at its end shows it from its first frame.
		void JumpToEnd()
		{
			if (IsFollowingEnd && !IsUserScrolling && _recyclerView.GetLayoutManager() is { ItemCount: > 0 } layoutManager)
			{
				_recyclerView.ScrollToPosition(layoutManager.ItemCount - 1);
			}
		}

		void PostUpdate() => Post(ref _updatePosted);

		void PostJumpToEnd() => Post(ref _jumpPosted);

		void Post(ref bool posted)
		{
			if (posted || _detached)
			{
				return;
			}

			var alreadyPosted = _updatePosted || _jumpPosted;
			posted = true;
			if (!alreadyPosted)
			{
				_postedRunnable ??= new Java.Lang.Runnable(RunPosted);
				_recyclerView.PostOnAnimation(_postedRunnable);
			}
		}

		void RunPosted()
		{
			var jump = _jumpPosted;
			_updatePosted = false;
			_jumpPosted = false;
			if (_detached)
			{
				return;
			}

			// The look that asked for a jump already counted on the view being at the end: get it there, and the layout
			// that follows looks again.
			if (jump)
			{
				JumpToEnd();
			}
			else
			{
				Update();
			}
		}

		(RecyclerView.LayoutManager LayoutManager, OrientationHelper Helper)? GetLayout()
		{
			var layoutManager = _recyclerView.GetLayoutManager();
			if (layoutManager is null || !layoutManager.CanScrollVertically() || _recyclerView.Height <= 0)
			{
				return null;
			}

			if (!ReferenceEquals(layoutManager, _layoutManager) || _helper is not { } helper)
			{
				// The layout manager is replaced when ItemsLayout changes or the empty view shows: start measuring again.
				helper = OrientationHelper.CreateVerticalHelper(layoutManager);
				if (helper is null)
				{
					return null;
				}

				_layoutManager = layoutManager;
				_helper = helper;
				_hasOffset = false;
				_reference = null;
				_scrolledSinceLook = 0;
			}

			return (layoutManager, helper);
		}
	}
}
