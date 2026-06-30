#nullable disable
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class GridLayoutSpanSizeLookup : GridLayoutManager.SpanSizeLookup
	{
		readonly GridItemsLayout _gridItemsLayout;
		readonly RecyclerView _recyclerView;

		public GridLayoutSpanSizeLookup(GridItemsLayout gridItemsLayout, RecyclerView recyclerView)
		{
			_gridItemsLayout = gridItemsLayout;
			_recyclerView = recyclerView;
		}

		public override int GetSpanSize(int position)
		{
			var adapter = _recyclerView.GetAdapter();

			// EmptyViewAdapter uses private incrementing view type IDs that never match
			// the static ItemViewType constants. All items it contains (header, empty view,
			// footer) should span the full grid width.
			if (adapter is EmptyViewAdapter)
			{
				return _gridItemsLayout.Span;
			}

			var itemViewType = adapter.GetItemViewType(position);

			if (itemViewType == ItemViewType.Header || itemViewType == ItemViewType.Footer
				|| itemViewType == ItemViewType.GroupHeader || itemViewType == ItemViewType.GroupFooter)
			{
				return _gridItemsLayout.Span;
			}

			return 1;
		}
	}
}
