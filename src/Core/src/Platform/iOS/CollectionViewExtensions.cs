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

			// In CV2 the scroll indicators are managed by an internal UIScrollView.
			if (collectionView.CollectionViewLayout is UICollectionViewCompositionalLayout)
			{
				UpdateInternalScrollView(collectionView, true);
			}
		}

		public static void UpdateHorizontalScrollBarVisibility(this UICollectionView collectionView, ScrollBarVisibility scrollBarVisibility)
		{
			collectionView.ShowsHorizontalScrollIndicator = scrollBarVisibility == ScrollBarVisibility.Always || scrollBarVisibility == ScrollBarVisibility.Default;

			// In CV2 the scroll indicators are managed by an internal UIScrollView.
			if (collectionView.CollectionViewLayout is UICollectionViewCompositionalLayout)
			{
				UpdateInternalScrollView(collectionView, false);
			}
		}

		static void UpdateInternalScrollView(UICollectionView collectionView, bool isVertical)
		{
			if (TryApplyToInternalScrollView(collectionView, isVertical))
			{
				return;
			}

			// One deferred retry only; if the scroll view still doesn't exist at that point,
			// the setting is not applied (acceptable: layout should always be complete by then).
			collectionView.BeginInvokeOnMainThread(() =>
			{
				TryApplyToInternalScrollView(collectionView, isVertical);
			});
		}

		// NOTE: This relies on UICollectionViewCompositionalLayout's internal implementation —
		// it creates a UIScrollView subview to host scroll indicators. If this changes in a
		// future iOS release, this method will return false and scrollbar visibility will fall
		// back to the outer UICollectionView only.
		static bool TryApplyToInternalScrollView(UICollectionView collectionView, bool isVertical)
		{
			foreach (var subview in collectionView.Subviews)
			{
				if (subview is UIScrollView scrollView && scrollView != collectionView)
				{
					if (isVertical)
					{
						scrollView.ShowsVerticalScrollIndicator = collectionView.ShowsVerticalScrollIndicator;
					}
					else
					{
						scrollView.ShowsHorizontalScrollIndicator = collectionView.ShowsHorizontalScrollIndicator;
					}
					return true;
				}
			}

			return false;
		}
	}
}
