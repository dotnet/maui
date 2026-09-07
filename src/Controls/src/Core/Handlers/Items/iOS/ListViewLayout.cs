#nullable disable
using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	[Obsolete("This type is obsolete on iOS and Mac Catalyst. Use the Items2 handler implementation with UIKit.UICollectionViewLayout instead.")]
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