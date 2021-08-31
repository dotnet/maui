using Android.Widget;
using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	internal static class FlowDirectionExtensions
	{
		internal static FlowDirection ToFlowDirection(this ALayoutDirection direction)
		{
			switch (direction)
			{
				case ALayoutDirection.Ltr:
					return FlowDirection.LeftToRight;
				case ALayoutDirection.Rtl:
					return FlowDirection.RightToLeft;
				default:
					return FlowDirection.MatchParent;
			}
		}
	}
}