using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents
	{
		WeakReference<ICrossPlatformLayout>? _crossPlatformLayoutReference;
		bool _fireSetNeedsLayoutOnParentWhenWindowAttached;
		bool _layoutInvalidated;
		
		public ICrossPlatformLayout? CrossPlatformLayout
		{
			get => _crossPlatformLayoutReference != null && _crossPlatformLayoutReference.TryGetTarget(out var v) ? v : null;
			set => _crossPlatformLayoutReference = value == null ? null : new WeakReference<ICrossPlatformLayout>(value);
		}

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

		public override void LayoutSubviews()
		{
			if (_layoutInvalidated)
			{
				_layoutInvalidated = false;

				// If the Superview is not a MauiView, we need to manually arrange the children
				if (Superview is not MauiView or WrapperView && CrossPlatformLayout is { } layout)
				{
					var bounds = Bounds.ToRectangle();
					layout.CrossPlatformArrange(bounds);
				}
			}

			base.LayoutSubviews();
		}

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();
			_layoutInvalidated = true;
			TryToInvalidateSuperView(false);
		}

		private protected void TryToInvalidateSuperView(bool shouldOnlyInvalidateIfPending)
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

