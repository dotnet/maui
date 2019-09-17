using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform.Android.CollectionView
{
	public class RecyclerViewScrollListener<TItemsView, TItemsViewSource> : RecyclerView.OnScrollListener 
		where TItemsView : ItemsView
		where TItemsViewSource : IItemsViewSource
	{
		bool _disposed;
		int _horizontalOffset, _verticalOffset;
		TItemsView _itemsView;
		ItemsViewAdapter<TItemsView, TItemsViewSource> _itemsViewAdapter;

		public RecyclerViewScrollListener(TItemsView itemsView, ItemsViewAdapter<TItemsView, TItemsViewSource> itemsViewAdapter)
		{
			_itemsView = itemsView;
			_itemsViewAdapter = itemsViewAdapter;
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

			var firstVisibleItemIndex = -1;
			var lastVisibleItemIndex = -1;
			var centerItemIndex = -1;

			if (recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager)
			{
				firstVisibleItemIndex = linearLayoutManager.FindFirstVisibleItemPosition();
				lastVisibleItemIndex = linearLayoutManager.FindLastVisibleItemPosition();
				centerItemIndex = CalculateCenterItemIndex(firstVisibleItemIndex, lastVisibleItemIndex, linearLayoutManager);
			}

			var context = recyclerView.Context;
			var itemsViewScrolledEventArgs = new ItemsViewScrolledEventArgs
			{
				HorizontalDelta = context.FromPixels(dx),
				VerticalDelta = context.FromPixels(dy),
				HorizontalOffset = context.FromPixels(_horizontalOffset),
				VerticalOffset = context.FromPixels(_verticalOffset),
				FirstVisibleItemIndex = firstVisibleItemIndex,
				CenterItemIndex = centerItemIndex,
				LastVisibleItemIndex = lastVisibleItemIndex
			};

			_itemsView.SendScrolled(itemsViewScrolledEventArgs);

			// Don't send RemainingItemsThresholdReached event for non-linear layout managers
			// This can also happen if a layout pass has not happened yet
			if (lastVisibleItemIndex == -1)
				return;

			switch (_itemsView.RemainingItemsThreshold)
			{
				case -1:
					return;
				case 0:
					if (lastVisibleItemIndex == _itemsViewAdapter.ItemCount - 1)
						_itemsView.SendRemainingItemsThresholdReached();
					break;
				default:
					if (_itemsViewAdapter.ItemCount - 1 - lastVisibleItemIndex <= _itemsView.RemainingItemsThreshold)
						_itemsView.SendRemainingItemsThresholdReached();
					break;
			}
		}

		static int CalculateCenterItemIndex(int firstVisibleItemIndex, int lastVisibleItemIndex, LinearLayoutManager linearLayoutManager)
		{
			// This can happen if a layout pass has not happened yet
			if (firstVisibleItemIndex == -1)
				return firstVisibleItemIndex;

			var keyValuePairs = new Dictionary<int, int>();
			for (var i = firstVisibleItemIndex; i <= lastVisibleItemIndex; i++)
			{
				var view = linearLayoutManager.FindViewByPosition(i);
				var rect = new Rect();

				view.GetLocalVisibleRect(rect);
				keyValuePairs[i] = rect.Height();
			}

			var center = keyValuePairs.Values.Sum() / 2.0;
			foreach (var keyValuePair in keyValuePairs)
			{
				center -= keyValuePair.Value;

				if (center <= 0)
					return keyValuePair.Key;
			}

			return firstVisibleItemIndex;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_itemsView = null;
				_itemsViewAdapter = null;
			}

			_disposed = true;

			base.Dispose(disposing);
		}
	}
}