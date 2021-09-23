using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	internal class CvDelegate : UICollectionViewDelegateFlowLayout
	{

		public CvDelegate(VirtualListViewHandler handler)
			: base()
		{
			Handler = handler;
		}

		internal readonly VirtualListViewHandler Handler;

		public Action<nfloat, nfloat> ScrollHandler { get; set; }

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
			=> HandleSelection(collectionView, indexPath, true);

		public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
			=> HandleSelection(collectionView, indexPath, false);

		void HandleSelection(UICollectionView collectionView, NSIndexPath indexPath, bool selected)
		{
			var real = Handler?.PositionalViewSelector?.GetRealIndexPath(indexPath.Section, (int)indexPath.Item);

			if (real != default)
			{
				var realSectionIndex = real?.realSectionIndex ?? -1;
				var realItemIndex = real?.realItemIndex ?? -1;

				if (realItemIndex < 0 || realSectionIndex < 0)
					return;

				if (selected)
					Handler?.VirtualView?.SetSelected(new ItemPosition(realSectionIndex, realItemIndex));
				else
					Handler?.VirtualView?.SetDeselected(new ItemPosition(realSectionIndex, realItemIndex));

				var cell = Handler?.GetCell(indexPath);

				if (cell?.PositionInfo != null)
					cell.PositionInfo.IsSelected = selected;
			}
		}

		public override void Scrolled(UIScrollView scrollView)
			=> ScrollHandler?.Invoke(scrollView.ContentOffset.X, scrollView.ContentOffset.Y);

		public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
			=> IsRealItem(indexPath);

		public override bool ShouldDeselectItem(UICollectionView collectionView, NSIndexPath indexPath)
			=> IsRealItem(indexPath);

		bool IsRealItem(NSIndexPath indexPath)
		{
			var real = Handler?.PositionalViewSelector?.GetRealIndexPath(indexPath.Section, (int)indexPath.Item);

			if (real != default)
			{
				if ((real?.realItemIndex ?? -1) < 0 || (real?.realSectionIndex ?? -1) < 0)
					return false;
			}

			return true;
		}
	}
}
