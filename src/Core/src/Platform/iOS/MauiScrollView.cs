using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView
	{
		readonly WeakEventManager _weakEventManager = new WeakEventManager();

		public MauiScrollView()
		{
		}

		public event EventHandler LayoutSubviewsChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(LayoutSubviewsChanged));
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

