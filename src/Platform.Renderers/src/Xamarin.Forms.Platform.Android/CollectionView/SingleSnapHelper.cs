using AndroidX.RecyclerView.Widget;
using ALayoutDirection = Android.Views.LayoutDirection;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class SingleSnapHelper : PagerSnapHelper
	{
		// CurrentTargetPosition will have this value until the user scrolls around
		// Or if we request a ScrollTo position
		// Or if the number of items on the ItemsSource changes
		protected int CurrentTargetPosition = -1;
		int _previousCount = 0;

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
			var itemCount = layoutManager.ItemCount;

			//reset CurrentTargetPosition if ItemCount Changed is requested
			//We could be adding a item before the CurrentTargetPosition
			if (_previousCount != itemCount)
			{
				CurrentTargetPosition = -1;
				_previousCount = itemCount;
			}

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
				increment *= -1;
			}

			if (CurrentTargetPosition == itemCount - 1 && increment == 1)
			{
				return CurrentTargetPosition;
			}

			CurrentTargetPosition = CurrentTargetPosition + increment;

			return CurrentTargetPosition;
		}

		internal void ResetCurrentTargetPosition()
		{
			//reset CurrentTargetPosition if ScrollTo is requested
			CurrentTargetPosition = -1;
		}
	}
}