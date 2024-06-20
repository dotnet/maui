using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents, IInputTransparentManagingView
	{
		public MauiScrollView()
		{
		}

		bool IInputTransparentManagingView.InputTransparent { get; set; }

		// overriding this method so it does not automatically scroll large UITextFields
		// while the KeyboardAutoManagerScroll is scrolling.
		public override void ScrollRectToVisible(CGRect rect, bool animated)
		{
			if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
				base.ScrollRectToVisible(rect, animated);
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
		}

		public override UIView? HitTest(CGPoint point, UIEvent? uievent)
		{
			var result = base.HitTest(point, uievent);

			if (result is null)
			{
				return null;
			}

			if (((IInputTransparentManagingView)this).InputTransparent && Equals(result))
			{
				// If user interaction is disabled (IOW, if the corresponding View is InputTransparent),
				// then we exclude the managing view itself from hit testing. But it's children are valid
				// hit testing targets.

				return null;
			}

			if (result is IInputTransparentManagingView v && v.InputTransparent)
			{
				// If the child is a managing view then we need to check the InputTransparent
				// since managing view instances always have user interaction enabled.

				return null;
			}

			return result;
		}
	}
}
