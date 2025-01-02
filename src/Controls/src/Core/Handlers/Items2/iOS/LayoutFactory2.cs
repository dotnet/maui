using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2;

internal static class LayoutFactory2
{
	public static UICollectionViewLayout CreateList(LinearItemsLayout linearItemsLayout,
		LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo)
		=> linearItemsLayout.Orientation == ItemsLayoutOrientation.Vertical
			? CreateVerticalList(linearItemsLayout, groupingInfo, headerFooterInfo)
			: CreateHorizontalList(linearItemsLayout, groupingInfo, headerFooterInfo);

	public static UICollectionViewLayout CreateGrid(GridItemsLayout gridItemsLayout,
	 LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo)
		=> gridItemsLayout.Orientation == ItemsLayoutOrientation.Vertical
			? CreateVerticalGrid(gridItemsLayout, groupingInfo, headerFooterInfo)
			: CreateHorizontalGrid(gridItemsLayout, groupingInfo, headerFooterInfo);

	static NSCollectionLayoutBoundarySupplementaryItem[] CreateSupplementaryItems(LayoutGroupingInfo? groupingInfo, LayoutHeaderFooterInfo? layoutHeaderFooterInfo,
		UICollectionViewScrollDirection scrollDirection, NSCollectionLayoutDimension width, NSCollectionLayoutDimension height)
	{
		if (groupingInfo is not null && groupingInfo.IsGrouped)
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

		if (layoutHeaderFooterInfo is not null)
		{
			var items = new List<NSCollectionLayoutBoundarySupplementaryItem>();

			if (layoutHeaderFooterInfo.HasHeader)
			{
				items.Add(NSCollectionLayoutBoundarySupplementaryItem.Create(
					NSCollectionLayoutSize.Create(width, height),
					UICollectionElementKindSectionKey.Header.ToString(),
					scrollDirection == UICollectionViewScrollDirection.Vertical
						? NSRectAlignment.Top
						: NSRectAlignment.Leading));
			};

			if (layoutHeaderFooterInfo.HasFooter)
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

	static UICollectionViewLayout CreateListLayout(UICollectionViewScrollDirection scrollDirection, LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo layoutHeaderFooterInfo, LayoutSnapInfo snapInfo, NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight, NSCollectionLayoutDimension groupWidth, NSCollectionLayoutDimension groupHeight, double itemSpacing, Func<Thickness>? peekAreaInsetsFunc)
	{
		var layoutConfiguration = new UICollectionViewCompositionalLayoutConfiguration();
		layoutConfiguration.ScrollDirection = scrollDirection;

		//create global header and footer
		layoutConfiguration.BoundarySupplementaryItems = CreateSupplementaryItems(null, layoutHeaderFooterInfo, scrollDirection, groupWidth, groupHeight);

		var layout = new CustomUICollectionViewCompositionalLayout(snapInfo, (sectionIndex, environment) =>
		{
			// Each item has a size
			var itemSize = NSCollectionLayoutSize.Create(itemWidth, itemHeight);
			// Create the item itself from the size
			var item = NSCollectionLayoutItem.Create(layoutSize: itemSize);

			if (peekAreaInsetsFunc?.Invoke() is Thickness peekAreaInsets)
			{
				if (scrollDirection == UICollectionViewScrollDirection.Vertical)
				{
					var newGroupHeight = environment.Container.ContentSize.Height - peekAreaInsets.Top - peekAreaInsets.Bottom;
					groupHeight = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupHeight);
				}
				else
				{
					var newGroupWidth = environment.Container.ContentSize.Width - peekAreaInsets.Left - peekAreaInsets.Right;
					groupWidth = NSCollectionLayoutDimension.CreateAbsolute((nfloat)newGroupWidth);
				}
			}
			// Each group of items (for grouped collections) has a size
			var groupSize = NSCollectionLayoutSize.Create(groupWidth, groupHeight);

			// Create the group
			// If vertical list, we want the group to layout horizontally (eg: grid columns go left to right)
			// for horizontal list, we want to lay grid rows out vertically
			// For simple lists it doesn't matter so much since the items span the entire width or height
			var group = scrollDirection == UICollectionViewScrollDirection.Vertical
				? NSCollectionLayoutGroup.CreateHorizontal(groupSize, item, 1)
				: NSCollectionLayoutGroup.CreateVertical(groupSize, item, 1);

			//group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(itemSpacing));

			// Create our section layout
			var section = NSCollectionLayoutSection.Create(group: group);
			section.InterGroupSpacing = new NFloat(itemSpacing);

			// Create header and footer for group
			section.BoundarySupplementaryItems = CreateSupplementaryItems(
				groupingInfo,
				null,
				scrollDirection,
				groupWidth,
				groupHeight);

			return section;
		}, layoutConfiguration);

		return layout;
	}



	static UICollectionViewLayout CreateGridLayout(UICollectionViewScrollDirection scrollDirection, LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo, LayoutSnapInfo snapInfo, NSCollectionLayoutDimension itemWidth, NSCollectionLayoutDimension itemHeight, NSCollectionLayoutDimension groupWidth, NSCollectionLayoutDimension groupHeight, double verticalItemSpacing, double horizontalItemSpacing, int columns)
	{
		var layoutConfiguration = new UICollectionViewCompositionalLayoutConfiguration();
		layoutConfiguration.ScrollDirection = scrollDirection;

		var layout = new CustomUICollectionViewCompositionalLayout(snapInfo, (sectionIndex, environment) =>
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
				group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(horizontalItemSpacing));
			else
				group.InterItemSpacing = NSCollectionLayoutSpacing.CreateFixed(new NFloat(verticalItemSpacing));

			// Create our section layout
			var section = NSCollectionLayoutSection.Create(group: group);

			if (scrollDirection == UICollectionViewScrollDirection.Vertical)
				section.InterGroupSpacing = new NFloat(verticalItemSpacing);
			else
				section.InterGroupSpacing = new NFloat(horizontalItemSpacing);


			section.BoundarySupplementaryItems = CreateSupplementaryItems(
				groupingInfo,
				headerFooterInfo,
				scrollDirection,
				groupWidth,
				groupHeight);

			return section;
		}, layoutConfiguration);

		return layout;
	}

	public static UICollectionViewLayout CreateVerticalList(LinearItemsLayout linearItemsLayout,
		LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo)
		=> CreateListLayout(UICollectionViewScrollDirection.Vertical,
			groupingInfo,
			headerFooterInfo,
			new LayoutSnapInfo { SnapType = linearItemsLayout.SnapPointsType, SnapAligment = linearItemsLayout.SnapPointsAlignment },
			// Fill the width
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			// Dynamic (estimate required)
			NSCollectionLayoutDimension.CreateEstimated(30f),
			NSCollectionLayoutDimension.CreateFractionalWidth(1f),
			NSCollectionLayoutDimension.CreateEstimated(30f),
			linearItemsLayout.ItemSpacing,
			null);


	public static UICollectionViewLayout CreateHorizontalList(LinearItemsLayout linearItemsLayout,
		LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo)
		=> CreateListLayout(UICollectionViewScrollDirection.Horizontal,
			groupingInfo,
			headerFooterInfo,
			new LayoutSnapInfo { SnapType = linearItemsLayout.SnapPointsType, SnapAligment = linearItemsLayout.SnapPointsAlignment },
			// Dynamic, estimated width
			NSCollectionLayoutDimension.CreateEstimated(30f),
			// Fill the height for horizontal
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			NSCollectionLayoutDimension.CreateEstimated(30f),
			NSCollectionLayoutDimension.CreateFractionalHeight(1f),
			linearItemsLayout.ItemSpacing,
			null);

	public static UICollectionViewLayout CreateVerticalGrid(GridItemsLayout gridItemsLayout,
		LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo)
		=> CreateGridLayout(UICollectionViewScrollDirection.Vertical,
			groupingInfo,
			headerFooterInfo,
			new LayoutSnapInfo { SnapType = gridItemsLayout.SnapPointsType, SnapAligment = gridItemsLayout.SnapPointsAlignment },
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
		LayoutGroupingInfo groupingInfo, LayoutHeaderFooterInfo headerFooterInfo)
		=> CreateGridLayout(UICollectionViewScrollDirection.Horizontal,
			groupingInfo,
			headerFooterInfo,
			new LayoutSnapInfo { SnapType = gridItemsLayout.SnapPointsType, SnapAligment = gridItemsLayout.SnapPointsAlignment },
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


	class CustomUICollectionViewCompositionalLayout : UICollectionViewCompositionalLayout
	{
		LayoutSnapInfo _snapInfo;
		public CustomUICollectionViewCompositionalLayout(LayoutSnapInfo snapInfo, UICollectionViewCompositionalLayoutSectionProvider sectionProvider, UICollectionViewCompositionalLayoutConfiguration configuration) : base(sectionProvider, configuration)
		{
			_snapInfo = snapInfo;
		}

		public override CGPoint TargetContentOffset(CGPoint proposedContentOffset, CGPoint scrollingVelocity)
		{
			var snapPointsType = _snapInfo.SnapType;
			var alignment = _snapInfo.SnapAligment;

			if (snapPointsType == SnapPointsType.None)
			{
				// Nothing to do here; fall back to the default
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			if (snapPointsType == SnapPointsType.MandatorySingle)
			{
				// Mandatory snapping, single element
				return ScrollSingle(alignment, proposedContentOffset, scrollingVelocity);
			}

			// Get the viewport of the UICollectionView at the proposed content offset
			var viewport = new CGRect(proposedContentOffset, CollectionView.Bounds.Size);

			// And find all the elements currently visible in the viewport
			var visibleElements = LayoutAttributesForElementsInRect(viewport);

			if (visibleElements.Length == 0)
			{
				// Nothing to see here; fall back to the default
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			if (visibleElements.Length == 1)
			{
				// If there is only one item in the viewport,  then we need to align the viewport with it
				return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, visibleElements[0].Frame, viewport,
					alignment, Configuration.ScrollDirection);
			}

			// If there are multiple items in the viewport, we need to choose the one which is 
			// closest to the relevant part of the viewport while being sufficiently visible

			// Find the spot in the viewport we're trying to align with
			var alignmentTarget = Items.SnapHelpers.FindAlignmentTarget(alignment, proposedContentOffset,
				CollectionView, Configuration.ScrollDirection);

			// Find the closest sufficiently visible candidate
			var bestCandidate = Items.SnapHelpers.FindBestSnapCandidate(visibleElements, viewport, alignmentTarget);

			if (bestCandidate != null)
			{
				return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, bestCandidate.Frame, viewport, alignment,
					Configuration.ScrollDirection);
			}

			// If we got this far an nothing matched, it means that we have multiple items but somehow
			// none of them fit at least half in the viewport. So just fall back to the first item
			return Items.SnapHelpers.AdjustContentOffset(proposedContentOffset, visibleElements[0].Frame, viewport, alignment,
					Configuration.ScrollDirection);
		}

		CGPoint ScrollSingle(SnapPointsAlignment alignment, CGPoint proposedContentOffset, CGPoint scrollingVelocity)
		{
			// Get the viewport of the UICollectionView at the current content offset
			var contentOffset = CollectionView.ContentOffset;
			var viewport = new CGRect(contentOffset, CollectionView.Bounds.Size);

			// Find the spot in the viewport we're trying to align with
			var alignmentTarget = Items.SnapHelpers.FindAlignmentTarget(alignment, contentOffset, CollectionView, Configuration.ScrollDirection);

			var visibleElements = LayoutAttributesForElementsInRect(viewport);

			// Find the current aligned item
			var currentItem = Items.SnapHelpers.FindBestSnapCandidate(visibleElements, viewport, alignmentTarget);

			if (currentItem == null)
			{
				// Somehow we don't currently have an item in the viewport near the target; fall back to the
				// default behavior
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			// Determine the index of the current item
			var currentIndex = visibleElements.IndexOf(currentItem);

			// Figure out the step size when jumping to the "next" element 
			var span = 1;
			// if (_itemsLayout is GridItemsLayout gridItemsLayout)
			// {
			// 	span = gridItemsLayout.Span;
			// }

			// Find the next item in the
			currentItem = Items.SnapHelpers.FindNextItem(visibleElements, Configuration.ScrollDirection, span, scrollingVelocity, currentIndex);

			return Items.SnapHelpers.AdjustContentOffset(CollectionView.ContentOffset, currentItem.Frame, viewport, alignment,
				Configuration.ScrollDirection);
		}
	}

}