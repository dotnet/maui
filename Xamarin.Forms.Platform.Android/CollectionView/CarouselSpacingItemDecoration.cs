using System;
using Android.Graphics;
using Android.Support.V7.Widget;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class CarouselSpacingItemDecoration : RecyclerView.ItemDecoration
	{
		readonly IItemsLayout _itemsLayout;
		readonly Func<int> _getWidth;
		readonly Func<int> _getHeight;

		public CarouselSpacingItemDecoration(IItemsLayout itemsLayout, Func<int> getWidth, Func<int> getHeight)
		{
			_itemsLayout = itemsLayout;
			_getWidth = getWidth;
			_getHeight = getHeight;
		}

		public override void GetItemOffsets(Rect outRect, AView view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets(outRect, view, parent, state);

			int position = parent.GetChildAdapterPosition(view);
			int itemCount = state.ItemCount;
			int width = _getWidth();

			if (position == RecyclerView.NoPosition || itemCount == 0)
				return;

			// this is the first and last item , we need to give them some inset 
			if (position == 0)
				outRect.Left = width / itemCount;

			if (position == itemCount - 1)
				outRect.Right = width;

			if (position == itemCount)
				outRect.Left = width;
		}
	}
}