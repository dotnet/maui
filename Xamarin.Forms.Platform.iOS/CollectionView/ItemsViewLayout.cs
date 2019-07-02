using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	public abstract class ItemsViewLayout : UICollectionViewFlowLayout
	{
		readonly ItemsLayout _itemsLayout;
		bool _determiningCellSize;
		bool _disposed;

		protected ItemsViewLayout(ItemsLayout itemsLayout)
		{
			Xamarin.Forms.CollectionView.VerifyCollectionViewFlagEnabled(nameof(ItemsViewLayout));

			_itemsLayout = itemsLayout;
			_itemsLayout.PropertyChanged += LayoutOnPropertyChanged;

			var scrollDirection = itemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
				? UICollectionViewScrollDirection.Horizontal
				: UICollectionViewScrollDirection.Vertical;

			Initialize(scrollDirection);

			if (Forms.IsiOS11OrNewer)
			{
				// `ContentInset` is actually the default value, but I'm leaving this here as a note to
				// future maintainers; it's likely that someone will want a Platform Specific to change this behavior
				// (Setting it to `SafeArea` lets you do the thing where the header/footer of your UICollectionView
				// fills the screen width in landscape while your items are automatically shifted to avoid the notch)
				SectionInsetReference = UICollectionViewFlowLayoutSectionInsetReference.ContentInset;
			}
		}

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
			if (propertyChanged.IsOneOf(ListItemsLayout.ItemSpacingProperty,
				GridItemsLayout.HorizontalItemSpacingProperty, GridItemsLayout.VerticalItemSpacingProperty))
			{
				UpdateItemSpacing();
			}
		}

		public nfloat ConstrainedDimension { get; set; }

		public Func<UICollectionViewCell> GetPrototype { get; set; }

		internal ItemSizingStrategy ItemSizingStrategy { get; set; }

		public abstract void ConstrainTo(CGSize size);

		public virtual UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout,
			nint section)
		{
			return UIEdgeInsets.Zero;
		}

		public virtual nfloat GetMinimumInteritemSpacingForSection(UICollectionView collectionView,
			UICollectionViewLayout layout, nint section)
		{
			if (_itemsLayout is GridItemsLayout gridItemsLayout)
			{
				if (ScrollDirection == UICollectionViewScrollDirection.Horizontal)
				{
					return (nfloat)gridItemsLayout.VerticalItemSpacing;
				}

				return (nfloat)gridItemsLayout.HorizontalItemSpacing;
			}

			return (nfloat)0.0;
		}

		public virtual nfloat GetMinimumLineSpacingForSection(UICollectionView collectionView,
			UICollectionViewLayout layout, nint section)
		{
			if (_itemsLayout is ListItemsLayout listViewLayout)
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
			if (_determiningCellSize)
			{
				return;
			}

			if (EstimatedItemSize == CGSize.Empty)
			{
				cell.ConstrainTo(ItemSize);
			}
			else
			{
				cell.ConstrainTo(ConstrainedDimension);
			}
		}

		public override bool ShouldInvalidateLayoutForBoundsChange(CGRect newBounds)
		{
			var shouldInvalidate = base.ShouldInvalidateLayoutForBoundsChange(newBounds);

			if (shouldInvalidate)
			{
				UpdateConstraints(newBounds.Size);
			}

			return shouldInvalidate;
		}

		public override bool ShouldInvalidateLayout(UICollectionViewLayoutAttributes preferredAttributes, UICollectionViewLayoutAttributes originalAttributes)
		{
			if (ItemSizingStrategy == ItemSizingStrategy.MeasureAllItems)
			{
				if (preferredAttributes.Bounds != originalAttributes.Bounds)
				{
					return true;
				}
			}

			if (Forms.IsiOS11OrNewer)
			{
				return base.ShouldInvalidateLayout(preferredAttributes, originalAttributes);
			}

			// For iOS 10 and lower, we have to invalidate on header/footer changes here; otherwise, all of the 
			// headers and footers will draw on top of one another
			if (preferredAttributes.RepresentedElementKind == UICollectionElementKindSectionKey.Header
				|| preferredAttributes.RepresentedElementKind == UICollectionElementKindSectionKey.Footer)
			{
				return true;
			}

			return base.ShouldInvalidateLayout(preferredAttributes, originalAttributes);
		}

		protected void DetermineCellSize()
		{
			if (GetPrototype == null)
			{
				return;
			}

			_determiningCellSize = true;

			// We set the EstimatedItemSize here for two reasons:
			// 1. If we don't set it, iOS versions below 10 will crash
			// 2. If GetPrototype() cannot return a cell because the items source is empty, we need to have
			//		an estimate set so that when a cell _does_ become available (i.e., when the items source
			//		has at least one item), Autolayout will kick in for the first cell and size it correctly
			// If GetPrototype() _can_ return a cell, this estimate will be updated once that cell is measured
			EstimatedItemSize = new CGSize(1, 1);

			if (!(GetPrototype() is ItemsViewCell prototype))
			{
				_determiningCellSize = false;
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
				EstimatedItemSize = measure;
			}

			_determiningCellSize = false;
		}

		bool ConstraintsMatchScrollDirection(CGSize size)
		{
			if (ScrollDirection == UICollectionViewScrollDirection.Vertical)
			{
				return ConstrainedDimension == size.Width;
			}

			return ConstrainedDimension == size.Height;
		}

		void Initialize(UICollectionViewScrollDirection scrollDirection)
		{
			ScrollDirection = scrollDirection;
		}

		internal void UpdateCellConstraints()
		{
			var cells = CollectionView.VisibleCells;

			for (int n = 0; n < cells.Length; n++)
			{
				if (cells[n] is ItemsViewCell constrainedCell)
				{
					PrepareCellForLayout(constrainedCell);
				}
			}
		}

		void UpdateConstraints(CGSize size)
		{
			if (ConstraintsMatchScrollDirection(size))
			{
				return;
			}

			ConstrainTo(size);
			UpdateCellConstraints();
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
			if (Forms.IsiOS11OrNewer)
			{
				return base.GetInvalidationContext(preferredAttributes, originalAttributes);
			}

			var indexPath = preferredAttributes.IndexPath;

			try
			{
				UICollectionViewLayoutInvalidationContext invalidationContext =
					base.GetInvalidationContext(preferredAttributes, originalAttributes);

				// Ensure that if this invalidation was triggered by header/footer changes, the header/footer
				// are being invalidated
				if (preferredAttributes.RepresentedElementKind == UICollectionElementKindSectionKey.Header)
				{
					invalidationContext.InvalidateSupplementaryElements(UICollectionElementKindSectionKey.Header,
						new[] { indexPath });
				}
				else if (preferredAttributes.RepresentedElementKind == UICollectionElementKindSectionKey.Footer)
				{
					invalidationContext.InvalidateSupplementaryElements(UICollectionElementKindSectionKey.Footer,
						new[] { indexPath });
				}

				return invalidationContext;
			}
			catch (MonoTouchException)
			{
				// This happens on iOS 10 if we have any empty groups in our ItemsSource. Catching here and 
				// returning a UICollectionViewFlowLayoutInvalidationContext means that the application does not
				// crash, though any group headers/footers will initially draw in the wrong location. It's possible to 
				// work around this problem by forcing a full layout update after the headers/footers have been 
				// drawn in the wrong places
			}

			return new UICollectionViewFlowLayoutInvalidationContext();
		}

		public override UICollectionViewLayoutAttributes LayoutAttributesForSupplementaryView(NSString kind, NSIndexPath indexPath)
		{
			if (Forms.IsiOS11OrNewer)
			{
				return base.LayoutAttributesForSupplementaryView(kind, indexPath);
			}

			// iOS 10 and lower doesn't create these and will throw an exception in GetViewForSupplementaryElement 
			// without them, so we need to do it manually here
			return UICollectionViewLayoutAttributes.CreateForSupplementaryView(kind, indexPath);
		}
	}
}
