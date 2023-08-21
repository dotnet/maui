//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
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