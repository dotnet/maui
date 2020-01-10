#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif

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