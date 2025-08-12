#nullable disable
using System;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class RecyclerViewScrollListener<TItemsView, TItemsViewSource> : RecyclerView.OnScrollListener
		where TItemsView : ItemsView
		where TItemsViewSource : IItemsViewSource
	{
		protected ItemsViewAdapter<TItemsView, TItemsViewSource> ItemsViewAdapter;
		bool _disposed;
		int _horizontalOffset, _verticalOffset;
		TItemsView _itemsView;
		bool _pendingRemainingItemsThresholdReached;
		readonly bool _getCenteredItemOnXAndY = false;
		bool _hasCompletedFirstLayout = false;

		public RecyclerViewScrollListener(TItemsView itemsView, ItemsViewAdapter<TItemsView, TItemsViewSource> itemsViewAdapter) : this(itemsView, itemsViewAdapter, false)
		{

		}

		public RecyclerViewScrollListener(TItemsView itemsView, ItemsViewAdapter<TItemsView, TItemsViewSource> itemsViewAdapter, bool getCenteredItemOnXAndY)
		{
			_itemsView = itemsView;
			UpdateAdapter(itemsViewAdapter);
			_getCenteredItemOnXAndY = getCenteredItemOnXAndY;
		}

		internal void UpdateAdapter(ItemsViewAdapter<TItemsView, TItemsViewSource> itemsViewAdapter)
		{
			ItemsViewAdapter = itemsViewAdapter;
			// Reset flag when adapter changes to handle ItemsSource updates
			_hasCompletedFirstLayout = false;
		}

		public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			base.OnScrolled(recyclerView, dx, dy);

			var itemCount = recyclerView.GetAdapter()?.ItemCount ?? 0;
			_horizontalOffset = itemCount == 0 ? 0 : _horizontalOffset + dx;
			_verticalOffset = itemCount == 0 ? 0 : _verticalOffset + dy;

			// Prevent the Scrolled event from firing on the very first layout callback only.
			// This is the initial OnScrolled(0,0) call when the view is first laid out.
			// After that, layout is marked as complete and all subsequent scroll events are allowed.
			if (!_hasCompletedFirstLayout && !recyclerView.IsLaidOut && dx == 0 && dy == 0)
			{
				return;
			}

			// Mark that first layout has been processed - all future scrolls should fire events
			_hasCompletedFirstLayout = true;

			var (First, Center, Last) = GetVisibleItemsIndex(recyclerView);
			var itemsViewScrolledEventArgs = new ItemsViewScrolledEventArgs
			{
				HorizontalDelta = recyclerView.FromPixels(dx),
				VerticalDelta = recyclerView.FromPixels(dy),
				HorizontalOffset = recyclerView.FromPixels(_horizontalOffset),
				VerticalOffset = recyclerView.FromPixels(_verticalOffset),
				FirstVisibleItemIndex = First,
				CenterItemIndex = Center,
				LastVisibleItemIndex = Last
			};

			_itemsView.SendScrolled(itemsViewScrolledEventArgs);

			// Don't send RemainingItemsThresholdReached event for non-linear layout managers
			// This can also happen if a layout pass has not happened yet
			if (Last == -1 || ItemsViewAdapter is null || _itemsView.RemainingItemsThreshold == -1)
			{
				return;
			}

			var itemsSource = ItemsViewAdapter.ItemsSource;
			int headerValue = itemsSource.HasHeader ? 1 : 0;
			int footerValue = itemsSource.HasFooter ? 1 : 0;

			// Calculate actual data item count (excluding header and footer positions)
			int actualItemCount = ItemsViewAdapter.ItemCount - footerValue - headerValue;

			// Ensure we're within the data items region (not in header/footer)
			if (Last < headerValue || Last > actualItemCount)
			{
				return;
			}

			// Check if we're at or within threshold distance from the last data item
			bool isThresholdReached = (Last == actualItemCount - 1) || (actualItemCount - 1 - Last <= _itemsView.RemainingItemsThreshold);

			if (isThresholdReached)
			{
				HandleRemainingItemsThresholdReached();
			}
		}

		public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
		{
			base.OnScrollStateChanged(recyclerView, newState);

			// If we have a pending threshold reached event and the RecyclerView is now idle,
			// it's safe to trigger the event without the risk of modifying the adapter during a scroll callback
			if (_pendingRemainingItemsThresholdReached && newState == RecyclerView.ScrollStateIdle)
			{
				_pendingRemainingItemsThresholdReached = false;
				if (!_disposed && _itemsView is not null)
				{
					_itemsView.SendRemainingItemsThresholdReached();
				}
			}
		}

		void HandleRemainingItemsThresholdReached()
		{
			// Mark that we need to trigger the threshold reached event
			// This will be handled when the RecyclerView transitions to idle state
			// to avoid the "Cannot call this method in a scroll callback" exception
			_pendingRemainingItemsThresholdReached = true;
		}

		protected virtual (int First, int Center, int Last) GetVisibleItemsIndex(RecyclerView recyclerView)
		{
			int first = -1, last = -1, center = -1;

			if (recyclerView.GetLayoutManager() is LinearLayoutManager lm)
			{
				first = lm.FindFirstVisibleItemPosition();
				last = lm.FindLastVisibleItemPosition();
				center = recyclerView.CalculateCenterItemIndex(first, lm, _getCenteredItemOnXAndY);
			}

			var adapter = ItemsViewAdapter;
			var itemsSource = adapter.ItemsSource;
			int count = adapter.ItemCount;
			bool hasHeader = itemsSource.HasHeader;
			bool hasFooter = itemsSource.HasFooter;

			if (itemsSource is IGroupableItemsViewSource groupable && itemsSource is not UngroupedItemsSource)
			{
				return (
				 AdjustGroupIndex(groupable, first, hasHeader, hasFooter, count, true),
				 AdjustGroupIndex(groupable, center, hasHeader, hasFooter, count, true),
				 AdjustGroupIndex(groupable, last, hasHeader, hasFooter, count, false)
				);
			}

			// Non-grouped items adjustment
			if (hasHeader)
			{
				first--;
				last--;
				center--;
			}

			if (hasFooter && last == count - 1)
			{
				last--;
			}

			first = Math.Max(0, first);
			last = Math.Max(0, last);

			return (first, center, last);
		}

		static int AdjustGroupIndex(IGroupableItemsViewSource source, int pos, bool hasHeader, bool hasFooter, int count, bool isStart)
		{
			if (pos < 0)
			{
				return 0;
			}

			// Adjust for header if present
			pos = pos - (hasHeader && pos > 0 ? 1 : 0);

			int dataIdx = hasHeader ? 1 : 0, curr = hasHeader ? 1 : 0;

			// Iterate through items until we reach the target position
			while (curr <= pos && curr < count)
			{
				if (hasFooter && curr == count - 1)
				{
					break;
				}

				bool isHeader = source.IsGroupHeader(curr), isFooter = source.IsGroupFooter(curr);

				// If current item is a normal data item (not header/footer)
				if (!isHeader && !isFooter)
				{
					if (curr == pos)
					{
						return dataIdx;
					}

					dataIdx++;
				}
				// If position is a group header/footer, find the nearest data item
				else if (curr == pos)
				{
					return isStart
					 ? FindNextDataIndex(source, curr, hasFooter, count, dataIdx)
					 : FindPrevDataIndex(source, curr, hasHeader);
				}
				curr++;
			}

			// If we reach here, pos was beyond the last item
			// Return the last valid data index (or 0 if empty)
			return Math.Max(0, dataIdx - 1);
		}

		static int FindNextDataIndex(IGroupableItemsViewSource source, int start, bool hasFooter, int count, int dataIdx)
		{
			for (int i = start + 1; i < count; i++)
			{
				// Skip footer item if present
				if (hasFooter && i == count - 1)
				{
					break;
				}

				// If we find a regular item (not a group header or footer),
				// return the current data index without incrementing
				if (!source.IsGroupHeader(i) && !source.IsGroupFooter(i))
				{
					return dataIdx;
				}
			}

			// If no valid data item found ahead, return the previous data index
			// (or 0 if no valid items exist)
			return Math.Max(0, dataIdx - 1);
		}

		static int FindPrevDataIndex(IGroupableItemsViewSource source, int start, bool hasHeader)
		{
			int lastValid = -1;
			int curr = hasHeader ? 1 : 0;

			for (; curr < start; curr++)
			{
				// Increment counter only for data items (not headers/footers)
				// to get accurate position for last visible item index
				if (!source.IsGroupHeader(curr) && !source.IsGroupFooter(curr))
				{
					lastValid++;
				}
			}

			// Return the last valid data item found (or 0 if none)
			return Math.Max(0, lastValid);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_itemsView = null;
				ItemsViewAdapter = null;
				_pendingRemainingItemsThresholdReached = false;
			}

			_disposed = true;

			base.Dispose(disposing);
		}
	}
}
