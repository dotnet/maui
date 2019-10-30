using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ListViewLayout : ItemsViewLayout
	{
		public ListViewLayout(LinearItemsLayout itemsLayout, ItemSizingStrategy itemSizingStrategy) : base(itemsLayout, itemSizingStrategy)
		{
		}

		public override void ConstrainTo(CGSize size)
		{
			ConstrainedDimension =
				ScrollDirection == UICollectionViewScrollDirection.Vertical ? size.Width : size.Height;
			DetermineCellSize();
		}
	}
}