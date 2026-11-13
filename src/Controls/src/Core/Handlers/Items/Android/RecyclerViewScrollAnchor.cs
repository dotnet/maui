#nullable enable
using System;
using AndroidX.RecyclerView.Widget;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	/// <summary>
	/// Keeps an edge of a <see cref="StructuredItemsView"/>'s <see cref="RecyclerView"/> in view for
	/// <see cref="ScrollAnchorMode.Start"/> and <see cref="ScrollAnchorMode.End"/>, grouped or not, with or without a
	/// header and footer (the adapter lays those out as items too).
	/// </summary>
	/// <remarks>
	/// <para>
	/// The recycler view reports to it after each layout (<see cref="LayoutFinished"/>), after it scrolled
	/// (<see cref="Scrolled"/>), and when its scroll state changes (<see cref="ScrollStateChanged"/>): it looks at the
	/// view then, and scrolls it to the edge it follows, without animating, when the content changed size while it
	/// followed it. RecyclerView keeps the first row it shows where it is when rows are added before it, so rows added
	/// before the first row while the view is at its start land above the viewport: following the start scrolls them into
	/// view.
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
	/// RecyclerView keeps the first row it shows where it is as it applies changes to the items, so rows added before it
	/// move the offset, even with the content the size it was (when rows are removed after it in the same update). The
	/// adapter reports those changes (<see cref="ItemsChanged"/>): the look after the layout that applies them counts the
	/// offset change as the layout's.
	/// </para>
	/// <para>
	/// The user is scrolling while the scroll state isn't idle: dragging, or settling after a fling (or after an animated
	/// scroll the app requested, which the anchor judges like a fling). It is vertical only.
	/// </para>
	/// <para>
	/// The anchor raises <see cref="StructuredItemsView.IsAnchored"/> changes, and RecyclerView raises scroll events as it
	/// lays out: the app can request a scroll from either. The newest request wins: the anchor doesn't scroll for an older
	/// one after reporting, and judges a request made during a layout once the layout it asks for has landed it.
	/// </para>
	/// </remarks>
	sealed class RecyclerViewScrollAnchor
	{
		readonly RecyclerView _recyclerView;
		readonly StructuredItemsView _itemsView;
		readonly ScrollAnchorTracker _tracker;
		readonly float _density;
		object? _itemsSource;
		bool _isGrouped;
		RecyclerView.LayoutManager? _layoutManager;
		OrientationHelper? _helper;
		AView? _reference;
		int _referencePosition;
		int _referenceStart;
		int _estimatedOffset;
		int _scrolledSinceLook;
		double _offset;
		bool _hasOffset;

		// How far to scroll, in pixels, to the start (negative, or zero) and to the end (positive, or zero), as the last look
		// measured them; null when the row at that edge wasn't laid out.
		int? _toStart;
		int? _toEnd;

		int _layoutDepth;
		bool _finishingLayout;
		bool _updating;
		bool _itemsChanged;

		// A ScrollTo that doesn't animate lands in the next layout: the one after this layout when the app requested it during
		// this one.
		bool _requestPending;
		bool _requestLandsLater;

		// Counts the scrolls requested: one requested while the anchor reports is newer than the scroll it was about to make.
		int _scrollRequests;

		bool _wasVertical;
		bool _detached;
		Java.Lang.IRunnable? _postedRunnable;
		bool _updatePosted;
		bool _jumpPosted;

		public RecyclerViewScrollAnchor(RecyclerView recyclerView, StructuredItemsView itemsView, ScrollAnchorMode mode)
		{
			_tracker = new ScrollAnchorTracker(mode);
			_recyclerView = recyclerView;
			_itemsView = itemsView;
			_itemsSource = itemsView.ItemsSource;
			_isGrouped = (itemsView as GroupableItemsView)?.IsGrouped == true;
			var density = recyclerView.Resources?.DisplayMetrics?.Density ?? 1f;
			_density = density > 0 ? density : 1f;
			_wasVertical = recyclerView.GetLayoutManager()?.CanScrollVertically() ?? true;
		}

		/// <summary>
		/// Starts anchoring: reports that the view follows an edge, and goes to that edge. Call it once the anchor is installed:
		/// the app can turn anchoring off as it hears the view is anchored, which detaches the installed anchor.
		/// </summary>
		public void Attach()
		{
			StopRequestedScroll();
			PublishAndJumpToEdge();
		}

		/// <summary>Gets the edges the view can follow.</summary>
		public ScrollAnchorMode Edges => _tracker.Edges;

		/// <summary>Gets whether the view can follow <paramref name="edge"/>.</summary>
		public bool CanFollow(ScrollAnchorMode edge) => _tracker.CanFollow(edge);

		/// <summary>Gets whether the view follows an edge of its content right now.</summary>
		public bool IsFollowingEdge => !_detached && _wasVertical && _tracker.IsAnchored;

		bool IsUserScrolling => _recyclerView.ScrollState != RecyclerView.ScrollStateIdle;

		/// <summary>Follows the edges <paramref name="mode"/> selects instead, starting over as for a new view.</summary>
		public void UpdateMode(ScrollAnchorMode mode)
		{
			_tracker.SetEdges(mode);
			Restart();
		}

		/// <summary>Starts following the edge a new view shows again, as for a new set of items.</summary>
		public void Reset()
		{
			_tracker.Reset();
			Restart();
		}

		// Measures from scratch, and goes to the edge the view now follows. It's newer than a scroll the anchor was about to
		// make for an earlier request, as when the app changes AnchorMode as it hears the view follows an edge.
		void Restart()
		{
			_scrollRequests++;
			_requestPending = false;
			_requestLandsLater = false;
			ResetMeasurement();
			StopRequestedScroll();
			PublishAndJumpToEdge();
		}

		// An animated scroll the app requested earlier (or an earlier ScrollToStart or ScrollToEnd) would carry on to where it
		// was going: the edge the view follows now supersedes it. A fling is the user's, and carries on: the anchor goes to
		// the edge once it settles, unless the user scrolled away.
		void StopRequestedScroll()
		{
			if (_recyclerView.GetLayoutManager()?.IsSmoothScrolling == true)
			{
				_recyclerView.StopScroll();
			}
		}

		// Reports whether the view follows an edge, and goes to it: unless the app requested a scroll as it heard, which is
		// newer.
		void PublishAndJumpToEdge()
		{
			var requests = _scrollRequests;
			Publish();
			if (requests == _scrollRequests)
			{
				JumpToEdge();
			}
		}

		/// <summary>
		/// Called when the adapter reports items added, removed, moved or changed: the layout that applies them moves the
		/// content, not the user.
		/// </summary>
		public void ItemsChanged() => _itemsChanged = true;

		/// <summary>
		/// Called after the recycler view replaced its adapter, which lays it out again from its start: for new items, or for
		/// the same items with a new header, footer or template.
		/// </summary>
		public void AdapterReplaced()
		{
			var itemsSource = _itemsView.ItemsSource;
			var isGrouped = (_itemsView as GroupableItemsView)?.IsGrouped == true;
			var newItems = !ReferenceEquals(itemsSource, _itemsSource) || isGrouped != _isGrouped;
			_itemsSource = itemsSource;
			_isGrouped = isGrouped;

			// New items start at the edge a new view shows, as the view did when it first appeared. The same items go back to
			// the edge the view followed, and otherwise stay where the new adapter lays them out.
			if (newItems)
			{
				Reset();
			}
			else if (_tracker.IsAnchored)
			{
				_tracker.Reset(_tracker.AnchoredEdge);
				Restart();
			}
			else
			{
				ResetMeasurement();
			}
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

			// A ScrollTo that doesn't animate moves the view in this layout: judge it by where it landed. One the app requested
			// during this layout, from an event it raised, lands in the layout it asked for: that one judges it.
			if (_requestPending)
			{
				if (_requestLandsLater)
				{
					_requestLandsLater = false;
					return;
				}

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

			// RecyclerView lays out and measures the rows a scroll brings into view as it scrolls, which can change the size
			// of the content it estimates: the move is still the scroll's.
			Update(scrolled: true);
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
		/// back to the edge the view followed.
		/// </summary>
		public void ScrollRequested(bool animated)
		{
			_scrollRequests++;

			// It supersedes a jump to an edge queued after the last layout.
			_jumpPosted = false;

			if (animated)
			{
				// It scrolls like a fling: judge it as one.
				_tracker.RequestScroll();
			}
			else
			{
				// It scrolls in the next layout: when requested during a layout, the one after it.
				_requestPending = true;
				_requestLandsLater = _layoutDepth > 0;
			}
		}

		/// <summary>Scrolls to the start, and follows it again. The view must be able to follow its start.</summary>
		public void ScrollToStart(bool animate)
		{
			if (_detached)
			{
				return;
			}

			// It's newer than any scroll requested before.
			_scrollRequests++;
			_requestPending = false;
			_requestLandsLater = false;
			if (animate && GetLayout() is var (layoutManager, _) && layoutManager.ItemCount > 0)
			{
				// It scrolls like a fling toward the start, which never disengages the anchor.
				_tracker.Engage(ScrollAnchorMode.Start);
				var requests = _scrollRequests;
				Publish();
				if (requests == _scrollRequests)
				{
					SmoothScrollToStart(_recyclerView, layoutManager);
				}

				return;
			}

			// Look again as for a new view that follows its start.
			_recyclerView.StopScroll();
			_tracker.Reset(ScrollAnchorMode.Start);
			PublishAndJumpToEdge();
		}

		/// <summary>Scrolls to the end, and follows it again. The view must be able to follow its end.</summary>
		public void ScrollToEnd(bool animate)
		{
			if (_detached)
			{
				return;
			}

			// It's newer than any scroll requested before.
			_scrollRequests++;
			_requestPending = false;
			_requestLandsLater = false;
			if (animate && GetLayout() is var (layoutManager, _) && layoutManager.ItemCount > 0)
			{
				// It scrolls like a fling toward the end, which never disengages the anchor; once it settles, the anchor
				// scrolls the rest of the way if the end moved meanwhile.
				_tracker.Engage(ScrollAnchorMode.End);
				var requests = _scrollRequests;
				Publish();
				if (requests == _scrollRequests)
				{
					SmoothScrollToEnd(_recyclerView, layoutManager);
				}

				return;
			}

			// Look again as for a new view: the look after the jump scrolls to the exact end.
			_recyclerView.StopScroll();
			_tracker.Reset(ScrollAnchorMode.End);
			PublishAndJumpToEdge();
		}

		/// <summary>Stops anchoring: the view no longer follows an edge.</summary>
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

		void Update(bool scrolled = false)
		{
			if (_updating || _detached)
			{
				return;
			}

			var isVertical = _recyclerView.GetLayoutManager()?.CanScrollVertically() == true;
			if (isVertical != _wasVertical)
			{
				// A new layout that scrolls vertically again starts over, as a new view does.
				_wasVertical = isVertical;
				_tracker.Reset();
				ResetMeasurement();
			}

			if (isVertical && ReadMetrics() is ScrollAnchorMetrics metrics)
			{
				// The look after the layout that applied item changes counts the offset change as the layout's. A look after a
				// scroll doesn't: items changing as the user flings would otherwise keep a fling from ever moving away.
				var itemsChanged = !scrolled && _itemsChanged && !_recyclerView.HasPendingAdapterUpdates;
				if (itemsChanged)
				{
					_itemsChanged = false;
				}

				_updating = true;
				try
				{
					if (_tracker.Update(metrics, IsUserScrolling, sizeChanged: itemsChanged, scrolled: scrolled) is not null)
					{
						CorrectToEdge();
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
			var isAnchored = IsFollowingEdge;
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
			_toStart = null;
			_toEnd = null;
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
				_toStart = -exact;
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
			if (lastLaidOut)
			{
				_toEnd = end - helper.EndAfterPadding;
			}

			double distance = _toEnd ?? Math.Max(
				_recyclerView.ComputeVerticalScrollRange() - estimatedOffset - _recyclerView.ComputeVerticalScrollExtent(),
				viewport);

			var offset = _offset / _density;
			var viewportUnits = viewport / (double)_density;
			return new ScrollAnchorMetrics(offset, offset + viewportUnits + distance / _density, viewportUnits);
		}

		// Scrolls to the edge the view follows, as the look that asked for it measured it: nothing laid out or scrolled since.
		void CorrectToEdge()
		{
			var toEnd = _tracker.AnchoredEdge == ScrollAnchorMode.End;

			// Past the end of the viewport for the end; above its start, so negative, for the start.
			if ((toEnd ? _toEnd : _toStart) is int distance)
			{
				if (toEnd ? distance > 0 : distance < 0)
				{
					_recyclerView.ScrollBy(0, distance);
				}

				return;
			}

			// Lay the row at the edge out first; the look after that layout scrolls the rest of the way.
			JumpToEdge();
		}

		/// <summary>Starts an animated scroll that ends with the start of the first row at the start of the viewport.</summary>
		internal static void SmoothScrollToStart(RecyclerView recyclerView, RecyclerView.LayoutManager layoutManager) =>
			layoutManager.StartSmoothScroll(new PositionalSmoothScroller(recyclerView.Context, ScrollToPosition.Start)
			{
				TargetPosition = 0
			});

		/// <summary>
		/// Starts an animated scroll that ends with the end of the last row at the end of the viewport, also when that row is
		/// taller than the viewport and already in view, where <c>SmoothScrollToPosition</c> snaps the nearer edge and can
		/// scroll back to the row's start.
		/// </summary>
		internal static void SmoothScrollToEnd(RecyclerView recyclerView, RecyclerView.LayoutManager layoutManager) =>
			layoutManager.StartSmoothScroll(new PositionalSmoothScroller(recyclerView.Context, ScrollToPosition.End)
			{
				TargetPosition = layoutManager.ItemCount - 1
			});

		/// <summary>Gets how far, in pixels, the end of the lowest row laid out is past the end of the viewport.</summary>
		/// <remarks>The lowest row ends the content once the last one is laid out: in a grid the last item can sit beside a taller one.</remarks>
		internal static int GetDistanceToEnd(RecyclerView.LayoutManager layoutManager, OrientationHelper helper)
		{
			var end = int.MinValue;
			var childCount = layoutManager.ChildCount;
			for (var index = 0; index < childCount; index++)
			{
				if (layoutManager.GetChildAt(index) is { } child)
				{
					end = Math.Max(end, helper.GetDecoratedEnd(child));
				}
			}

			return end - helper.EndAfterPadding;
		}

		// Lays the row at the edge the view follows out at that edge in the next layout, so a view that starts at its end
		// shows it from its first frame.
		void JumpToEdge()
		{
			if (!IsFollowingEdge || IsUserScrolling || _recyclerView.GetLayoutManager() is not { ItemCount: > 0 } layoutManager)
			{
				return;
			}

			// Scrolling to a position asks for a layout, which is lost if asked for during one: ask once it's over.
			if (_layoutDepth > 0 || _finishingLayout || _recyclerView.IsInLayout)
			{
				PostJumpToEdge();
				return;
			}

			_recyclerView.ScrollToPosition(_tracker.AnchoredEdge == ScrollAnchorMode.End ? layoutManager.ItemCount - 1 : 0);
		}

		void PostUpdate() => Post(ref _updatePosted);

		void PostJumpToEdge() => Post(ref _jumpPosted);

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

			// The layout a requested scroll makes looks next, and judges it.
			if (_detached || _requestPending)
			{
				return;
			}

			// The look that asked for a jump already counted on the view being at the edge: get it there, and the layout
			// that follows looks again.
			if (jump)
			{
				JumpToEdge();
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
