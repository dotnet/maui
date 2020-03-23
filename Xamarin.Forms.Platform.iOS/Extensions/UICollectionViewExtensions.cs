using System;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal static class CollectionViewExtensions
	{
		public static int GetCenteredIndex(this UICollectionView collectionView)
		{
			var centerItemIndex = -1;

			var indexPathsForVisibleItems = collectionView.IndexPathsForVisibleItems.OrderBy(x => x.Row).ToList();

			if (indexPathsForVisibleItems.Count == 0)
				return -1;

			var firstVisibleItemIndex = (int)indexPathsForVisibleItems.First().Item;

			var centerPoint = new CGPoint(collectionView.Center.X + collectionView.ContentOffset.X, collectionView.Center.Y + collectionView.ContentOffset.Y);
			var centerIndexPath = collectionView.IndexPathForItemAtPoint(centerPoint);
			centerItemIndex = centerIndexPath?.Row ?? firstVisibleItemIndex;
			return centerItemIndex;
		}
	}
}
