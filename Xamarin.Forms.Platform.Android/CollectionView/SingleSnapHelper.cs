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
	internal class SingleSnapHelper : PagerSnapHelper
	{
		// CurrentTargetPosition will have this value until the user scrolls around
		protected int CurrentTargetPosition = -1;

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

			var targetItemPosition = CurrentTargetPosition;

			if (targetItemPosition != -1)
			{
				return linearLayoutManager.FindViewByPosition(targetItemPosition);
			}

			return null;
		}

		public override int FindTargetSnapPosition(RecyclerView.LayoutManager layoutManager, int velocityX, int velocityY)
		{
			if (CurrentTargetPosition == -1)
			{
				CurrentTargetPosition = base.FindTargetSnapPosition(layoutManager, velocityX, velocityY);
				return CurrentTargetPosition;
			}

			var increment = 1;

			if (layoutManager.CanScrollHorizontally())
			{
				if (velocityX < 0)
				{
					increment = -1;
				}
			}
			else if (layoutManager.CanScrollVertically())
			{
				if (velocityY < 0)
				{
					increment = -1;
				}
			}

			if (IsLayoutReversed(layoutManager))
			{
				increment = increment * -1;
			}

			CurrentTargetPosition = CurrentTargetPosition + increment;

			return CurrentTargetPosition;
		}
	}
}