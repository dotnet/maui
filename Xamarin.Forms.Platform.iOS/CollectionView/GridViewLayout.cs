using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class GridViewLayout : ItemsViewLayout
	{
		readonly GridItemsLayout _itemsLayout;

		public GridViewLayout(GridItemsLayout itemsLayout, ItemSizingStrategy itemSizingStrategy) : base(itemsLayout, itemSizingStrategy)
		{
			_itemsLayout = itemsLayout;
		}

		protected override void HandlePropertyChanged(PropertyChangedEventArgs propertyChanged)
		{
			if(propertyChanged.IsOneOf(GridItemsLayout.SpanProperty, GridItemsLayout.HorizontalItemSpacingProperty, 
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
			var availableSpace = ScrollDirection == UICollectionViewScrollDirection.Vertical
					? size.Width : size.Height;

			var spacing = (nfloat)(ScrollDirection == UICollectionViewScrollDirection.Vertical
					? _itemsLayout.HorizontalItemSpacing
					: _itemsLayout.VerticalItemSpacing);

			spacing = spacing * (_itemsLayout.Span - 1);

			ConstrainedDimension = (availableSpace - spacing) / _itemsLayout.Span;

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

			ConstrainedDimension = (int)ConstrainedDimension;
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

		bool NeedsPartialColumnAdjustment(int section = 0)
		{
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

			if (itemCount < _itemsLayout.Span)
			{
				// If there is just one partial column, no problem; UICollectionViewFlowLayout gets it right
				return false;
			}

			if (itemCount % _itemsLayout.Span == 0)
			{
				// All of the columns are full; the bug only occurs when we have a partial column
				return false;
			}

			return true;
		}
	}
}