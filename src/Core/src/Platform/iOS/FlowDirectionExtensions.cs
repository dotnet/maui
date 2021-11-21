using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class FlowDirectionExtensions
	{
		internal static FlowDirection ToFlowDirection(this UIUserInterfaceLayoutDirection direction)
		{
			switch (direction)
			{
				case UIUserInterfaceLayoutDirection.LeftToRight:
					return FlowDirection.LeftToRight;
				case UIUserInterfaceLayoutDirection.RightToLeft:
					return FlowDirection.RightToLeft;
				default:
					throw new NotSupportedException($"ToFlowDirection: {direction}");
			}
		}
	}
}