using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items;

internal static class LayoutFactory
{
	public static UICollectionViewLayout CreateList(LinearItemsLayout linearItemsLayout)
	{
		if (linearItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			return CreateVerticalList(linearItemsLayout);
		
		return CreateHorizontalList(linearItemsLayout);
	}
	
	public static UICollectionViewLayout CreateCarousel(LinearItemsLayout linearItemsLayout)
	{
		if (linearItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			return CreateVerticalList(linearItemsLayout);
		
		return CreateHorizontalList(linearItemsLayout);
	}

	public static UICollectionViewLayout CreateVerticalList(LinearItemsLayout linearItemsLayout)
	{
		var layoutConfig = new UICollectionViewCompositionalLayoutConfiguration(); 
            
		var headerItemSize = NSCollectionLayoutSize.Create(NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			NSCollectionLayoutDimension.CreateEstimated(30f));
		var headerItemObj = NSCollectionLayoutBoundarySupplementaryItem.Create(layoutSize: headerItemSize, UICollectionElementKindSectionKey.Header.ToString(), NSRectAlignment.Top);
		var headerItem = headerItemObj as NSCollectionLayoutBoundarySupplementaryItem;

		layoutConfig.BoundarySupplementaryItems = new [] { headerItem! };
			
		var layout = new UICollectionViewCompositionalLayout((sectionIndex, environment) =>
		{
			var itemSize = NSCollectionLayoutSize.Create(
				NSCollectionLayoutDimension.CreateFractionalWidth(1f),
				NSCollectionLayoutDimension.CreateEstimated(30f));
			var item = NSCollectionLayoutItem.Create(itemSize);
			
			var groupSize = NSCollectionLayoutSize.Create(
				NSCollectionLayoutDimension.CreateFractionalWidth(1f),
				NSCollectionLayoutDimension.CreateEstimated(30f));

			var group = NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, count: 1);

			var section = NSCollectionLayoutSection.Create(group: group);

			return section;
		}, layoutConfig);

		return layout;
	}
	
	public static UICollectionViewLayout CreateHorizontalList(LinearItemsLayout linearItemsLayout)
	{
		var layoutConfig = new UICollectionViewCompositionalLayoutConfiguration();
		layoutConfig.ScrollDirection = UICollectionViewScrollDirection.Horizontal;
            
		var headerItemSize = NSCollectionLayoutSize.Create(NSCollectionLayoutDimension.CreateEstimated(1f),
			NSCollectionLayoutDimension.CreateFractionalHeight(1f));
		var headerItem = NSCollectionLayoutBoundarySupplementaryItem.Create(layoutSize: headerItemSize) as NSCollectionLayoutBoundarySupplementaryItem;

		layoutConfig.BoundarySupplementaryItems = new [] { headerItem! };
			
		var layout = new UICollectionViewCompositionalLayout((sectionIndex, environment) =>
		{
			var itemSize = NSCollectionLayoutSize.Create(
				NSCollectionLayoutDimension.CreateEstimated(30f),
				NSCollectionLayoutDimension.CreateFractionalHeight(1f));
			var item = NSCollectionLayoutItem.Create(itemSize);
			
			var groupSize = NSCollectionLayoutSize.Create(
				NSCollectionLayoutDimension.CreateEstimated(30f),
				NSCollectionLayoutDimension.CreateFractionalHeight(1f));

			var group = NSCollectionLayoutGroup.CreateVertical(groupSize, item, count: 1);

			var section = NSCollectionLayoutSection.Create(group: group);

			return section;
		}, layoutConfig);

		
		return layout;
	}
	
	public static UICollectionViewLayout CreateGrid(GridItemsLayout gridItemsLayout)
	{
		if (gridItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			return CreateVerticalGrid(gridItemsLayout);

		return CreateHorizontalGrid(gridItemsLayout);
	}
	
	public static UICollectionViewLayout CreateVerticalGrid(GridItemsLayout gridItemsLayout)
	{
		var layoutConfiguration = new UICollectionViewCompositionalLayoutConfiguration();
		layoutConfiguration.ScrollDirection = UICollectionViewScrollDirection.Vertical;
		
		var headerItemSize = NSCollectionLayoutSize.Create(NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			NSCollectionLayoutDimension.CreateEstimated(30f));
		var headerItem = NSCollectionLayoutBoundarySupplementaryItem.Create(layoutSize: headerItemSize) as NSCollectionLayoutBoundarySupplementaryItem;

		//layoutConfig.BoundarySupplementaryItems = new [] { headerItem! };
			
		var layout = new UICollectionViewCompositionalLayout((sectionIndex, environment) =>
		{
			var columns = gridItemsLayout.Span; // sectionIndex == 0 ? 2 : 4

			var itemSize = NSCollectionLayoutSize.Create(
				NSCollectionLayoutDimension.CreateFractionalWidth(1f / columns),
				NSCollectionLayoutDimension.CreateEstimated(30f));
			var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

			var groupSize = NSCollectionLayoutSize.Create(NSCollectionLayoutDimension.CreateFractionalWidth(1f),
				NSCollectionLayoutDimension.CreateEstimated(30f));

			var group = NSCollectionLayoutGroup.CreateHorizontal(layoutSize: groupSize,
				subitem: item,
				count: columns);

			var section = NSCollectionLayoutSection.Create(group: group);

			return section;
		}, layoutConfiguration);

		return layout;
	}
	
	
	public static UICollectionViewLayout CreateHorizontalGrid(GridItemsLayout gridItemsLayout)
	{
		var layoutConfiguration = new UICollectionViewCompositionalLayoutConfiguration();
		layoutConfiguration.ScrollDirection = UICollectionViewScrollDirection.Horizontal;
		
		var headerItemSize = NSCollectionLayoutSize.Create(NSCollectionLayoutDimension.CreateEstimated(30f),
			NSCollectionLayoutDimension.CreateFractionalHeight(1f));
		var headerItem = NSCollectionLayoutBoundarySupplementaryItem.Create(layoutSize: headerItemSize) as NSCollectionLayoutBoundarySupplementaryItem;

		//layoutConfig.BoundarySupplementaryItems = new [] { headerItem! };
			
		var layout = new UICollectionViewCompositionalLayout((sectionIndex, environment) =>
		{
			var rows = gridItemsLayout.Span; // sectionIndex == 0 ? 2 : 4
			var itemSize = NSCollectionLayoutSize.Create(
				NSCollectionLayoutDimension.CreateEstimated(30f),
				NSCollectionLayoutDimension.CreateFractionalHeight(1f / rows));
			var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

			var groupSize = NSCollectionLayoutSize.Create(NSCollectionLayoutDimension.CreateEstimated(30f),
				NSCollectionLayoutDimension.CreateFractionalHeight(1f));

			var group = NSCollectionLayoutGroup.CreateVertical(layoutSize: groupSize,
				subitem: item,
				count: rows);

			var section = NSCollectionLayoutSection.Create(group: group);

			return section;
		}, layoutConfiguration);

		return layout;
	}
}