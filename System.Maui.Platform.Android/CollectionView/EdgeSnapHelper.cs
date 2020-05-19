#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using AView = Android.Views.View;
using ALayoutDirection = Android.Views.LayoutDirection;

namespace Xamarin.Forms.Platform.Android
{
	internal abstract class EdgeSnapHelper : NongreedySnapHelper
	{
		protected static OrientationHelper CreateOrientationHelper(RecyclerView.LayoutManager layoutManager)
		{
			return layoutManager.CanScrollHorizontally()
				? OrientationHelper.CreateHorizontalHelper(layoutManager)
				: OrientationHelper.CreateVerticalHelper(layoutManager);
		}

		protected static bool IsLayoutReversed(RecyclerView.LayoutManager layoutManager)
		{
			if (layoutManager.LayoutDirection == (int)(ALayoutDirection.Rtl))
				return true;

			if (layoutManager is LinearLayoutManager linearLayoutManager)
			{
				return linearLayoutManager.ReverseLayout;
			}

			return false;
		}

		protected int[] CalculateDistanceToFinalSnap(RecyclerView.LayoutManager layoutManager, AView targetView,
			int direction = 1)
		{
			var orientationHelper = CreateOrientationHelper(layoutManager);
			var isHorizontal = layoutManager.CanScrollHorizontally();
			var rtl = isHorizontal && IsLayoutReversed(layoutManager);

			var size = orientationHelper.GetDecoratedMeasurement(targetView);

			var hiddenPortion = size - VisiblePortion(targetView, orientationHelper, rtl);

			var distance = (rtl ? hiddenPortion : -hiddenPortion) * direction;

			return isHorizontal
				? new[] { distance, 1 }
				: new[] { 1, distance };
		}

		protected bool IsAtLeastHalfVisible(AView view, RecyclerView.LayoutManager layoutManager)
		{
			var orientationHelper = CreateOrientationHelper(layoutManager);
			var reversed = IsLayoutReversed(layoutManager);
			var isHorizontal = layoutManager.CanScrollHorizontally();

			// Find the size of the view (including margins, etc.)
			var size = orientationHelper.GetDecoratedMeasurement(view);

			var portionInViewPort = VisiblePortion(view, orientationHelper, reversed && isHorizontal);

			// Is the first visible view at least halfway on screen?
			return portionInViewPort >= size / 2;
		}

		protected abstract int VisiblePortion(AView view, OrientationHelper orientationHelper, bool rtl);
	}
}