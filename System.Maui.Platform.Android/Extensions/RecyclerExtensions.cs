#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal static class RecyclerExtensions
	{
		public static int CalculateCenterItemIndex(this RecyclerView recyclerView, int firstVisibleItemIndex, LinearLayoutManager linearLayoutManager)
		{
			// This can happen if a layout pass has not happened yet
			if (firstVisibleItemIndex == -1)
				return firstVisibleItemIndex;

			AView centerView;

			if (linearLayoutManager.Orientation == LinearLayoutManager.Horizontal)
			{
				float centerX = recyclerView.Width / 2;
				centerView = recyclerView.FindChildViewUnder(centerX, recyclerView.Top);
			}
			else
			{
				float centerY = recyclerView.Height / 2;
				centerView = recyclerView.FindChildViewUnder(recyclerView.Left, centerY);
			}

			if (centerView != null)
				return recyclerView.GetChildAdapterPosition(centerView);

			return firstVisibleItemIndex;
		}
	}
}