using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents, ISchedulesSetNeedsLayout
	{
		bool _fireSetNeedsLayoutOnParentWhenWindowAttached;

		// overriding this method so it does not automatically scroll large UITextFields
		// while the KeyboardAutoManagerScroll is scrolling.
		public override void ScrollRectToVisible(CGRect rect, bool animated)
		{
			if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
				base.ScrollRectToVisible(rect, animated);
		}

		public override void SetNeedsLayout()
		{
			// If the content container is set, we need to invalidate that too
			if (Subviews.Length > 0 && Subviews[0] is ContentView contentContainer)
			{
				contentContainer.SetNeedsLayout();
			}

			base.SetNeedsLayout();
		}

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler? _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);

			if (_fireSetNeedsLayoutOnParentWhenWindowAttached)
			{
				Superview?.PropagateSetNeedsLayout();
				_fireSetNeedsLayoutOnParentWhenWindowAttached = false;
			}
		}

		void ISchedulesSetNeedsLayout.ScheduleSetNeedsLayoutPropagation()
		{
			_fireSetNeedsLayoutOnParentWhenWindowAttached = true;
		}
	}
}

