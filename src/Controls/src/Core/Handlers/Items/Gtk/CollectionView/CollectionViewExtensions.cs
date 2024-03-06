#nullable disable

using Microsoft.Maui.Controls;

namespace Gtk.UIExtensions.NUI
{

	public static class CollectionViewExtensions
	{

		public static ICollectionViewLayoutManager ToLayoutManager(this IItemsLayout layout, ItemSizingStrategy sizing = ItemSizingStrategy.MeasureFirstItem)
		{
			switch (layout)
			{
				case LinearItemsLayout listItemsLayout:
					return new LinearLayoutManager(listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal, sizing, listItemsLayout.ItemSpacing.ToScaledPixel());
				case GridItemsLayout gridItemsLayout:
					return new GridLayoutManager(gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal,
						gridItemsLayout.Span,
						sizing,
						gridItemsLayout.VerticalItemSpacing.ToScaledPixel(),
						gridItemsLayout.HorizontalItemSpacing.ToScaledPixel());
				default:
					break;
			}

			return new LinearLayoutManager(false);
		}

	}

}