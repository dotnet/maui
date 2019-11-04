using System;
using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class CarouselViewLayout : ItemsViewLayout
	{
		readonly CarouselView _carouselView;
		readonly ItemsLayout _itemsLayout;

		public CarouselViewLayout(ItemsLayout itemsLayout, ItemSizingStrategy itemSizingStrategy, CarouselView carouselView) : base(itemsLayout, itemSizingStrategy)
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
			var aspectRatio = size.Width / size.Height;
			var numberOfVisibleItems = _carouselView.NumberOfSideItems * 2 + 1;
			var width = (size.Width - _carouselView.PeekAreaInsets.Left - _carouselView.PeekAreaInsets.Right) / numberOfVisibleItems;
			var height = (size.Height - _carouselView.PeekAreaInsets.Top - _carouselView.PeekAreaInsets.Bottom) / numberOfVisibleItems;

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

			// We give some insets so the user can scroll to the first and last item
			if (_carouselView.NumberOfSideItems > 0)
			{
				if (ScrollDirection == UICollectionViewScrollDirection.Horizontal)
				{
					left += ItemSize.Width;
					right += ItemSize.Width;

					return new UIEdgeInsets(insets.Top, left, insets.Bottom, right);
				}

				return new UIEdgeInsets(ItemSize.Height, insets.Left, ItemSize.Height, insets.Right);
			}

			return new UIEdgeInsets(top, left, bottom, right);
		}
	}
}