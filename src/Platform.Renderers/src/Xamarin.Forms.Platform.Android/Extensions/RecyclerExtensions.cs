using AndroidX.RecyclerView.Widget;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal static class RecyclerExtensions
	{
		public static int CalculateCenterItemIndex(this RecyclerView recyclerView, int firstVisibleItemIndex, LinearLayoutManager linearLayoutManager, bool lookCenteredOnXAndY)
		{
			// This can happen if a layout pass has not happened yet
			if (firstVisibleItemIndex == -1)
				return firstVisibleItemIndex;

			AView centerView;

			if (linearLayoutManager.Orientation == LinearLayoutManager.Horizontal)
			{
				float centerX = recyclerView.Width / 2;
				float centerY = recyclerView.Top;

				if (lookCenteredOnXAndY)
					centerY = recyclerView.Height / 2;

				centerView = recyclerView.FindChildViewUnder(centerX, centerY);
			}
			else
			{
				float centerY = recyclerView.Height / 2;
				float centerX = recyclerView.Left;

				if (lookCenteredOnXAndY)
					centerX = recyclerView.Width / 2;

				centerView = recyclerView.FindChildViewUnder(centerX, centerY);
			}

			if (centerView != null)
				return recyclerView.GetChildAdapterPosition(centerView);

			return firstVisibleItemIndex;
		}

		public static AView GetCenteredView(this RecyclerView recyclerView)
		{
			if (!(recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager))
				return null;

			AView centeredView;
			if (linearLayoutManager.Orientation == LinearLayoutManager.Horizontal)
			{
				float centerX = linearLayoutManager.Width / 2;
				float centerY = linearLayoutManager.Height / 2;

				centeredView = recyclerView.FindChildViewUnder(centerX, centerY);
			}
			else
			{
				float centerY = linearLayoutManager.Height / 2;
				float centerX = linearLayoutManager.Width / 2;

				centeredView = recyclerView.FindChildViewUnder(centerX, centerY);
			}
			return centeredView;
		}


	}

}