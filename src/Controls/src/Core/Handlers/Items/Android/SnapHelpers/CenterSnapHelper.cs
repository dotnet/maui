#nullable disable
using AndroidX.RecyclerView.Widget;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
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
