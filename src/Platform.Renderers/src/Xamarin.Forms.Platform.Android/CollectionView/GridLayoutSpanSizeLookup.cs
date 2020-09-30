using AndroidX.RecyclerView.Widget;

namespace Xamarin.Forms.Platform.Android
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
			var itemViewType = _recyclerView.GetAdapter().GetItemViewType(position);

			if (itemViewType == ItemViewType.Header || itemViewType == ItemViewType.Footer
				|| itemViewType == ItemViewType.GroupHeader || itemViewType == ItemViewType.GroupFooter)
			{
				return _gridItemsLayout.Span;
			}

			return 1;
		}
	}
}