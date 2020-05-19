using System;
using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class CarouselViewLayout : ItemsViewLayout
	{
		readonly CarouselView _carouselView;
		readonly ItemsLayout _itemsLayout;
		CGPoint? _pendingOffset;

		public CarouselViewLayout(ItemsLayout itemsLayout, CarouselView carouselView) : base(itemsLayout)
		{
			_carouselView = carouselView;
			_itemsLayout = itemsLayout;
		}

		public override bool ShouldInvalidateLayoutForBoundsChange(CGRect newBounds)
		{
			return false;
		}

		public override void ConstrainTo(CGSize size)
		{
			//TODO: Should we scale the items 
			var width = size.Width - _carouselView.PeekAreaInsets.Left - _carouselView.PeekAreaInsets.Right;
			var height = size.Height - _carouselView.PeekAreaInsets.Top - _carouselView.PeekAreaInsets.Bottom;

			if (ScrollDirection == UICollectionViewScrollDirection.Horizontal)
			{
				ItemSize = new CGSize(width, size.Height);
			}
			else
			{
				ItemSize = new CGSize(size.Width, height);
			}
		}

		internal void UpdateConstraints(CGSize size)
		{
			ConstrainTo(size);
			UpdateCellConstraints();
		}

		public override nfloat GetMinimumInteritemSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			if (_itemsLayout is LinearItemsLayout linearItemsLayout)
				return (nfloat)linearItemsLayout.ItemSpacing;

			return base.GetMinimumInteritemSpacingForSection(collectionView, layout, section);
		}

		public override UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			var insets = base.GetInsetForSection(collectionView, layout, section);
			var left = insets.Left + (float)_carouselView.PeekAreaInsets.Left;
			var right = insets.Right + (float)_carouselView.PeekAreaInsets.Right;
			var top = insets.Top + (float)_carouselView.PeekAreaInsets.Top;
			var bottom = insets.Bottom + (float)_carouselView.PeekAreaInsets.Bottom;

			return new UIEdgeInsets(top, left, bottom, right);
		}

		public override void PrepareForCollectionViewUpdates(UICollectionViewUpdateItem[] updateItems)
		{
			base.PrepareForCollectionViewUpdates(updateItems);

			// Determine whether the change is a removal 
			if (updateItems.Length == 0 || updateItems[0].UpdateAction != UICollectionUpdateAction.Delete)
			{
				return;
			}

			// Determine whether the removed item is before the current position
			if (updateItems[0].IndexPathBeforeUpdate.Item >= _carouselView.Position)
			{
				return;
			}

			// If an earlier item is being removed, we'll need to adjust the content offset to account for 
			// the now mising item. Calculate what the new offset will be and store that.
			var currentOffset = CollectionView.ContentOffset;
			if (ScrollDirection == UICollectionViewScrollDirection.Horizontal)
				_pendingOffset = new CGPoint(currentOffset.X - ItemSize.Width, currentOffset.Y);
			else
				_pendingOffset = new CGPoint(currentOffset.X, currentOffset.Y - ItemSize.Height);
		}

		public override void FinalizeCollectionViewUpdates()
		{
			base.FinalizeCollectionViewUpdates();

			// Adjust the offset if necessary (e.g., if we've removed items from earlier in the carousel)
			if (_pendingOffset.HasValue)
			{
				CollectionView.SetContentOffset(_pendingOffset.Value, false);
				_pendingOffset = null;
			}
		}
	}
}