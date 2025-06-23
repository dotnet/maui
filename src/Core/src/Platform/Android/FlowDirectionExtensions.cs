using Android.Widget;
using ALayoutDirection = Android.Views.LayoutDirection;
using ATextDirection = Android.Views.TextDirection;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
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

		internal static ALayoutDirection ToLayoutDirection(this FlowDirection direction)
		{
			switch (direction)
			{
				case FlowDirection.LeftToRight:
					return ALayoutDirection.Ltr;
				case FlowDirection.RightToLeft:
					return ALayoutDirection.Rtl;
				default:
					return ALayoutDirection.Inherit;
			}
		}

		internal static ATextDirection ToTextDirection(this ALayoutDirection direction)
		{
			switch (direction)
			{
				case ALayoutDirection.Ltr:
					return ATextDirection.Ltr;
				case ALayoutDirection.Rtl:
					return ATextDirection.Rtl;
				default:
					return ATextDirection.Inherit;
			}
		}
	}
}