using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class CollectionViewExtensions
	{
		public static void UpdateVerticalScrollBarVisibility(this UICollectionView collectionView, ScrollBarVisibility scrollBarVisibility)
		{
			collectionView.ShowsVerticalScrollIndicator = scrollBarVisibility == ScrollBarVisibility.Always || scrollBarVisibility == ScrollBarVisibility.Default;
		}

		public static void UpdateHorizontalScrollBarVisibility(this UICollectionView collectionView, ScrollBarVisibility scrollBarVisibility)
		{
			collectionView.ShowsHorizontalScrollIndicator = scrollBarVisibility == ScrollBarVisibility.Always || scrollBarVisibility == ScrollBarVisibility.Default;
		}
	}
}
