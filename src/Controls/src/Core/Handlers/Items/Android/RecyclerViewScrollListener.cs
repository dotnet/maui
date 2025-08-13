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
		}

		public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			base.OnScrolled(recyclerView, dx, dy);

			// TODO: These offsets will be incorrect upon row size or count change.
			// They are currently provided in place of LayoutManager's default offset calculation
			// because it does not report accurate values in the presence of uneven rows.
			// See https://stackoverflow.com/questions/27507715/android-how-to-get-the-current-x-offset-of-recyclerview
			_horizontalOffset += dx;
			_verticalOffset += dy;

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
			if (Last == -1)
				return;

			switch (_itemsView.RemainingItemsThreshold)
			{
				case -1:
					return;
				case 0:
					if (Last == ItemsViewAdapter.ItemsSource.Count - 1)
						_itemsView.SendRemainingItemsThresholdReached();
					break;
				default:
					if (ItemsViewAdapter.ItemsSource.Count - 1 - Last <= _itemsView.RemainingItemsThreshold)
						_itemsView.SendRemainingItemsThresholdReached();
					break;
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
				 AdjustGroupIndex(groupable, firstVisibleItemIndex, hasHeader, hasFooter, itemsCount, true),
				 AdjustGroupIndex(groupable, centerItemIndex, hasHeader, hasFooter, itemsCount, true),
				 AdjustGroupIndex(groupable, lastVisibleItemIndex, hasHeader, hasFooter, itemsCount, false)
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

			firstVisibleItemIndex = Math.Max(0, firstVisibleItemIndex);
			lastVisibleItemIndex = Math.Max(0, lastVisibleItemIndex);

			return (firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
		}

		static int AdjustGroupIndex(IGroupableItemsViewSource source, int position, bool hasHeader, bool hasFooter, int count, bool isStart)
		{
			if (position < 0)
			{
				return 0;
			}

			// Adjust for header if present
			position = position - (hasHeader && position > 0 ? 1 : 0);

			int dataIndex = hasHeader ? 1 : 0, currentItem = hasHeader ? 1 : 0;

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
					return isStart
					 ? FindNextDataIndex(source, currentItem, hasFooter, count, dataIndex)
					 : FindPrevDataIndex(source, currentItem, hasHeader);
				}

				currentItem++;
			}

			// If we reach here, pos was beyond the last item
			// Return the last valid data index (or 0 if empty)
			return Math.Max(0, dataIndex - 1);
		}

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
