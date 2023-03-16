using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView
	{
		public MauiScrollView()
		{
		}

		// overriding this method so it does not automatically scroll large UITextFields
		// while the KeyboardAutoManagerScroll is scrolling.
		public override void ScrollRectToVisible(CGRect rect, bool animated)
		{
			if (!KeyboardAutoManagerScroll.IsCurrentlyScrolling)
				base.ScrollRectToVisible(rect, animated);
		}
	}
}

