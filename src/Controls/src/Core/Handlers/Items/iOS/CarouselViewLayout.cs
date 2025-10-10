#nullable disable
using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class CarouselViewLayout : ItemsViewLayout
	{
		readonly WeakReference<CarouselView> _carouselView;
		readonly WeakReference<ItemsLayout> _itemsLayout;
		ItemsLayout ItemsLayout
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
		CGPoint? _pendingOffset;

		public CarouselViewLayout(ItemsLayout itemsLayout, CarouselView carouselView) : base(itemsLayout)
		{
			_carouselView = new(carouselView);
			_itemsLayout = new(itemsLayout);
		}

		public override void ConstrainTo(CGSize size)
		{
			if (!_carouselView.TryGetTarget(out var carouselView))
				return;

			// TODO: Should we scale the items 
			var width = size.Width != 0 ? size.Width - carouselView.PeekAreaInsets.Left - carouselView.PeekAreaInsets.Right : 0;
			var height = size.Height != 0 ? size.Height - carouselView.PeekAreaInsets.Top - carouselView.PeekAreaInsets.Bottom : 0;

			if (ScrollDirection == UICollectionViewScrollDirection.Horizontal)
			{
				ItemSize = new CGSize(width, size.Height);
			}
			else
			{
				ItemSize = new CGSize(size.Width, height);
			}
		}

		public override nfloat GetMinimumInteritemSpacingForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			if (ItemsLayout is LinearItemsLayout linearItemsLayout)
				return (nfloat)linearItemsLayout.ItemSpacing;

			return base.GetMinimumInteritemSpacingForSection(collectionView, layout, section);
		}

		public override UIEdgeInsets GetInsetForSection(UICollectionView collectionView, UICollectionViewLayout layout, nint section)
		{
			if (!_carouselView.TryGetTarget(out var carouselView))
				return default;

			var insets = base.GetInsetForSection(collectionView, layout, section);
			var left = insets.Left + (float)carouselView.PeekAreaInsets.Left;
			var right = insets.Right + (float)carouselView.PeekAreaInsets.Right;
			var top = insets.Top + (float)carouselView.PeekAreaInsets.Top;
			var bottom = insets.Bottom + (float)carouselView.PeekAreaInsets.Bottom;

			return new UIEdgeInsets(top, left, bottom, right);
		}

		public override void PrepareForCollectionViewUpdates(UICollectionViewUpdateItem[] updateItems)
		{
			base.PrepareForCollectionViewUpdates(updateItems);

			if (!_carouselView.TryGetTarget(out var carouselView))
				return;

			// Determine whether the change is a removal 
			if (updateItems.Length == 0 || updateItems[0].UpdateAction != UICollectionUpdateAction.Delete)
			{
				return;
			}

			// Determine whether the removed item is before the current position
			if (updateItems[0].IndexPathBeforeUpdate.Item >= carouselView.Position)
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