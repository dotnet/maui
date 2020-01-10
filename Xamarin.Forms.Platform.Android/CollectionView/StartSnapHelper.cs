#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class StartSnapHelper : EdgeSnapHelper
	{
		public override int[] CalculateDistanceToFinalSnap(RecyclerView.LayoutManager layoutManager, AView targetView)
		{
			return CalculateDistanceToFinalSnap(layoutManager, targetView);
		}

		public override AView FindSnapView(RecyclerView.LayoutManager layoutManager)
		{
			if (!CanSnap)
			{
				return null;
			}

			if (layoutManager.ItemCount == 0)
			{
				return null;
			}

			if (!(layoutManager is LinearLayoutManager linearLayoutManager))
			{
				// Don't snap to anything if this isn't a LinearLayoutManager;
				return null;
			}

			int span = 1;
			if (layoutManager is GridLayoutManager gridLayoutManager)
			{
				span = gridLayoutManager.SpanCount;
			}

			// Find the first visible item; may be only partially on screen
			var firstVisibleItemPosition = linearLayoutManager.FindFirstVisibleItemPosition();

			if (firstVisibleItemPosition == RecyclerView.NoPosition)
			{
				return null;
			}

			// Get the view itself
			var firstView = linearLayoutManager.FindViewByPosition(firstVisibleItemPosition);

			// If the first visible item is in the last row/col of the collection, snap to it
			if(firstVisibleItemPosition >= linearLayoutManager.ItemCount - span)
			{
				return firstView;
			}

			if (IsAtLeastHalfVisible(firstView, layoutManager))
			{
				// If it's halfway in the viewport, snap to it
				return firstView;
			}

			// The first item is mostly off screen, and it's not in the last row/col of the collection
			// So we'll snap to the start of an item in the next row/col
			var nextPos = firstVisibleItemPosition + span;
			if (nextPos >= linearLayoutManager.ItemCount)
			{
				// If we were near the end of the collection, then jumping by "span" may have put us past the end
				nextPos = linearLayoutManager.ItemCount - 1;
			}

			return linearLayoutManager.FindViewByPosition(nextPos);
		}

		protected override int VisiblePortion(AView view, OrientationHelper orientationHelper, bool rtl)
		{
			if (rtl)
			{
				return orientationHelper.TotalSpace - orientationHelper.GetDecoratedStart(view);
			}

			return orientationHelper.GetDecoratedEnd(view);
		}
	}
}