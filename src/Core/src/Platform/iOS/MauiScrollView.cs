using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents, IUserInteractionEnabledManagingView
	{
		bool _userInteractionEnabledOverride;

		public MauiScrollView()
		{
		}

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

			if (!_userInteractionEnabledOverride && Equals(result))
			{
				// If user interaction is disabled (IOW, if the corresponding View is InputTransparent),
				// then we exclude the managing view itself from hit testing. But it's children are valid
				// hit testing targets.

				return null;
			}

			if (result is IUserInteractionEnabledManagingView v && !v.UserInteractionEnabledOverride)
			{
				// If the child is a managing view then we need to check the UserInteractionEnabledOverride
				// since managing view instances always have user interaction enabled.

				return null;
			}

			return result;
		}

		bool IUserInteractionEnabledManagingView.UserInteractionEnabledOverride => _userInteractionEnabledOverride;

		public override bool UserInteractionEnabled
		{
			get => base.UserInteractionEnabled;
			set
			{
				// We leave the base UIE value true no matter what, so that hit testing will find children
				// of the LayoutView. But we track the intended value so we can use it during hit testing
				// to ignore the LayoutView itself, if necessary.

				base.UserInteractionEnabled = true;
				_userInteractionEnabledOverride = value;
			}
		}
	}
}
