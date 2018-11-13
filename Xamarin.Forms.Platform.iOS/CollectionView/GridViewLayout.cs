using System;
using System.ComponentModel;
using System.Diagnostics;
using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class GridViewLayout : ItemsViewLayout
	{
		readonly GridItemsLayout _itemsLayout;

		public GridViewLayout(GridItemsLayout itemsLayout) : base(itemsLayout)
		{
			_itemsLayout = itemsLayout;
		}

		protected override void HandlePropertyChanged(PropertyChangedEventArgs propertyChanged)
		{
			if(propertyChanged.Is(GridItemsLayout.SpanProperty))
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
			ConstrainedDimension =
				ScrollDirection == UICollectionViewScrollDirection.Vertical 
					? size.Width / _itemsLayout.Span 
					: size.Height / _itemsLayout.Span;

			// TODO hartez 2018/09/12 14:52:24 We need to truncate the decimal part of ConstrainedDimension
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
	}
}