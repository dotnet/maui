using Android.Support.V7.Widget;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class StartPagerSnapHelper : PagerSnapHelper
	{
		protected static OrientationHelper CreateOrientationHelper(RecyclerView.LayoutManager layoutManager)
		{
			return layoutManager.CanScrollHorizontally()
				? OrientationHelper.CreateHorizontalHelper(layoutManager)
				: OrientationHelper.CreateVerticalHelper(layoutManager);
		}

		protected static bool IsLayoutReversed(RecyclerView.LayoutManager layoutManager)
		{
			if (layoutManager is LinearLayoutManager linearLayoutManager)
			{
				return linearLayoutManager.ReverseLayout;
			}

			return false;
		}

		public override int FindTargetSnapPosition(RecyclerView.LayoutManager layoutManager, int velocityX, int velocityY)
		{
			var x = base.FindTargetSnapPosition(layoutManager, velocityX, velocityY);
			return x;
		}

		public override AView FindSnapView(RecyclerView.LayoutManager layoutManager)
		{
			if (layoutManager.ItemCount == 0)
			{
				return null;
			}

			if (!(layoutManager is LinearLayoutManager linearLayoutManager))
			{
				// Don't snap to anything if this isn't a LinearLayoutManager;
				return null;
			}

			// Find the first fully visible item
			var firstVisibleItemPosition = linearLayoutManager.FindFirstCompletelyVisibleItemPosition();

			if (firstVisibleItemPosition == RecyclerView.NoPosition)
			{
				// If there are no fully visible items, drop back to default PagerSnapHelper behavior
				return base.FindSnapView(layoutManager);
			}

			// Return the view to snap
			return linearLayoutManager.FindViewByPosition(firstVisibleItemPosition);
		}

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