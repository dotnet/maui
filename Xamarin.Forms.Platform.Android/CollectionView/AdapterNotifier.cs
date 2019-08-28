using Android.Support.V7.Widget;

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
			_adapter.NotifyDataSetChanged();
		}

		public void NotifyItemChanged(IItemsViewSource source, int startIndex)
		{
			_adapter.NotifyItemChanged(startIndex);
		}

		public void NotifyItemInserted(IItemsViewSource source, int startIndex)
		{
			_adapter.NotifyItemInserted(startIndex);
		}

		public void NotifyItemMoved(IItemsViewSource source, int fromPosition, int toPosition)
		{
			_adapter.NotifyItemMoved(fromPosition, toPosition);
		}

		public void NotifyItemRangeChanged(IItemsViewSource source, int start, int end)
		{
			_adapter.NotifyItemRangeChanged(start, end);
		}

		public void NotifyItemRangeInserted(IItemsViewSource source, int startIndex, int count)
		{
			_adapter.NotifyItemRangeInserted(startIndex, count);
		}

		public void NotifyItemRangeRemoved(IItemsViewSource source, int startIndex, int count)
		{
			_adapter.NotifyItemRangeRemoved(startIndex, count);
		}

		public void NotifyItemRemoved(IItemsViewSource source, int startIndex)
		{
			_adapter.NotifyItemRemoved(startIndex);
		}
	}
}