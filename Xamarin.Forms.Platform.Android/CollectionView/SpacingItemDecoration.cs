using System;
using Android.Graphics;
#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class SpacingItemDecoration : RecyclerView.ItemDecoration
	{
		readonly ItemsLayoutOrientation _orientation;
		readonly double _verticalSpacing;
		double _adjustedVerticalSpacing = -1;
		readonly double _horizontalSpacing;
		double _adjustedHorizontalSpacing = -1;

		public SpacingItemDecoration(IItemsLayout itemsLayout)
		{
			if (itemsLayout == null)
			{
				throw new ArgumentNullException(nameof(itemsLayout));
			}

			switch (itemsLayout)
			{
				case GridItemsLayout gridItemsLayout:
					_orientation = gridItemsLayout.Orientation;
					_horizontalSpacing = gridItemsLayout.HorizontalItemSpacing;
					_verticalSpacing = gridItemsLayout.VerticalItemSpacing;
					break;
				case LinearItemsLayout listItemsLayout:
					_orientation = listItemsLayout.Orientation;
					if (_orientation == ItemsLayoutOrientation.Horizontal)
						_horizontalSpacing = listItemsLayout.ItemSpacing;
					else
						_verticalSpacing = listItemsLayout.ItemSpacing;
					break;
			}
		}

		public override void GetItemOffsets(Rect outRect, AView view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets(outRect, view, parent, state);

			if (_adjustedVerticalSpacing == -1)
			{
				_adjustedVerticalSpacing = parent.Context.ToPixels(_verticalSpacing);
			}

			if (_adjustedHorizontalSpacing == -1)
			{
				_adjustedHorizontalSpacing = parent.Context.ToPixels(_horizontalSpacing);
			}

			var itemViewType = parent.GetChildViewHolder(view).ItemViewType;

			if (itemViewType == ItemViewType.Header)
			{
				outRect.Bottom = (int)_adjustedVerticalSpacing;
				return;
			}

			if (itemViewType == ItemViewType.Footer)
			{
				return;
			}

			var spanIndex = 0;

			if(view.LayoutParameters is GridLayoutManager.LayoutParams gridLayoutParameters)
			{
				spanIndex = gridLayoutParameters.SpanIndex;
			}

			if (_orientation == ItemsLayoutOrientation.Vertical)
			{
				outRect.Left = spanIndex == 0 ? 0 : (int)_adjustedHorizontalSpacing;

				if (parent.GetChildAdapterPosition(view) != parent.GetAdapter().ItemCount - 1)
					outRect.Bottom = (int)_adjustedVerticalSpacing;
			}

			if (_orientation == ItemsLayoutOrientation.Horizontal)
			{
				outRect.Top = spanIndex == 0 ? 0 : (int)_adjustedVerticalSpacing;

				if (parent.GetChildAdapterPosition(view) != parent.GetAdapter().ItemCount - 1)
					outRect.Right = (int)_adjustedHorizontalSpacing;
			}
		}
	}
}