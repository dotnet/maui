using AndroidX.RecyclerView.Widget;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
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