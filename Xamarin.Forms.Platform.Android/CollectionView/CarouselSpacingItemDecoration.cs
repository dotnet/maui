using System;
using Android.Graphics;
using Android.Support.V7.Widget;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class CarouselSpacingItemDecoration : RecyclerView.ItemDecoration
	{
		readonly ItemsLayoutOrientation _orientation;
		readonly double _verticalSpacing;
		double _adjustedVerticalSpacing = -1;
		readonly double _horizontalSpacing;
		double _adjustedHorizontalSpacing = -1;

		public CarouselSpacingItemDecoration(IItemsLayout itemsLayout)
		{
			var layout = itemsLayout ?? throw new ArgumentNullException(nameof(itemsLayout));

			switch (layout)
			{
				case GridItemsLayout gridItemsLayout:
					_orientation = gridItemsLayout.Orientation;
					_horizontalSpacing = gridItemsLayout.HorizontalItemSpacing;
					_verticalSpacing = gridItemsLayout.VerticalItemSpacing;
					break;
				case ListItemsLayout listItemsLayout:
					_orientation = listItemsLayout.Orientation;
					if (_orientation == ItemsLayoutOrientation.Horizontal)
						_horizontalSpacing = listItemsLayout.ItemSpacing;
					else
						_verticalSpacing = listItemsLayout.ItemSpacing;
					break;
			}
		}

		public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state)
		{
			base.OnDraw(c, parent, state);
		}

		public override void GetItemOffsets(Rect outRect, AView view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets(outRect, view, parent, state);

			if (Math.Abs(_adjustedVerticalSpacing - (-1)) < double.Epsilon)
			{
				_adjustedVerticalSpacing = parent.Context.ToPixels(_verticalSpacing);
			}

			if (Math.Abs(_adjustedHorizontalSpacing - (-1)) < double.Epsilon)
			{
				_adjustedHorizontalSpacing = parent.Context.ToPixels(_horizontalSpacing);
			}

			int position = parent.GetChildAdapterPosition(view);
			int itemCount = state.ItemCount;

			if (position == RecyclerView.NoPosition || itemCount == 0)
				return;

			if (_orientation == ItemsLayoutOrientation.Vertical)
			{
				outRect.Left = position == 0 ? 0 : (int)_adjustedHorizontalSpacing;
				outRect.Bottom = (int)(_adjustedVerticalSpacing - (_verticalSpacing * 2));
				outRect.Top = (int)(_adjustedVerticalSpacing - (_verticalSpacing * 2));
			}

			if (_orientation == ItemsLayoutOrientation.Horizontal)
			{
				outRect.Top = position == 0 ? 0 : (int)_adjustedVerticalSpacing;
				outRect.Right = (int)(_adjustedHorizontalSpacing - (_horizontalSpacing * 2));
				outRect.Left = (int)(_adjustedHorizontalSpacing - (_horizontalSpacing * 2));
			}
		}
	}
}