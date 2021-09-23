using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	internal class CvLayout : UICollectionViewFlowLayout
	{
		public CvLayout(VirtualListViewHandler handler) : base()
		{
			Handler = handler;
			isiOS11 = UIDevice.CurrentDevice.CheckSystemVersion(11, 0);
		}

		readonly VirtualListViewHandler Handler;

		readonly bool isiOS11;

		public override UICollectionViewLayoutAttributes LayoutAttributesForItem(NSIndexPath path)
		{
			var layoutAttributes = base.LayoutAttributesForItem(path);

			if (Handler.VirtualView.Orientation == ListOrientation.Vertical)
			{
				var x = SectionInset.Left;

				nfloat width;

				if (isiOS11)
					width = CollectionView.SafeAreaLayoutGuide.LayoutFrame.Width - SectionInset.Left - SectionInset.Right;
				else
					width = CollectionView.Bounds.Width - SectionInset.Left - SectionInset.Right;

				layoutAttributes.Frame = new CGRect(x, layoutAttributes.Frame.Y, width, layoutAttributes.Frame.Height);
			}
			else
			{
				var y = SectionInset.Top;

				nfloat height;

				if (isiOS11)
					height = CollectionView.SafeAreaLayoutGuide.LayoutFrame.Height - SectionInset.Top - SectionInset.Bottom;
				else
					height = CollectionView.Bounds.Height - SectionInset.Top - SectionInset.Bottom;

				layoutAttributes.Frame = new CGRect(layoutAttributes.Frame.X, y, layoutAttributes.Frame.Width, height);
			}

			return layoutAttributes;
		}

		public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect(CGRect rect)
		{
			var layoutAttributesObjects = base.LayoutAttributesForElementsInRect(rect);

			foreach (var layoutAttributes in layoutAttributesObjects)
			{
				var indexPath = layoutAttributes.IndexPath;

				if (layoutAttributes.RepresentedElementCategory == UICollectionElementCategory.Cell)
				{
					var newFrame = LayoutAttributesForItem(indexPath).Frame;

					layoutAttributes.Frame = newFrame;
				}
			}

			return layoutAttributesObjects;
		}
	}
}
