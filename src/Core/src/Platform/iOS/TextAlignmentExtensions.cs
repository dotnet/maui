// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class TextAlignmentExtensions
	{
		internal static UITextAlignment ToPlatformHorizontal(this TextAlignment alignment, UIUserInterfaceLayoutDirection layoutDirection)
		{
			var platformAlignment = alignment.ToPlatformHorizontal();

			if (layoutDirection == UIUserInterfaceLayoutDirection.RightToLeft)
			{
				if (platformAlignment == UITextAlignment.Left)
				{
					return UITextAlignment.Right;
				}
				else if (platformAlignment == UITextAlignment.Right)
				{
					return UITextAlignment.Left;
				}
			}

			return platformAlignment;
		}

		public static UITextAlignment ToPlatformHorizontal(this TextAlignment alignment, IView view)
			=> alignment.ToPlatformHorizontal();

		public static UITextAlignment ToPlatformHorizontal(this TextAlignment alignment)
		{
			return alignment switch
			{
				TextAlignment.Center => UITextAlignment.Center,
				TextAlignment.End => UITextAlignment.Right,
				TextAlignment.Start => UITextAlignment.Left,
				_ => UITextAlignment.Left,
			};
		}

		public static UIControlContentVerticalAlignment ToPlatformVertical(this TextAlignment alignment)
		{
			return alignment switch
			{
				TextAlignment.Center => UIControlContentVerticalAlignment.Center,
				TextAlignment.End => UIControlContentVerticalAlignment.Bottom,
				TextAlignment.Start => UIControlContentVerticalAlignment.Top,
				_ => UIControlContentVerticalAlignment.Top,
			};
		}
	}
}