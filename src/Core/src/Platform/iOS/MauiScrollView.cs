using System;
using UIKit;
using CoreGraphics;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView
	{
		public MauiScrollView()
		{
		}

		// overriding this method it does not automatically scroll large UITextFields.
		// Save the scrolling for KeyboardAutoManagerScroll.cs
		public override void ScrollRectToVisible(CGRect rect, bool animated)
		{
		}
	}
}

