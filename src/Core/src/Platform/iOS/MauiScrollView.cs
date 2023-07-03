using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView
	{
		internal event EventHandler? LayoutSubviewsChanged;

		public MauiScrollView()
		{
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			LayoutSubviewsChanged?.Invoke(this, EventArgs.Empty);
		}

		// overriding this method so it does not automatically scroll large UITextFields
		// while the KeyboardAutoManagerScroll is scrolling.
		public override void ScrollRectToVisible(CGRect rect, bool animated)
		{
			if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
				base.ScrollRectToVisible(rect, animated);
		}
	}
}

