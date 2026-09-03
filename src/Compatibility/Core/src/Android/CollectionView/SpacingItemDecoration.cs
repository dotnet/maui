using System;
using AndroidX.RecyclerView.Widget;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class SpacingItemDecoration : RecyclerView.ItemDecoration
	{
		readonly ItemsLayoutOrientation _orientation;
		readonly double _verticalSpacing;
		int _adjustedVerticalSpacing = -1;
		readonly double _horizontalSpacing;
		int _adjustedHorizontalSpacing = -1;
		readonly int _spanCount;

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
					_spanCount = gridItemsLayout.Span;
					break;
				case LinearItemsLayout listItemsLayout:
					_orientation = listItemsLayout.Orientation;
					if (_orientation == ItemsLayoutOrientation.Horizontal)
						_horizontalSpacing = listItemsLayout.ItemSpacing;
					else
						_verticalSpacing = listItemsLayout.ItemSpacing;
					_spanCount = 1;
					break;
			}
		}

		public override void GetItemOffsets(ARect outRect, AView view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets(outRect, view, parent, state);

			if (_adjustedVerticalSpacing == -1)
			{
				_adjustedVerticalSpacing = (int)parent.Context.ToPixels(_verticalSpacing);
			}

			if (_adjustedHorizontalSpacing == -1)
			{
				_adjustedHorizontalSpacing = (int)parent.Context.ToPixels(_horizontalSpacing);
			}

			var itemViewType = parent.GetChildViewHolder(view).ItemViewType;

			if (itemViewType == ItemViewType.Header)
			{
				if (_orientation == ItemsLayoutOrientation.Vertical)
				{
					outRect.Bottom = _adjustedVerticalSpacing;
				}
				else
				{
					outRect.Right = _adjustedHorizontalSpacing;
				}

				return;
			}

			if (itemViewType == ItemViewType.Footer)
			{
				return;
			}

			var spanIndex = 0;

			if (view.LayoutParameters is GridLayoutManager.LayoutParams gridLayoutParameters)
			{
				spanIndex = gridLayoutParameters.SpanIndex;
			}

			var spanGroupIndex = GetSpanGroupIndex(view, parent);

			if (_orientation == ItemsLayoutOrientation.Vertical)
			{
				outRect.Left = spanIndex == 0 ? 0 : _adjustedHorizontalSpacing;
				outRect.Top = spanGroupIndex == 0 ? 0 : _adjustedVerticalSpacing;
			}

			if (_orientation == ItemsLayoutOrientation.Horizontal)
			{
				outRect.Top = spanIndex == 0 ? 0 : _adjustedVerticalSpacing;
				outRect.Left = spanGroupIndex == 0 ? 0 : _adjustedHorizontalSpacing;
			}
		}

		int GetSpanGroupIndex(AView view, RecyclerView parent)
		{
			var position = parent.GetChildAdapterPosition(view);

			if (_spanCount > 1)
			{
				if (parent.GetLayoutManager() is GridLayoutManager gridLayoutManager)
				{
					return gridLayoutManager.GetSpanSizeLookup().GetSpanGroupIndex(position, _spanCount);
				}
			}

			return position;
		}
	}
}