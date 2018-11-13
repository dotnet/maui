using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class ListViewLayout : ItemsViewLayout
	{
		public ListViewLayout(ListItemsLayout itemsLayout): base(itemsLayout)
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