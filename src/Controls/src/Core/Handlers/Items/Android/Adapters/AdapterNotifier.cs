#nullable disable
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class AdapterNotifier : ICollectionChangedNotifier, ICollectionChangedNotifierWithCleanup
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

		internal void NotifyDataSetChangedAndUnbind()
		{
			if (IsValidAdapter())
			{
				if (_adapter is IItemsViewAdapter itemsViewAdapter)
				{
					itemsViewAdapter.UnbindAllItems();
				}

				_adapter.NotifyDataSetChanged();
			}
		}

		public void NotifyItemChanged(IItemsViewSource source, int startIndex)
		{
			if (IsValidAdapter())
				_adapter.NotifyItemChanged(startIndex);
		}

		public void NotifyItemInserted(IItemsViewSource source, int startIndex)
		{
			if (IsValidAdapter())
			{
				_adapter.NotifyItemInserted(startIndex);
			}
		}

		public void NotifyItemMoved(IItemsViewSource source, int fromPosition, int toPosition)
		{
			if (IsValidAdapter())
			{
				_adapter.NotifyItemMoved(fromPosition, toPosition);
			}
		}

		public void NotifyItemRangeChanged(IItemsViewSource source, int start, int end)
		{
			if (IsValidAdapter())
				_adapter.NotifyItemRangeChanged(start, end);
		}

		public void NotifyItemRangeInserted(IItemsViewSource source, int startIndex, int count)
		{
			if (IsValidAdapter())
			{
				_adapter.NotifyItemRangeInserted(startIndex, count);
			}
		}

		public void NotifyItemRangeRemoved(IItemsViewSource source, int startIndex, int count)
		{
			NotifyItemRangeRemoved(startIndex, count, unbind: false);
		}

		void ICollectionChangedNotifierWithCleanup.NotifyItemRangeRemovedAndUnbind(IItemsViewSource source, int startIndex, int count)
		{
			NotifyItemRangeRemoved(startIndex, count, unbind: true);
		}

		void NotifyItemRangeRemoved(int startIndex, int count, bool unbind)
		{
			if (IsValidAdapter())
			{
				if (unbind && _adapter is IItemsViewAdapter itemsViewAdapter)
				{
					itemsViewAdapter.UnbindItemRange(startIndex, count);
				}

				_adapter.NotifyItemRangeRemoved(startIndex, count);
			}
		}

		public void NotifyItemRemoved(IItemsViewSource source, int startIndex)
		{
			if (IsValidAdapter())
			{
				_adapter.NotifyItemRemoved(startIndex);
			}
		}

		internal bool IsValidAdapter()
		{
			if (_adapter == null || _adapter.IsDisposed())
				return false;

			return true;
		}
	}
}
