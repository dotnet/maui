using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	internal static class AlignmentExtensions
	{
		internal static NSTextAlignment ToNativeTextAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return NSTextAlignment.Center;
				case TextAlignment.End:
					return NSTextAlignment.Right;
				default:
					return NSTextAlignment.Left;
			}
		}
	}
}