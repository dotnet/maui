#nullable disable
using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class GridViewLayout : ItemsViewLayout
	{
		readonly  WeakReference<GridItemsLayout> _itemsLayout  = new WeakReference<GridItemsLayout>(null);

		GridItemsLayout ItemsLayout
		{
			get
			{
				_itemsLayout.TryGetTarget(out var itemsLayout);
				return itemsLayout;
			}
			set
			{
				_itemsLayout.SetTarget(value);
			}
		}

		public GridViewLayout(GridItemsLayout itemsLayout, ItemSizingStrategy itemSizingStrategy) : base(itemsLayout, itemSizingStrategy)
		{
			ItemsLayout = itemsLayout;
		}

		protected override void HandlePropertyChanged(PropertyChangedEventArgs propertyChanged)
		{
			if (CollectionView != null && propertyChanged.IsOneOf(GridItemsLayout.SpanProperty, GridItemsLayout.HorizontalItemSpacingProperty,
				GridItemsLayout.VerticalItemSpacingProperty))
			{
				// Update the constraints; ConstrainTo will pick up the new span
				ConstrainTo(CollectionView.Frame.Size);

				// And force the UICollectionView to reload everything with the new span
				CollectionView.ReloadData();
			}

			base.HandlePropertyChanged(propertyChanged);
		}

		public override void ConstrainTo(CGSize size)
		{
			var itemsLayout = ItemsLayout;
			var availableSpace = ScrollDirection == UICollectionViewScrollDirection.Vertical
					? size.Width : size.Height;

			var spacing = (nfloat)(ScrollDirection == UICollectionViewScrollDirection.Vertical
					? itemsLayout.HorizontalItemSpacing
					: itemsLayout.VerticalItemSpacing);

			spacing = ReduceSpacingToFitIfNeeded(availableSpace, spacing, itemsLayout.Span);

			spacing *= (itemsLayout.Span - 1);

			ConstrainedDimension = (availableSpace - spacing) / itemsLayout.Span;

			// We need to truncate the decimal part of ConstrainedDimension
			// or we occasionally run into situations where the rows/columns don't fit	
			// But this can run into situations where we have an extra gap because we're cutting off too much
			// and we have a small gap; need to determine where the cut-off is that leads to layout dropping a whole row/column
			// and see if we can adjust for that

			// E.G.: We have a CollectionView that's 532 units tall, and we have a span of 3
			// So we end up with ConstrainedDimension of 177.3333333333333...
			// When UICollectionView lays that out, it can't fit all the rows in so it just gives us two rows.
			// Truncating to 177 means the rows fit, but there's a very slight gap
			// There may not be anything we can do about this.

			// Possibly the solution is to round to the tenths or hundredths place, we should look into that. 
			// But for the moment, we need a special case for dimensions < 1, because upon transition from invisible to visible,
			// Forms will briefly layout the CollectionView at a size of 1,1. For a spanned collectionview, that means we 
			// need to accept a constrained dimension of 1/span. If we don't, autolayout will start throwing a flurry of 
			// exceptions (which we can't catch) and either crash the app or spin until we kill the app. 
			if (ConstrainedDimension > 1)
			{
				ConstrainedDimension = (int)ConstrainedDimension;
			}

			DetermineCellSize();
		}

		/* `CollectionViewContentSize` and `LayoutAttributesForElementsInRect` are overridden here to work around what 
		 * appears to be a bug in the UICollectionViewFlowLayout implementation: for horizontally scrolling grid
		 * layouts with auto-sized cells, trailing items which don't fill out a column are never displayed. 
		 * For example, with a span of 3 and either 4 or 5 items, the resulting layout looks like
		 *
		 * 		Item1 
		 * 		Item2
		 * 		Item3
		 * 
		 * But with 6 items, it looks like
		 * 
		 * 		Item1 Item4
		 * 		Item2 Item5
		 * 		Item3 Item6
		 * 
		 * IOW, if there are not enough items to fill out the last column, the last column is ignored.
		 * 
		 * These overrides detect and correct that situation.	 
		 */

		public override CGSize CollectionViewContentSize
		{
			get
			{
				if (!NeedsPartialColumnAdjustment())
				{
					return base.CollectionViewContentSize;
				}

				var contentSize = base.CollectionViewContentSize;

				// Add space for the missing column at the end
				var correctedSize = new CGSize(contentSize.Width + EstimatedItemSize.Width, contentSize.Height);

				return correctedSize;
			}
		}

		public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect(CGRect rect)
		{
			var layoutAttributesForRectElements = base.LayoutAttributesForElementsInRect(rect);

			if (ScrollDirection == UICollectionViewScrollDirection.Vertical && NeedsSingleItemHorizontalAlignmentAdjustment(layoutAttributesForRectElements))
			{
				// If there's exactly one item in a vertically scrolling grid, for some reason UICollectionViewFlowLayout
				// tries to center it. This corrects that issue.
				var currentFrame = layoutAttributesForRectElements[0].Frame;
				var newFrame = new CGRect(CollectionView.Frame.Left + CollectionView.ContentInset.Right,
				currentFrame.Top, currentFrame.Width, currentFrame.Height);
				layoutAttributesForRectElements[0].Frame = newFrame;
			}
			else if (ScrollDirection == UICollectionViewScrollDirection.Horizontal && layoutAttributesForRectElements.Length == 1)
			{
				// Adjusts alignment for a single item in a horizontally scrolling grid to prevent centering the item
				var currentFrame = layoutAttributesForRectElements[0].Frame;
				var newFrame = new CGRect(currentFrame.Left, CollectionView.Frame.Top + CollectionView.ContentInset.Top,
				currentFrame.Width, currentFrame.Height);
				layoutAttributesForRectElements[0].Frame = newFrame;
			}

			if (!NeedsPartialColumnAdjustment())
			{
				return layoutAttributesForRectElements;
			}

			// When we implement Groups, we'll have to iterate over all of them to adjust and this will
			// be a lot more complicated. But until then, we only have to worry about section 0
			var section = 0;

			var itemCount = CollectionView.NumberOfItemsInSection(section);

			if (layoutAttributesForRectElements.Length == itemCount)
			{
				return layoutAttributesForRectElements;
			}

			var layoutAttributesForAllCells = new UICollectionViewLayoutAttributes[itemCount];

			layoutAttributesForRectElements.CopyTo(layoutAttributesForAllCells, 0);

			for (int i = layoutAttributesForRectElements.Length; i < layoutAttributesForAllCells.Length; i++)
			{
				layoutAttributesForAllCells[i] = LayoutAttributesForItem(NSIndexPath.FromItemSection(i, section));
			}

			return layoutAttributesForAllCells;
		}

		public override UICollectionViewLayoutInvalidationContext GetInvalidationContext(UICollectionViewLayoutAttributes preferredAttributes, UICollectionViewLayoutAttributes originalAttributes)
		{
			var invalidationContext = base.GetInvalidationContext(preferredAttributes, originalAttributes);

			if (invalidationContext.InvalidatedItemIndexPaths == null)
			{
				return invalidationContext;
			}

			if (invalidationContext.InvalidatedItemIndexPaths.Length == 0)
			{
				return invalidationContext;
			}

			if (ScrollDirection == UICollectionViewScrollDirection.Horizontal
				&& preferredAttributes.Frame.Width - originalAttributes.Frame.Width > 1)
			{
				// If this is a horizontal grid and we're laying out or adjusting a cell 
				// and we've decided it needs to be wider, then this might throw off the alignment of
				// any cells above it in the layout. We'll need to recenter those cells
				CenterAlignCellsInColumn(preferredAttributes);

				// (Technically speaking, we _could_ simply add the cells above the current cell to the invalidationContext;
				// after invalidation, they would be realigned correctly. But doing that causes subsequent calls to 
				// GetInvalidationContext to happen every time a new column needs layout, and those calls will include
				// _every single subsequent cell in the collection_ in the invalidation list. For very large collections,
				// this gets really slow and the scrolling becomes jerky. This one-time realignment is much faster.
			}

			return invalidationContext;
		}

		public override nfloat GetMinimumInteritemSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			var itemsLayout = ItemsLayout;
			var requestedSpacing = ScrollDirection == UICollectionViewScrollDirection.Horizontal
				? (nfloat)itemsLayout.VerticalItemSpacing
				: (nfloat)itemsLayout.HorizontalItemSpacing;

			var availableSpace = ScrollDirection == UICollectionViewScrollDirection.Horizontal
				? collectionView.Frame.Height
				: collectionView.Frame.Width;

			return ReduceSpacingToFitIfNeeded(availableSpace, requestedSpacing, itemsLayout.Span);
		}

		void CenterAlignCellsInColumn(UICollectionViewLayoutAttributes preferredAttributes)
		{
			// Determine the set of cells above this one
			var index = preferredAttributes.IndexPath;
			var span = ItemsLayout.Span;

			var column = index.Item / span;
			var start = (int)column * span;

			// If this is the first cell in the column, we don't need to adjust
			if (index.Item > start)
			{
				var currentCenter = preferredAttributes.Frame.GetMidX();

				// Work our way through the column
				for (int n = start; n < index.Item; n++)
				{
					// Get the layout attributes for each cell
					var path = NSIndexPath.FromItemSection(n, index.Section);
					var attr = LayoutAttributesForItem(path);

					// And see if the midpoints line up with the new layout attributes for the current cell
					var center = attr.Frame.GetMidX();

					if (currentCenter - center > 1)
					{
						// If the midpoints don't line up (withing a tolerance), adjust the cell's frame
						var cell = CollectionView.CellForItem(path);
						cell.Frame = new CGRect(currentCenter - cell.Frame.Width / 2, cell.Frame.Top, cell.Frame.Width, cell.Frame.Height);
					}
				}
			}
		}

		bool NeedsSingleItemHorizontalAlignmentAdjustment(UICollectionViewLayoutAttributes[] layoutAttributesForRectElements)
		{
			if (layoutAttributesForRectElements.Length != 1)
			{
				return false;
			}

			// We need to determine whether this 'if' statement is needed, as its relevance is currently uncertain.
			if (layoutAttributesForRectElements[0].Frame.Top != CollectionView.Frame.Top)
			{
				return false;
			}

			return true;
		}

		bool NeedsPartialColumnAdjustment(int section = 0)
		{
			var itemsLayout = ItemsLayout;
			if (ScrollDirection == UICollectionViewScrollDirection.Vertical)
			{
				// The bug only occurs with Horizontal scrolling
				return false;
			}

			if (CollectionView.NumberOfSections() == 0)
			{
				// And it only happens if there are items
				return false;
			}

			if (EstimatedItemSize.IsEmpty)
			{
				// The bug only occurs when using Autolayout; with a set ItemSize, we don't have to worry about it
				return false;
			}

			if (CollectionView.NumberOfSections() == 0)
				return false;

			var itemCount = CollectionView.NumberOfItemsInSection(section);

			if (itemCount < itemsLayout.Span)
			{
				// If there is just one partial column, no problem; UICollectionViewFlowLayout gets it right
				return false;
			}

			if (itemCount % itemsLayout.Span == 0)
			{
				// All of the columns are full; the bug only occurs when we have a partial column
				return false;
			}

			return true;
		}

		static nfloat ReduceSpacingToFitIfNeeded(nfloat available, nfloat requestedSpacing, int span)
		{
			if (span == 1)
			{
				return requestedSpacing;
			}

			var maxSpacing = (available - span) / (span - 1);

			if (maxSpacing < 0)
			{
				return 0;
			}

			return (nfloat)Math.Min(requestedSpacing, maxSpacing);
		}
	}
}