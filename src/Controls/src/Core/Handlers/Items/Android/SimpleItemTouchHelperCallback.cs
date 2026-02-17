#nullable disable
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class SimpleItemTouchHelperCallback : ItemTouchHelper.Callback
	{
		IItemTouchHelperAdapter _adapter;

		public override bool IsLongPressDragEnabled => true;

		public SimpleItemTouchHelperCallback()
		{
		}

		public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
		{
			var itemViewType = viewHolder.ItemViewType;
			if (itemViewType == ItemViewType.Header || itemViewType == ItemViewType.Footer
				|| itemViewType == ItemViewType.GroupHeader || itemViewType == ItemViewType.GroupFooter)
			{
				return MakeMovementFlags(0, 0);
			}

			var dragFlags = ItemTouchHelper.Up | ItemTouchHelper.Down | ItemTouchHelper.Left | ItemTouchHelper.Right;
			return MakeMovementFlags(dragFlags, 0);
		}

		public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
		{
			// Block reordering onto structural elements (Header, Footer, GroupHeader, GroupFooter).
			// Dragging FROM structural elements is already prevented by GetMovementFlags returning 0.
			// All other items (including those with different DataTemplateSelector view types) can be freely reordered.
			var targetViewType = target.ItemViewType;
			if (targetViewType == ItemViewType.Header || targetViewType == ItemViewType.Footer
				|| targetViewType == ItemViewType.GroupHeader || targetViewType == ItemViewType.GroupFooter)
			{
				return false;
			}

			return _adapter.OnItemMove(viewHolder.BindingAdapterPosition, target.BindingAdapterPosition);
		}

		public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
		{
		}

		public void SetAdapter(IItemTouchHelperAdapter adapter)
		{
			_adapter = adapter;
		}
	}
}