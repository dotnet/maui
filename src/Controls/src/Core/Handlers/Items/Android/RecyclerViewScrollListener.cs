#nullable disable
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

			var itemCount = recyclerView.GetAdapter()?.ItemCount ?? 0;
			_horizontalOffset = itemCount == 0 ? 0 : _horizontalOffset + dx;
			_verticalOffset = itemCount == 0 ? 0 : _verticalOffset + dy;

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
			var firstVisibleItemIndex = -1;
			var lastVisibleItemIndex = -1;
			var centerItemIndex = -1;

			if (recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager)
			{
				firstVisibleItemIndex = linearLayoutManager.FindFirstVisibleItemPosition();
				lastVisibleItemIndex = linearLayoutManager.FindLastVisibleItemPosition();
				centerItemIndex = recyclerView.CalculateCenterItemIndex(firstVisibleItemIndex, linearLayoutManager, _getCenteredItemOnXAndY);
			}

			bool hasHeader = ItemsViewAdapter.ItemsSource.HasHeader;
			bool hasFooter = ItemsViewAdapter.ItemsSource.HasFooter;
			int itemsCount = ItemsViewAdapter.ItemCount;

			if (!hasHeader && !hasFooter)
			{
				return (firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
			}

			if (firstVisibleItemIndex == 0 && lastVisibleItemIndex == itemsCount - 1)
			{
				lastVisibleItemIndex -= hasHeader && hasFooter ? 2 : 1;
			}
			else
			{
				if (hasHeader && !hasFooter)
				{
					lastVisibleItemIndex -= 1;
					firstVisibleItemIndex -= 1;
				}
				else if (!hasHeader && hasFooter)
				{
					if (lastVisibleItemIndex == itemsCount - 1)
					{
						lastVisibleItemIndex -= 1;
					}
				}
				else if (hasHeader && hasFooter)
				{
					if (firstVisibleItemIndex == 0)
					{
						lastVisibleItemIndex -= 1;
					}
					else if (lastVisibleItemIndex != itemsCount - 1)
					{
						firstVisibleItemIndex -= 1;
						lastVisibleItemIndex -= 1;
					}
					else
					{
						firstVisibleItemIndex -= 1;
						lastVisibleItemIndex -= 2;
					}
				}
			}

			if (firstVisibleItemIndex < 0)
			{
				firstVisibleItemIndex = 0;
			}

			if (lastVisibleItemIndex < 0)
			{
				lastVisibleItemIndex = 0;
			}

			return (firstVisibleItemIndex, centerItemIndex, lastVisibleItemIndex);
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
