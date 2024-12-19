using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents, IMauiPlatformView
	{
		bool _invalidateParentWhenMovedToWindow;
		CGSize _lastContentSize;

		WeakReference<IView>? _reference;

		internal IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		public override CGSize ContentSize
		{
			get => base.ContentSize;
			set
			{
				base.ContentSize = value;

				// If the ContentSize changed, we need to invalidate the measure of the ScrollView
				if (_lastContentSize != value)
				{
					_lastContentSize = value;
					View?.InvalidateMeasure();
				}
			}
		}

		// overriding this method so it does not automatically scroll large UITextFields
		// while the KeyboardAutoManagerScroll is scrolling.
		public override void ScrollRectToVisible(CGRect rect, bool animated)
		{
			if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
				base.ScrollRectToVisible(rect, animated);
		}

		void IMauiPlatformView.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateParentWhenMovedToWindow = true;
		}

		void IMauiPlatformView.InvalidateMeasure(bool isPropagating)
		{
			// To correctly invalidate the measure of a ScrollView, we need to include its content
			if (!isPropagating)
			{
				Subviews.FirstOrDefault()?.SetNeedsLayout();
			}

			SetNeedsLayout();
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
			if (_invalidateParentWhenMovedToWindow)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}
		}
	}
}

