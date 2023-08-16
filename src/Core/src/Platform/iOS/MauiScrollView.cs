using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView
	{
		public MauiScrollView()
		{
		}

		[UnconditionalSuppressMessage("Memory", "MA0001", Justification = "Proven safe in test: ScrollViewTests.HandlerDoesNotleak")]
		public event EventHandler? LayoutSubviewsChanged;

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

