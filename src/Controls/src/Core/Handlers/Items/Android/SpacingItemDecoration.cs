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
					break;
				default:
					horizontalOffset = 0;
					verticalOffset = 0;
					break;
			}

			HorizontalOffset = (int)context.ToPixels(horizontalOffset);
			VerticalOffset = (int)context.ToPixels(verticalOffset);
		}

		public override void GetItemOffsets(ARect outRect, AView view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets(outRect, view, parent, state);

			outRect.Left = HorizontalOffset;
			outRect.Right = HorizontalOffset;
			outRect.Bottom = VerticalOffset;
			outRect.Top = VerticalOffset;
		}
	}
}