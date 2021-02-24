using System;
using AppKit;
using Microsoft.Maui.Controls.Compatibility.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
{
	internal static class AlignmentExtensions
	{
		internal static NSTextAlignment ToNativeTextAlignment(this TextAlignment alignment, EffectiveFlowDirection flowDirection)
		{
			var isLtr = flowDirection.IsLeftToRight();
			switch (alignment)
			{
				case TextAlignment.Center:
					return NSTextAlignment.Center;
				case TextAlignment.End:
					if (isLtr)
						return NSTextAlignment.Right;
					else
						return NSTextAlignment.Left;
				default:
					if (isLtr)
						return NSTextAlignment.Left;
					else
						return NSTextAlignment.Natural;
			}
		}
	}
}