#nullable disable
using Gtk.UIExtensions.NUI;
using TCollectionViewSelectionMode = Gtk.UIExtensions.NUI.CollectionViewSelectionMode;
using TItemSizingStrategy = Microsoft.Maui.Controls.ItemSizingStrategy;
using Microsoft.Maui.Controls;

namespace Gtk.UIExtensions.NUI
{
	public static class CollectionViewExtensions
	{
		public static TCollectionViewSelectionMode ToNative(this SelectionMode selectionMode)
		{
			switch (selectionMode)
			{
				case SelectionMode.Multiple:
					return TCollectionViewSelectionMode.Multiple;
				case SelectionMode.Single:
					return TCollectionViewSelectionMode.SingleAlways;
				default:
					return TCollectionViewSelectionMode.None;
			}
		}

		public static ICollectionViewLayoutManager ToLayoutManager(this IItemsLayout layout, ItemSizingStrategy sizing = ItemSizingStrategy.MeasureFirstItem)
		{
			switch (layout)
			{
				case LinearItemsLayout listItemsLayout:
					return new LinearLayoutManager(listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal, (TItemSizingStrategy)sizing, listItemsLayout.ItemSpacing.ToScaledPixel());
				case GridItemsLayout gridItemsLayout:
					return new GridLayoutManager(gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal,
												 gridItemsLayout.Span,
												 (TItemSizingStrategy)sizing,
												 gridItemsLayout.VerticalItemSpacing.ToScaledPixel(),
												 gridItemsLayout.HorizontalItemSpacing.ToScaledPixel());
				default:
					break;
			}
			return new LinearLayoutManager(false);
		}
	}
}
