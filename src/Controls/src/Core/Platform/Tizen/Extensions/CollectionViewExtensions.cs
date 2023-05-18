#nullable disable
using Tizen.UIExtensions.NUI;
using TCollectionViewSelectionMode = Tizen.UIExtensions.NUI.CollectionViewSelectionMode;
using TItemSizingStrategy = Tizen.UIExtensions.NUI.ItemSizingStrategy;

namespace Microsoft.Maui.Controls.Platform
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
