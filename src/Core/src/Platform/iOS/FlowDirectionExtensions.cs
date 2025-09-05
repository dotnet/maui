using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class FlowDirectionExtensions
	{
		public static FlowDirection ToFlowDirection(this UIUserInterfaceLayoutDirection direction)
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
		
		// TODO: Make it public in .NET 10
		internal static UIUserInterfaceLayoutDirection ToUIUserInterfaceLayoutDirection(this FlowDirection direction)
		{
			switch (direction)
			{
				case FlowDirection.LeftToRight:
					return UIUserInterfaceLayoutDirection.LeftToRight;
				case FlowDirection.RightToLeft:
					return UIUserInterfaceLayoutDirection.RightToLeft;
				default:
					throw new NotSupportedException($"ToUIUserInterfaceLayoutDirection: {direction}");
			}
		}
	}
}