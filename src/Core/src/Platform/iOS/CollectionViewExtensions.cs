using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class CollectionViewExtensions
	{
		// TODO: Change the modifier to public in .NET 11.
		internal static void UpdateIsEnabled(this UICollectionView collectionView, IView view)
		{
			// UICollectionView inherits from UIScrollView (not UIControl), so we set UserInteractionEnabled
			// to properly disable user interactions like scrolling and swiping based on IsEnabled
			collectionView.UserInteractionEnabled = view.IsEnabled;
		}

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
