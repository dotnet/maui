using System;
namespace Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls
{
	public static class LayoutDirectionExtensions
	{
		public static FlowDirection ToFlowDirection(this LayoutDirection layoutDirection) =>
			layoutDirection == LayoutDirection.RightToLeft
				? FlowDirection.RightToLeft
				: FlowDirection.LeftToRight;
	}
}