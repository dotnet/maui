using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items;

internal static class LayoutFactory
{
	public static UICollectionViewLayout CreateList(LinearItemsLayout linearItemsLayout,
		LayoutGroupingInfo groupingInfo)
		=> linearItemsLayout.Orientation == ItemsLayoutOrientation.Vertical
			? CreateVerticalList(linearItemsLayout, groupingInfo)
			: CreateHorizontalList(linearItemsLayout, groupingInfo);
	
	public static UICollectionViewLayout CreateCarousel(LinearItemsLayout linearItemsLayout, LayoutGroupingInfo groupingInfo)
		=> linearItemsLayout.Orientation == ItemsLayoutOrientation.Vertical
			? CreateVerticalCarousel(linearItemsLayout)
			: CreateHorizontalCarousel(linearItemsLayout);
	
	public static UICollectionViewLayout CreateGrid(GridItemsLayout gridItemsLayout, LayoutGroupingInfo groupingInfo)
		=> gridItemsLayout.Orientation == ItemsLayoutOrientation.Vertical
			? CreateVerticalGrid(gridItemsLayout, groupingInfo)
			: CreateHorizontalGrid(gridItemsLayout, groupingInfo);

	static NSCollectionLayoutBoundarySupplementaryItem[] CreateSupplementaryItems(LayoutGroupingInfo groupingInfo,
		UICollectionViewScrollDirection scrollDirection, NSCollectionLayoutDimension width, NSCollectionLayoutDimension height)
	{
		if (groupingInfo.IsGrouped)
		{
			var items = new List<NSCollectionLayoutBoundarySupplementaryItem>();
			
			if (groupingInfo.HasHeader)
			{
				items.Add(NSCollectionLayoutBoundarySupplementaryItem.Create(
					NSCollectionLayoutSize.Create(width, height),
					UICollectionElementKindSectionKey.Header.ToString(),
					scrollDirection == UICollectionViewScrollDirection.Vertical
						? NSRectAlignment.Top
						: NSRectAlignment.Leading));
			}

			if (groupingInfo.HasFooter)
			{
				items.Add(NSCollectionLayoutBoundarySupplementaryItem.Create(
					NSCollectionLayoutSize.Create(width, height),
					UICollectionElementKindSectionKey.Footer.ToString(),
					scrollDirection == UICollectionViewScrollDirection.Vertical
						? NSRectAlignment.Bottom
						: NSRectAlignment.Trailing));
			}

			return items.ToArray();
		}

		return [];
	}

	static UICollectionViewLayout CreateListLayout(UICollectionViewScrollDirection scrollDirection, LayoutGroupingInfo groupingInfo, NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight, NSCollectionLayoutDimension groupWidth, NSCollectionLayoutDimension groupHeight, double itemSpacing)
	{
		var layoutConfiguration = new UICollectionViewCompositionalLayoutConfiguration();
		layoutConfiguration.ScrollDirection = scrollDirection;

		var layout = new UICollectionViewCompositionalLayout((sectionIndex, environment) =>
		{
			// Each item has a size
			var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
			// Create the item itself from the size
			var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);
			
			// Each group of items (for grouped collections) has a size
			var groupSize = NSCollectionLayoutSize.Create(groupWidth, groupHeight);

			// Create the group
			// If vertical list, we want the group to layout horizontally (eg: grid columns go left to right)
			// for horizontal list, we want to lay grid rows out vertically
			// For simple lists it doesn't matter so much since the items span the entire width or height
			var group = scrollDirection == UICollectionViewScrollDirection.Vertical
				? NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, 1)
				: NSCollectionLayoutGroup.CreateVertical(groupSize, item, 1);
			
			if (itemSpacing > 0)
				group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(itemSpacing));

			// Create our section layout
			var section = NSCollectionLayoutSection.Create(group: group);

			section.BoundarySupplementaryItems = CreateSupplementaryItems(
				groupingInfo,
				scrollDirection,
				groupWidth,
				groupHeight);
			
			return section;
		}, layoutConfiguration);

		return layout;
	}


	static UICollectionViewLayout CreateGridLayout(UICollectionViewScrollDirection scrollDirection, LayoutGroupingInfo groupingInfo, NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight, NSCollectionLayoutDimension groupWidth, NSCollectionLayoutDimension groupHeight, double verticalItemSpacing, double horizontalItemSpacing,  int columns)
	{
		var layoutConfiguration = new UICollectionViewCompositionalLayoutConfiguration();
		layoutConfiguration.ScrollDirection = scrollDirection;

		var layout = new UICollectionViewCompositionalLayout((sectionIndex, environment) =>
		{
			// Each item has a size
			var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
			// Create the item itself from the size
			var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

			// Each group of items (for grouped collections) has a size
			var groupSize = NSCollectionLayoutSize.Create(groupWidth, groupHeight);

			// Create the group
			// If vertical list, we want the group to layout horizontally (eg: grid columns go left to right)
			// for horizontal list, we want to lay grid rows out vertically
			// For simple lists it doesn't matter so much since the items span the entire width or height
			var group = scrollDirection == UICollectionViewScrollDirection.Vertical
				? NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, columns)
				: NSCollectionLayoutGroup.CreateVertical(groupSize, item, columns);
			
			if (scrollDirection == UICollectionViewScrollDirection.Vertical)
				group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(verticalItemSpacing));
			else
				group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(horizontalItemSpacing));

			// Create our section layout
			var section = NSCollectionLayoutSection.Create(group: group);
			
			section.BoundarySupplementaryItems = CreateSupplementaryItems(
				groupingInfo,
				scrollDirection,
				groupWidth,
				groupHeight);
			
			return section;
		}, layoutConfiguration);

		return layout;
	}

	public static UICollectionViewLayout CreateVerticalList(LinearItemsLayout linearItemsLayout,
		LayoutGroupingInfo groupingInfo)
		=> CreateListLayout(UICollectionViewScrollDirection.Vertical,
			groupingInfo,
			// Fill the width
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			// Dynamic (estimate required)
			NSCollectionLayoutDimension.CreateEstimated(30f),
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			NSCollectionLayoutDimension.CreateEstimated(30f),
			linearItemsLayout.ItemSpacing);


	public static UICollectionViewLayout CreateHorizontalList(LinearItemsLayout linearItemsLayout,
		LayoutGroupingInfo groupingInfo)
		=> CreateListLayout(UICollectionViewScrollDirection.Horizontal,
			groupingInfo,
			// Dynamic, estimated width
			NSCollectionLayoutDimension.CreateEstimated(30f),
			// Fill the height for horizontal
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			NSCollectionLayoutDimension.CreateEstimated(30f),
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			linearItemsLayout.ItemSpacing);

	public static UICollectionViewLayout CreateVerticalCarousel(LinearItemsLayout linearItemsLayout)
		=> CreateListLayout(UICollectionViewScrollDirection.Vertical,
			new LayoutGroupingInfo { IsGrouped = false, HasHeader = false, HasFooter = false },
			// Fill the width and height
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			0d);


	public static UICollectionViewLayout CreateHorizontalCarousel(LinearItemsLayout linearItemsLayout)
		=> CreateListLayout(UICollectionViewScrollDirection.Horizontal,
			new LayoutGroupingInfo { IsGrouped = false, HasHeader = false, HasFooter = false },
			// Fill the width and height
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			0d);
	
	public static UICollectionViewLayout CreateVerticalGrid(GridItemsLayout gridItemsLayout,
		LayoutGroupingInfo groupingInfo)
		=> CreateGridLayout(UICollectionViewScrollDirection.Vertical,
			groupingInfo,
			// Width is the number of columns
			NSCollectionLayoutDimension.CreateFractionalWidth(1f / gridItemsLayout.Span),
			// Height is dynamic, estimated
			NSCollectionLayoutDimension.CreateEstimated(30f),
			// Group spans all columns, full width for vertical
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			// Group is dynamic height for vertical
			NSCollectionLayoutDimension.CreateEstimated(30f),
			gridItemsLayout.VerticalItemSpacing,
			gridItemsLayout.HorizontalItemSpacing,
			gridItemsLayout.Span);
	
	
	public static UICollectionViewLayout CreateHorizontalGrid(GridItemsLayout gridItemsLayout,
		LayoutGroupingInfo groupingInfo)
		=> CreateGridLayout(UICollectionViewScrollDirection.Horizontal,
			groupingInfo,
			// Item width is estimated
			NSCollectionLayoutDimension.CreateEstimated(30f),
			// Item height is number of rows
			NSCollectionLayoutDimension.CreateFractionalHeight(1f / gridItemsLayout.Span),
			// Group width is dynamic for horizontal
			NSCollectionLayoutDimension.CreateEstimated(30f),
			// Group spans all rows, full height for horizontal
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			gridItemsLayout.VerticalItemSpacing,
			gridItemsLayout.HorizontalItemSpacing,
			gridItemsLayout.Span);
	
}