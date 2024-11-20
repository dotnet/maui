#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract class ItemsViewLayout : UICollectionViewFlowLayout
	{
		readonly ItemsLayout _itemsLayout;
		bool _disposed;
		bool _adjustContentOffset;
		CGSize _adjustmentSize0;
		CGSize _adjustmentSize1;
		CGSize _currentSize;
		WeakReference<Func<UICollectionViewCell>> _getPrototype;

		WeakReference<Func<NSIndexPath, UICollectionViewCell>> _getPrototypeForIndexPath;

		readonly Dictionary<object, CGSize> _cellSizeCache = new();

		public ItemsUpdatingScrollMode ItemsUpdatingScrollMode { get; set; }

		public nfloat ConstrainedDimension { get; set; }

		public Func<UICollectionViewCell> GetPrototype
		{
			get => _getPrototype is not null && _getPrototype.TryGetTarget(out var func) ? func : null;
			set => _getPrototype = new(value);
		}

		internal Func<NSIndexPath, UICollectionViewCell> GetPrototypeForIndexPath
		{
			get => _getPrototypeForIndexPath is not null && _getPrototypeForIndexPath.TryGetTarget(out var func) ? func : null;
			set => _getPrototypeForIndexPath = new(value);
		}

		internal ItemSizingStrategy ItemSizingStrategy { get; private set; }

		protected ItemsViewLayout(ItemsLayout itemsLayout, ItemSizingStrategy itemSizingStrategy = ItemSizingStrategy.MeasureFirstItem)
		{
			ItemSizingStrategy = itemSizingStrategy;

			_itemsLayout = itemsLayout;
			_itemsLayout.PropertyChanged += LayoutOnPropertyChanged;

			var scrollDirection = itemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
				? UICollectionViewScrollDirection.Horizontal
				: UICollectionViewScrollDirection.Vertical;

			Initialize(scrollDirection);

			if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)
#if TVOS
				|| OperatingSystem.IsTvOSVersionAtLeast(11)
#endif
			)
			{
				// `ContentInset` is actually the default value, but I'm leaving this here as a note to
				// future maintainers; it's likely that someone will want a Platform Specific to change this behavior
				// (Setting it to `SafeArea` lets you do the thing where the header/footer of your UICollectionView
				// fills the screen width in landscape while your items are automatically shifted to avoid the notch)
				SectionInsetReference = UICollectionViewFlowLayoutSectionInsetReference.ContentInset;
			}
		}

		public override bool FlipsHorizontallyInOppositeLayoutDirection => true;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (_itemsLayout != null)
				{
					_itemsLayout.PropertyChanged -= LayoutOnPropertyChanged;
				}
			}

			base.Dispose(disposing);
		}

		void LayoutOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChanged)
		{
			HandlePropertyChanged(propertyChanged);
		}

		protected virtual void HandlePropertyChanged(PropertyChangedEventArgs propertyChanged)
		{
			if (propertyChanged.IsOneOf(LinearItemsLayout.ItemSpacingProperty,
				GridItemsLayout.HorizontalItemSpacingProperty, GridItemsLayout.VerticalItemSpacingProperty))
			{
				UpdateItemSpacing();
			}
		}

		internal virtual bool UpdateConstraints(CGSize size)
		{
			if (size.IsCloseTo(_currentSize))
			{
				return false;
			}

			ClearCellSizeCache();

			_currentSize = size;

			var newSize = new CGSize(Math.Floor(size.Width), Math.Floor(size.Height));
			ConstrainTo(newSize);

			UpdateCellConstraints();
			return true;
		}

		internal void SetInitialConstraints(CGSize size)
		{
			_currentSize = size;
			ConstrainTo(size);
		}

		public abstract void ConstrainTo(CGSize size);

		public virtual UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout,
			nint section)
		{
			if (_itemsLayout is GridItemsLayout gridItemsLayout)
			{
				if (ScrollDirection == UICollectionViewScrollDirection.Horizontal)
				{
					return new UIEdgeInsets(0, 0, 0, new nfloat(gridItemsLayout.HorizontalItemSpacing * collectionView.NumberOfItemsInSection(section)));
				}

				return new UIEdgeInsets(0, 0, new nfloat(gridItemsLayout.VerticalItemSpacing * collectionView.NumberOfItemsInSection(section)), 0);
			}

			return UIEdgeInsets.Zero;
		}

		public virtual nfloat GetMinimumInteritemSpacingForSection(UICollectionView collectionView,
			UICollectionViewLayout layout, nint section)
		{
			return (nfloat)0.0;
		}

		public virtual nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView,
			UICollectionViewLayout layout, nint section)
		{
			if (_itemsLayout is LinearItemsLayout listViewLayout)
			{
				return (nfloat)listViewLayout.ItemSpacing;
			}

			if (_itemsLayout is GridItemsLayout gridItemsLayout)
			{
				if (ScrollDirection == UICollectionViewScrollDirection.Horizontal)
				{
					return (nfloat)gridItemsLayout.HorizontalItemSpacing;
				}

				return (nfloat)gridItemsLayout.VerticalItemSpacing;
			}

			return (nfloat)0.0;
		}

		public void PrepareCellForLayout(ItemsViewCell cell)
		{
			if (EstimatedItemSize == CGSize.Empty)
			{
				cell.ConstrainTo(ItemSize);
			}
			else
			{
				cell.ConstrainTo(ConstrainedDimension);
			}
		}

		public override bool ShouldInvalidateLayout(UICollectionViewLayoutAttributes preferredAttributes, UICollectionViewLayoutAttributes originalAttributes)
		{
			// This is currently causing an infinite layout loop on iOS 15 https://github.com/dotnet/maui/issues/6566
			if (preferredAttributes.RepresentedElementKind == "UICollectionElementKindSectionHeader" && OperatingSystem.IsIOSVersionAtLeast(15))
				return base.ShouldInvalidateLayout(preferredAttributes, originalAttributes);

			if (ItemSizingStrategy == ItemSizingStrategy.MeasureAllItems)
			{
				if (preferredAttributes.Bounds != originalAttributes.Bounds)
				{
					return true;
				}
			}

			return base.ShouldInvalidateLayout(preferredAttributes, originalAttributes);
		}

		protected void DetermineCellSize()
		{
			if (GetPrototype == null)
			{
				return;
			}

			// We set the EstimatedItemSize here for two reasons:
			// 1. If we don't set it, iOS versions below 10 will crash
			// 2. If GetPrototype() cannot return a cell because the items source is empty, we need to have
			//		an estimate set so that when a cell _does_ become available (i.e., when the items source
			//		has at least one item), Autolayout will kick in for the first cell and size it correctly
			// If GetPrototype() _can_ return a cell, this estimate will be updated once that cell is measured
			if (EstimatedItemSize == CGSize.Empty)
			{
				EstimatedItemSize = new CGSize(1, 1);
			}

			ItemsViewCell prototype = null;

			if (CollectionView?.VisibleCells.Length > 0)
			{
				prototype = CollectionView.VisibleCells[0] as ItemsViewCell;
			}

			if (prototype == null)
			{
				prototype = GetPrototype() as ItemsViewCell;
			}

			if (prototype == null)
			{
				return;
			}

			// Constrain and measure the prototype cell
			prototype.ConstrainTo(ConstrainedDimension);
			var measure = prototype.Measure();

			if (ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
			{
				// This is the size we'll give all of our cells from here on out
				ItemSize = measure;

				// Make sure autolayout is disabled 
				EstimatedItemSize = CGSize.Empty;
			}
			else
			{
				// Autolayout is now enabled, and this is the size used to guess scrollbar size and progress
				measure = TryFindEstimatedSize(measure);
				EstimatedItemSize = measure;
			}
		}

		void Initialize(UICollectionViewScrollDirection scrollDirection)
		{
			ScrollDirection = scrollDirection;
		}

		protected void UpdateCellConstraints()
		{
			PrepareCellsForLayout(CollectionView.VisibleCells);
			PrepareCellsForLayout(CollectionView.GetVisibleSupplementaryViews(UICollectionElementKindSectionKey.Header));
			PrepareCellsForLayout(CollectionView.GetVisibleSupplementaryViews(UICollectionElementKindSectionKey.Footer));
		}

		void PrepareCellsForLayout(UICollectionReusableView[] cells)
		{
			for (int n = 0; n < cells.Length; n++)
			{
				if (cells[n] is ItemsViewCell constrainedCell)
				{
					PrepareCellForLayout(constrainedCell);
				}
			}
		}

		public override CGPoint TargetContentOffset(CGPoint proposedContentOffset, CGPoint scrollingVelocity)
		{
			var snapPointsType = _itemsLayout.SnapPointsType;

			if (snapPointsType == SnapPointsType.None)
			{
				// Nothing to do here; fall back to the default
				return base.TargetContentOffset(proposedContentOffset, scrollingVelocity);
			}

			var alignment = _itemsLayout.SnapPointsAlignment;

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
				return SnapHelpers.AdjustContentOffset(proposedContentOffset, visibleElements[0].Frame, viewport,
					alignment, ScrollDirection);
			}

			// If there are multiple items in the viewport, we need to choose the one which is 
			// closest to the relevant part of the viewport while being sufficiently visible

			// Find the spot in the viewport we're trying to align with
			var alignmentTarget = SnapHelpers.FindAlignmentTarget(alignment, proposedContentOffset,
				CollectionView, ScrollDirection);

			// Find the closest sufficiently visible candidate
			var bestCandidate = SnapHelpers.FindBestSnapCandidate(visibleElements, viewport, alignmentTarget);

			if (bestCandidate != null)
			{
				return SnapHelpers.AdjustContentOffset(proposedContentOffset, bestCandidate.Frame, viewport, alignment,
					ScrollDirection);
			}

			// If we got this far an nothing matched, it means that we have multiple items but somehow
			// none of them fit at least half in the viewport. So just fall back to the first item
			return SnapHelpers.AdjustContentOffset(proposedContentOffset, visibleElements[0].Frame, viewport, alignment,
					ScrollDirection);
		}

		CGPoint ScrollSingle(SnapPointsAlignment alignment, CGPoint proposedContentOffset, CGPoint scrollingVelocity)
		{
			// Get the viewport of the UICollectionView at the current content offset
			var contentOffset = CollectionView.ContentOffset;
			var viewport = new CGRect(contentOffset, CollectionView.Bounds.Size);

			// Find the spot in the viewport we're trying to align with
			var alignmentTarget = SnapHelpers.FindAlignmentTarget(alignment, contentOffset, CollectionView, ScrollDirection);

			var visibleElements = LayoutAttributesForElementsInRect(viewport);

			// Find the current aligned item
			var currentItem = SnapHelpers.FindBestSnapCandidate(visibleElements, viewport, alignmentTarget);

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
			if (_itemsLayout is GridItemsLayout gridItemsLayout)
			{
				span = gridItemsLayout.Span;
			}

			// Find the next item in the
			currentItem = SnapHelpers.FindNextItem(visibleElements, ScrollDirection, span, scrollingVelocity, currentIndex);

			return SnapHelpers.AdjustContentOffset(CollectionView.ContentOffset, currentItem.Frame, viewport, alignment,
				ScrollDirection);
		}

		protected virtual void UpdateItemSpacing()
		{
			if (_itemsLayout == null)
			{
				return;
			}

			InvalidateLayout();
		}

		public override UICollectionViewLayoutInvalidationContext GetInvalidationContext(UICollectionViewLayoutAttributes preferredAttributes, UICollectionViewLayoutAttributes originalAttributes)
		{
			if (preferredAttributes.RepresentedElementKind != UICollectionElementKindSectionKey.Header
				&& preferredAttributes.RepresentedElementKind != UICollectionElementKindSectionKey.Footer)
			{
				if (OperatingSystem.IsIOSVersionAtLeast(12) || OperatingSystem.IsTvOSVersionAtLeast(12))
				{
					return base.GetInvalidationContext(preferredAttributes, originalAttributes);
				}

				try
				{
					// We only have to do this on older iOS versions; sometimes when removing a cell that's right at the edge
					// of the viewport we'll run into a race condition where the invalidation context will have the removed
					// indexpath. And then things crash. So 

					var defaultContext = base.GetInvalidationContext(preferredAttributes, originalAttributes);
					return defaultContext;
				}
				catch (ObjCRuntime.ObjCException ex) when (ex.Name == "NSRangeException")
				{
					Application.Current?.FindMauiContext()?.CreateLogger<ItemsViewLayout>()?.LogWarning(ex, "NSRangeException");
				}

				UICollectionViewFlowLayoutInvalidationContext context = new UICollectionViewFlowLayoutInvalidationContext();
				return context;
			}

			// Ensure that if this invalidation was triggered by header/footer changes, the header/footer are being invalidated

			UICollectionViewFlowLayoutInvalidationContext invalidationContext = new UICollectionViewFlowLayoutInvalidationContext();
			var indexPath = preferredAttributes.IndexPath;

			if (preferredAttributes.RepresentedElementKind == UICollectionElementKindSectionKey.Header)
			{
				invalidationContext.InvalidateSupplementaryElements(UICollectionElementKindSectionKey.Header, new[] { indexPath });
			}
			else if (preferredAttributes.RepresentedElementKind == UICollectionElementKindSectionKey.Footer)
			{
				invalidationContext.InvalidateSupplementaryElements(UICollectionElementKindSectionKey.Footer, new[] { indexPath });
			}

			return invalidationContext;
		}

		public override void PrepareLayout()
		{
			base.PrepareLayout();

			// PrepareLayout is the only good place to consistently track the content size changes
			TrackOffsetAdjustment();
		}

		public override void PrepareForCollectionViewUpdates(UICollectionViewUpdateItem[] updateItems)
		{
			base.PrepareForCollectionViewUpdates(updateItems);

			if (ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepScrollOffset)
			{
				// This is the default behavior for iOS, no need to do anything
				return;
			}

			if (ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView
			   || ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
			{
				// If this update will shift the visible items,  we'll have to adjust for 
				// that later in TargetContentOffsetForProposedContentOffset
				_adjustContentOffset = UpdateWillShiftVisibleItems(CollectionView, updateItems);
			}
		}

		public override CGPoint TargetContentOffsetForProposedContentOffset(CGPoint proposedContentOffset)
		{
			if (_adjustContentOffset)
			{
				_adjustContentOffset = false;

				// PrepareForCollectionViewUpdates detected that an item update was going to shift the viewport
				// and we want to make sure it stays in place
				return proposedContentOffset + ComputeOffsetAdjustment();
			}

			return base.TargetContentOffsetForProposedContentOffset(proposedContentOffset);
		}

		public override void FinalizeCollectionViewUpdates()
		{
			base.FinalizeCollectionViewUpdates();

			if (ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
			{
				ForceScrollToLastItem(CollectionView, _itemsLayout);
			}
		}

		void TrackOffsetAdjustment()
		{
			// Keep track of the previous sizes of the CollectionView content so we can adjust the viewport
			// offsets if we're in ItemsUpdatingScrollMode.KeepItemsInView

			// We keep track of the last two adjustments because the only place we can consistently track this
			// is PrepareLayout, and by the time PrepareLayout has been called, the CollectionViewContentSize
			// has already been updated

			if (_adjustmentSize0.IsEmpty)
			{
				_adjustmentSize0 = CollectionViewContentSize;
			}
			else if (_adjustmentSize1.IsEmpty)
			{
				_adjustmentSize1 = CollectionViewContentSize;
			}
			else
			{
				_adjustmentSize0 = _adjustmentSize1;
				_adjustmentSize1 = CollectionViewContentSize;
			}
		}

		CGSize ComputeOffsetAdjustment()
		{
			return CollectionViewContentSize - _adjustmentSize0;
		}

		static bool UpdateWillShiftVisibleItems(UICollectionView collectionView, UICollectionViewUpdateItem[] updateItems)
		{
			// Find the first visible item
			var firstPath = collectionView.IndexPathsForVisibleItems.FindFirst();

			if (firstPath == null)
			{
				// No visible items to shift
				return false;
			}

			// Determine whether any of the new items will be "before" the first visible item
			foreach (var item in updateItems)
			{
				if (item.UpdateAction == UICollectionUpdateAction.Delete
					|| item.UpdateAction == UICollectionUpdateAction.Insert
					|| item.UpdateAction == UICollectionUpdateAction.Move)
				{
					if (item.IndexPathAfterUpdate == null)
					{
						continue;
					}

					if (item.IndexPathAfterUpdate.IsLessThanOrEqualToPath(firstPath))
					{
						// If any of these items will end up "before" the first visible item, then the items will shift
						return true;
					}
				}
			}

			return false;
		}

		static void ForceScrollToLastItem(UICollectionView collectionView, ItemsLayout itemsLayout)
		{
			var sections = (int)collectionView.NumberOfSections();

			if (sections == 0)
			{
				return;
			}

			for (int section = sections - 1; section >= 0; section--)
			{
				var itemCount = collectionView.NumberOfItemsInSection(section);
				if (itemCount > 0)
				{
					var lastIndexPath = NSIndexPath.FromItemSection(itemCount - 1, section);

					if (itemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
						collectionView.ScrollToItem(lastIndexPath, UICollectionViewScrollPosition.Bottom, true);
					else
						collectionView.ScrollToItem(lastIndexPath, UICollectionViewScrollPosition.Right, true);

					return;
				}
			}
		}

		public override bool ShouldInvalidateLayoutForBoundsChange(CGRect newBounds)
		{
			if (newBounds.Size.IsCloseTo(_currentSize))
			{
				return base.ShouldInvalidateLayoutForBoundsChange(newBounds);
			}

			return UpdateConstraints(CollectionView.AdjustedContentInset.InsetRect(newBounds).Size);
		}

		internal bool TryGetCachedCellSize(object item, out CGSize size)
		{
			if (_cellSizeCache.TryGetValue(item, out CGSize internalSize))
			{
				size = internalSize;
				return true;
			}

			size = CGSize.Empty;
			return false;
		}

		internal void CacheCellSize(object item, CGSize size)
		{
			_cellSizeCache[item] = size;
		}

		internal void ClearCellSizeCache()
		{
			_cellSizeCache.Clear();
		}

		CGSize TryFindEstimatedSize(CGSize existingMeasurement)
		{
			if (CollectionView == null || GetPrototypeForIndexPath == null)
				return existingMeasurement;

			//Since this issue only seems to be reproducible on Horizontal scrolling, we only check for that
			if (ScrollDirection == UICollectionViewScrollDirection.Horizontal)
			{
				return FindEstimatedSizeUsingWidth(existingMeasurement);
			}

			return existingMeasurement;
		}

		CGSize FindEstimatedSizeUsingWidth(CGSize existingMeasurement)
		{
			// TODO: Handle grouping
			var group = 0;
			var collectionViewWidth = CollectionView.Bounds.Width;
			var numberOfItemsInGroup = CollectionView.NumberOfItemsInSection(group);

			// Calculate the number of cells that can fit in the viewport
			var numberOfCellsToCheck = Math.Min((int)(collectionViewWidth / existingMeasurement.Width) + 1, numberOfItemsInGroup);

			// Iterate through the cells and find the one with a wider width
			for (int i = 1; i < numberOfCellsToCheck; i++)
			{
				var indexPath = NSIndexPath.Create(group, i);
				if (GetPrototypeForIndexPath(indexPath) is ItemsViewCell cellAtIndex)
				{
					cellAtIndex.ConstrainTo(ConstrainedDimension);
					var measureCellAtIndex = cellAtIndex.Measure();

					// Check if the cell has a wider width
					if (measureCellAtIndex.Width > existingMeasurement.Width)
					{
						existingMeasurement = measureCellAtIndex;
					}

					// TODO: Cache this cell size
				}
			}

			return existingMeasurement;
		}
	}
}
