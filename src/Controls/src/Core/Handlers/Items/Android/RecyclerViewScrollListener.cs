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
				_itemsView.SendRemainingItemsThresholdReached();
			}
		}

		protected virtual (int First, int Center, int Last) GetVisibleItemsIndex(RecyclerView recyclerView)
		{
			int firstVisibleItemIndex = -1, lastVisibleItemIndex = -1, centerItemIndex = -1;

			if (recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager)
			{
				firstVisibleItemIndex = linearLayoutManager.FindFirstVisibleItemPosition();
				lastVisibleItemIndex = linearLayoutManager.FindLastVisibleItemPosition();
				centerItemIndex = recyclerView.CalculateCenterItemIndex(firstVisibleItemIndex, linearLayoutManager, _getCenteredItemOnXAndY);
			}

			var adapter = ItemsViewAdapter;
			var itemsSource = adapter.ItemsSource;
			int itemsCount = adapter.ItemCount;
			bool hasHeader = itemsSource.HasHeader;
			bool hasFooter = itemsSource.HasFooter;

			if (itemsSource is not UngroupedItemsSource && itemsSource is IGroupableItemsViewSource groupable)
			{
				return (
				 AdjustGroupIndex(groupable, firstVisibleItemIndex, hasHeader, hasFooter, itemsCount, snapForward: true),
				 AdjustGroupIndex(groupable, centerItemIndex, hasHeader, hasFooter, itemsCount, snapForward: true),
				 AdjustGroupIndex(groupable, lastVisibleItemIndex, hasHeader, hasFooter, itemsCount, snapForward: false)
				);
			}

			// Adjust for footer: if the last visible item is the footer, decrement to get the last data item index
			if (hasFooter && lastVisibleItemIndex == itemsCount - 1)
			{
				lastVisibleItemIndex--;
			}

			// Non-grouped items adjustment
			if (hasHeader)
			{
				firstVisibleItemIndex--;
				lastVisibleItemIndex--;
				centerItemIndex--;
			}

			int maxValidIndex = Math.Max(0, itemsSource.Count - 1);
			firstVisibleItemIndex = Math.Clamp(firstVisibleItemIndex, 0, maxValidIndex);
			lastVisibleItemIndex = Math.Clamp(lastVisibleItemIndex, 0, maxValidIndex);
			centerItemIndex = Math.Clamp(centerItemIndex, 0, maxValidIndex);

			return (firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
		}

		/// <param name="snapForward">
		/// When the adapter position falls on a group header or group footer,
		/// true  = snap to the first data item in the following group (use for FirstVisible/Center),
		/// false = snap to the last data item in the preceding group (use for LastVisible).
		/// </param>
		static int AdjustGroupIndex(IGroupableItemsViewSource source, int position, bool hasHeader, bool hasFooter, int count, bool snapForward)
		{
			if (position < 0)
			{
				return 0;
			}

			if (position >= count)
			{
				return Math.Max(0, GetGroupedDataCount(source) - 1);
			}

			int dataIndex = 0, currentItem = hasHeader ? 1 : 0;

			// Iterate through items until we reach the target position
			while (currentItem <= position && currentItem < count)
			{
				if (hasFooter && currentItem == count - 1)
				{
					break;
				}

				bool isHeader = source.IsGroupHeader(currentItem), isFooter = source.IsGroupFooter(currentItem);

				// If current item is a normal data item (not header/footer)
				if (!isHeader && !isFooter)
				{
					if (currentItem == position)
					{
						return dataIndex;
					}

					dataIndex++;
				}
				// If position is a group header/footer, find the nearest data item
				else if (currentItem == position)
				{
					return snapForward
					 ? FindNextDataIndex(source, currentItem, hasFooter, count, dataIndex)
					 : FindPrevDataIndex(source, currentItem, hasHeader);
				}

				currentItem++;
			}

			// If we reach here, pos was beyond the last item
			// Return the last valid data index (or 0 if empty)
			return Math.Max(0, dataIndex - 1);
		}

		static int GetGroupedDataCount(IGroupableItemsViewSource source)
		{
			// Count data items only (excluding all headers and footers)
			int dataCount = 0;
			for (int index = 0; index < source.Count; index++)
			{
				if (!source.IsGroupHeader(index) && !source.IsGroupFooter(index) &&
					!source.IsHeader(index) && !source.IsFooter(index))
				{
					dataCount++;
				}
			}

			return dataCount;
		}

		// dataIndex: the 0-based data item index to assign to the next valid item found.
		// Returned without incrementing because the item following a header/footer inherits this index.
		static int FindNextDataIndex(IGroupableItemsViewSource source, int start, bool hasFooter, int count, int dataIndex)
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
					return dataIndex;
				}
			}

			// If no valid data item found ahead, return the previous data index
			// (or 0 if no valid items exist)
			return Math.Max(0, dataIndex - 1);
		}

		static int FindPrevDataIndex(IGroupableItemsViewSource source, int start, bool hasHeader)
		{
			int lastValid = -1;
			int currentItem = hasHeader ? 1 : 0;

			for (; currentItem < start; currentItem++)
			{
				// Increment counter only for data items (not headers/footers)
				// to get accurate position for last visible item index
				if (!source.IsGroupHeader(currentItem) && !source.IsGroupFooter(currentItem))
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
			}

			_disposed = true;

			base.Dispose(disposing);
		}
	}
}
