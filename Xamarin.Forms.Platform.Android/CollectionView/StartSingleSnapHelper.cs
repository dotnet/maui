#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class StartSingleSnapHelper : SingleSnapHelper
	{
		public override int[] CalculateDistanceToFinalSnap(RecyclerView.LayoutManager layoutManager, AView targetView)
		{
			var orientationHelper = CreateOrientationHelper(layoutManager);
			var isHorizontal = layoutManager.CanScrollHorizontally();
			var rtl = isHorizontal && IsLayoutReversed(layoutManager);

			var distance = rtl
				? -orientationHelper.GetDecoratedEnd(targetView)
				: orientationHelper.GetDecoratedStart(targetView);

			return isHorizontal
				? new[] { distance, 1 }
				: new[] { 1, distance };
		}
	}
}