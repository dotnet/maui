#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif

namespace Xamarin.Forms.Platform.Android
{
	// Passes change notifications directly through to a RecyclerView.Adapter
	internal class AdapterNotifier : ICollectionChangedNotifier
	{
		readonly RecyclerView.Adapter _adapter;

		public AdapterNotifier(RecyclerView.Adapter adapter)
		{
			_adapter = adapter;
		}

		public void NotifyDataSetChanged()
		{
			if (IsValidAdapter())
				_adapter.NotifyDataSetChanged();
		}

		public void NotifyItemChanged(IItemsViewSource source, int startIndex)
		{
			if (IsValidAdapter())
				_adapter.NotifyItemChanged(startIndex);
		}

		public void NotifyItemInserted(IItemsViewSource source, int startIndex)
		{
			if (IsValidAdapter())
				_adapter.NotifyItemInserted(startIndex);
		}

		public void NotifyItemMoved(IItemsViewSource source, int fromPosition, int toPosition)
		{
			if (IsValidAdapter())
				_adapter.NotifyItemMoved(fromPosition, toPosition);
		}

		public void NotifyItemRangeChanged(IItemsViewSource source, int start, int end)
		{
			if (IsValidAdapter())
				_adapter.NotifyItemRangeChanged(start, end);
		}

		public void NotifyItemRangeInserted(IItemsViewSource source, int startIndex, int count)
		{
			if (IsValidAdapter())
				_adapter.NotifyItemRangeInserted(startIndex, count);
		}

		public void NotifyItemRangeRemoved(IItemsViewSource source, int startIndex, int count)
		{
			if (IsValidAdapter())
				_adapter.NotifyItemRangeRemoved(startIndex, count);
		}

		public void NotifyItemRemoved(IItemsViewSource source, int startIndex)
		{
			if (IsValidAdapter())
				_adapter.NotifyItemRemoved(startIndex);
		}

		internal bool IsValidAdapter()
		{
			if (_adapter == null || _adapter.IsDisposed())
				return false;

			return true;
		}
	}
}
