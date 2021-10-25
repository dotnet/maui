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
			var info = Handler?.PositionalViewSelector?.GetInfo(indexPath.Section, (int)indexPath.Item);

			if ((info?.Kind ?? PositionKind.Header) == PositionKind.Item)
			{
				var itemPos = new ItemPosition(info.SectionIndex, info.ItemIndex);

				if (selected)
					Handler?.VirtualView?.SetSelected(itemPos);
				else
					Handler?.VirtualView?.SetDeselected(itemPos);

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
			var info = Handler?.PositionalViewSelector?.GetInfo(indexPath.Section, (int)indexPath.Item);
			return (info?.Kind ?? PositionKind.Header) == PositionKind.Item;
		}
	}
}
