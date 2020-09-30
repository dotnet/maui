using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	internal static class ButtonExtensions
	{
		public static NSCellImagePosition ToNSCellImagePosition(this Button control)
		{
			switch (control.ContentLayout.Position)
			{
				case Button.ButtonContentLayout.ImagePosition.Left:
					return NSCellImagePosition.ImageLeft;
				case Button.ButtonContentLayout.ImagePosition.Top:
					return NSCellImagePosition.ImageAbove;
				case Button.ButtonContentLayout.ImagePosition.Right:
					return NSCellImagePosition.ImageRight;
				case Button.ButtonContentLayout.ImagePosition.Bottom:
					return NSCellImagePosition.ImageBelow;
				default:
					return NSCellImagePosition.ImageOnly;
			}
		}
	}
}