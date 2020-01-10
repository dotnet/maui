#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
#else
using Android.Support.V7.Widget;
#endif
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal class CenterSnapHelper : NongreedySnapHelper
	{
		public override AView FindSnapView(RecyclerView.LayoutManager layoutManager)
		{
			if (!CanSnap)
			{
				return null;
			}

			return base.FindSnapView(layoutManager);
		}
	}
}