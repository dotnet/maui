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

			if (itemsSource is IGroupableItemsViewSource groupable)
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

			int idx = pos - (hasHeader && pos > 0 ? 1 : 0);

			int dataIdx = 0, curr = hasHeader ? 1 : 0;
			while (curr <= pos && curr < count)
			{
				if (hasFooter && curr == count - 1)
				{
					break;
				}

				bool isHeader = source.IsGroupHeader(curr), isFooter = source.IsGroupFooter(curr);
				if (!isHeader && !isFooter)
				{
					if (curr == pos)
					{
						return dataIdx;
					}

					dataIdx++;
				}
				else if (curr == pos)
				{
					return isStart
					 ? FindNextDataIndex(source, curr, hasHeader, hasFooter, count, dataIdx)
					 : FindPrevDataIndex(source, curr, hasHeader, dataIdx);
				}
				curr++;
			}
			return Math.Max(0, dataIdx - 1);
		}

		static int FindNextDataIndex(IGroupableItemsViewSource source, int start, bool hasHeader, bool hasFooter, int count, int dataIdx)
		{
			for (int i = start + 1; i < count; i++)
			{
				if (hasFooter && i == count - 1)
				{
					break;
				}

				if (!source.IsGroupHeader(i) && !source.IsGroupFooter(i))
				{
					return dataIdx;
				}
			}
			return Math.Max(0, dataIdx - 1);
		}

		static int FindPrevDataIndex(IGroupableItemsViewSource source, int start, bool hasHeader, int dataIdx)
		{
			int lastValid = -1;
			int curr = hasHeader ? 1 : 0;
			for (; curr < start; curr++)
			{
				if (!source.IsGroupHeader(curr) && !source.IsGroupFooter(curr))
				{
					lastValid = dataIdx++;
				}
			}
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
