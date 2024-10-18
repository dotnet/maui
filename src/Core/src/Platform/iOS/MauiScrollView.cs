using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents, IPropagatesSetNeedsLayout
	{
		bool _isSettingNeedsLayout;
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
			if (_isSettingNeedsLayout)
			{
				return;
			}

			_isSettingNeedsLayout = true;
			// If the content container is set, we need to invalidate that too
			if (Subviews.Length > 0 && Subviews[0] is ContentView contentContainer)
			{
				contentContainer.SetNeedsLayout();
			}
			_isSettingNeedsLayout = false;

			base.SetNeedsLayout();
			TryToInvalidateSuperView(false);
		}

		private void TryToInvalidateSuperView(bool shouldOnlyInvalidateIfPending)
		{
			if (shouldOnlyInvalidateIfPending && !_fireSetNeedsLayoutOnParentWhenWindowAttached)
			{
				return;
			}

			// We check for Window to avoid scenarios where an invalidate might propagate up the tree
			// To a SuperView that's been disposed which will cause a crash when trying to access it
			if (Window is not null)
			{
				this.Superview?.SetNeedsLayout();
				_fireSetNeedsLayoutOnParentWhenWindowAttached = false;
			}
			else
			{
				_fireSetNeedsLayoutOnParentWhenWindowAttached = true;
			}
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
			TryToInvalidateSuperView(true);
		}
	}
}

