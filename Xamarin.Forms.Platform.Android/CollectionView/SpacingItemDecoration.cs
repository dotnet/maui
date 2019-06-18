using System;
using Android.Graphics;
using Android.Support.V7.Widget;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class SpacingItemDecoration : RecyclerView.ItemDecoration
	{
		ItemsLayoutOrientation _orientation;
		double _verticalSpacing;
		double _adjustedVerticalSpacing = -1;
		double _horizontalSpacing;
		double _adjustedHorizontalSpacing = -1;
		int _span = 1;

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
					_span = gridItemsLayout.Span;
					break;
				case ListItemsLayout listItemsLayout:
					_orientation = listItemsLayout.Orientation;
					if (_orientation == ItemsLayoutOrientation.Horizontal)
					{
						_horizontalSpacing = listItemsLayout.ItemSpacing;
					}
					else
					{
						_verticalSpacing = listItemsLayout.ItemSpacing;
					}
					break;
			}
		}

		public override void GetItemOffsets(Rect outRect, AView view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets(outRect, view, parent, state);

			var position = parent.GetChildAdapterPosition(view);

			if (position == 0)
			{
				return;
			}

			if (_adjustedVerticalSpacing == -1)
			{
				_adjustedVerticalSpacing = parent.Context.ToPixels(_verticalSpacing);
			}

			if (_adjustedHorizontalSpacing == -1)
			{
				_adjustedHorizontalSpacing  = parent.Context.ToPixels(_horizontalSpacing);
			}

			var firstInRow = false;
			var firstInCol = false;

			if (_orientation == ItemsLayoutOrientation.Vertical)
			{
				firstInRow = position >= _span && position % _span == 0;
				firstInCol = position < _span;
			}

			if (_orientation == ItemsLayoutOrientation.Horizontal)
			{
				firstInCol = position >= _span && position % _span == 0;
				firstInRow = position < _span;
			}

			outRect.Top = firstInCol ? 0 : (int)_adjustedVerticalSpacing;
			outRect.Left = firstInRow ? 0 : (int)_adjustedHorizontalSpacing;
		}
	}
}