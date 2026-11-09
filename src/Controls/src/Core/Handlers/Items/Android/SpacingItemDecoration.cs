#nullable disable
using System;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class SpacingItemDecoration : RecyclerView.ItemDecoration
	{
		public int HorizontalOffset { get; }

		public int VerticalOffset { get; }

		ItemsLayoutOrientation _orientation;

		public SpacingItemDecoration(Context context, IItemsLayout itemsLayout)
		{
			// The original "SpacingItemDecoration" applied spacing based on an item's current span index.
			// It did not apply any spacing to items currently at span index 0 which can create an issue for us with grid layouts.
			// If one of those items at span index 0 were to move to another column, it would result in misaligned items.
			// It's better to just apply equal spacing to all items so we can avoid that issue (even the ones at span index 0).
			// The reason they didn't do this originally, I suspect, is that they didn't want spacing around the edge of the RecyclerView.
			// That however can be corrected by adjusting the padding on the RecyclerView which we are now doing.

			if (itemsLayout == null)
			{
				throw new ArgumentNullException(nameof(itemsLayout));
			}

			double horizontalOffset;
			double verticalOffset;

			switch (itemsLayout)
			{
				case GridItemsLayout gridItemsLayout:
					horizontalOffset = gridItemsLayout.HorizontalItemSpacing / 2.0;
					verticalOffset = gridItemsLayout.VerticalItemSpacing / 2.0;
					_orientation = gridItemsLayout.Orientation;
					break;
				case LinearItemsLayout listItemsLayout:
					if (listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
					{
						horizontalOffset = listItemsLayout.ItemSpacing / 2.0;
						verticalOffset = 0;
					}
					else
					{
						horizontalOffset = 0;
						verticalOffset = listItemsLayout.ItemSpacing / 2.0;
					}
					_orientation = listItemsLayout.Orientation;
					break;
				default:
					horizontalOffset = 0;
					verticalOffset = 0;
					_orientation = ItemsLayoutOrientation.Vertical;
					break;
			}

			HorizontalOffset = (int)context.ToPixels(horizontalOffset);
			VerticalOffset = (int)context.ToPixels(verticalOffset);
		}

		public override void GetItemOffsets(ARect outRect, AView view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets(outRect, view, parent, state);

			// Nothing to apply; skip the row computations below entirely.
			if (HorizontalOffset == 0 && VerticalOffset == 0)
			{
				return;
			}

			var adapter = parent.GetAdapter();
			if (adapter is null)
			{
				return;
			}

			int position = parent.GetChildAdapterPosition(view);
			int itemCount = adapter.ItemCount;

			if (position == RecyclerView.NoPosition || position < 0 || position >= itemCount)
			{
				return;
			}

			outRect.Left = HorizontalOffset;
			outRect.Right = HorizontalOffset;
			outRect.Bottom = VerticalOffset;
			outRect.Top = VerticalOffset;

			// Remove spacing on the outer edges so spacing only appears between items.
			bool isInFirstRowCol;
			bool isInLastRowCol;

			if (parent.GetLayoutManager() is GridLayoutManager gridLayoutManager)
			{
				// Full-span items (group headers, footers, etc.) are accounted for via the SpanSizeLookup.
				// Only the items that can share a row with `position` are inspected (at most spanCount of them);
				// SpanSizeLookup.GetSpanGroupIndex(itemCount - 1) would walk every position in the adapter
				// for every cell on every layout pass.
				var spanSizeLookup = gridLayoutManager.GetSpanSizeLookup();
				int spanCount = gridLayoutManager.SpanCount;
				isInFirstRowCol = IsInFirstSpanGroup(spanSizeLookup, position, spanCount);
				isInLastRowCol = IsInLastSpanGroup(spanSizeLookup, view, position, itemCount, spanCount);
			}
			else
			{
				// Linear layout: each item occupies exactly one row/column.
				isInFirstRowCol = position == 0;
				isInLastRowCol = position == itemCount - 1;
			}

			if (_orientation == ItemsLayoutOrientation.Vertical)
			{
				if (isInFirstRowCol)
					outRect.Top = 0;
				if (isInLastRowCol)
					outRect.Bottom = 0;
			}
			else
			{
				if (isInFirstRowCol)
					outRect.Left = 0;
				if (isInLastRowCol)
					outRect.Right = 0;
			}
		}

		static bool IsInFirstSpanGroup(GridLayoutManager.SpanSizeLookup spanSizeLookup, int position, int spanCount)
		{
			// Every item occupies at least one span, so the first row holds at most spanCount items.
			if (position >= spanCount)
			{
				return false;
			}

			int span = 0;

			for (int i = 0; i <= position; i++)
			{
				int spanSize = spanSizeLookup.GetSpanSize(i);

				// Item i did not fit on the first row, so neither does anything after it.
				if (span + spanSize > spanCount)
				{
					return false;
				}

				span += spanSize;
			}

			return true;
		}

		static bool IsInLastSpanGroup(GridLayoutManager.SpanSizeLookup spanSizeLookup, AView view, int position, int itemCount, int spanCount)
		{
			// Every item occupies at least one span, so the last row holds at most spanCount items.
			if (itemCount - 1 - position >= spanCount)
			{
				return false;
			}

			int span = GetSpanIndex(spanSizeLookup, view, position, spanCount) + spanSizeLookup.GetSpanSize(position);

			for (int i = position + 1; i < itemCount; i++)
			{
				int spanSize = spanSizeLookup.GetSpanSize(i);

				// Item i starts a new row, so `position` is not on the last one.
				if (span + spanSize > spanCount)
				{
					return false;
				}

				span += spanSize;
			}

			return true;
		}

		static int GetSpanIndex(GridLayoutManager.SpanSizeLookup spanSizeLookup, AView view, int position, int spanCount)
		{
			// GridLayoutManager assigns spans to the whole row before measuring its children, so the
			// LayoutParams already hold the answer; fall back to the lookup for anything not yet assigned.
			if (view.LayoutParameters is GridLayoutManager.LayoutParams layoutParams
				&& layoutParams.SpanIndex != GridLayoutManager.LayoutParams.InvalidSpanId)
			{
				return layoutParams.SpanIndex;
			}

			return spanSizeLookup.GetSpanIndex(position, spanCount);
		}
	}
}