using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : UIScrollView, IUIViewLifeCycleEvents, IMauiPlatformView
	{
#pragma warning disable MEM0002
		// Justification: this is a self-reference so it won't cause a memory leak
		// Having this as a field prevents the observer from being GC'd
		IDisposable? _contentSizeObserver;
#pragma warning restore MEM0002

		bool _invalidateParentWhenMovedToWindow;
		CGSize _lastContentSize;

		WeakReference<IView>? _reference;

		internal IView? View
		{
			get => _reference != null && _reference.TryGetTarget(out var v) ? v : null;
			set => _reference = value == null ? null : new(value);
		}

		public MauiScrollView()
		{
			_contentSizeObserver = AddObserver("contentSize", NSKeyValueObservingOptions.New, ContentSizeChanged);
		}

		void ContentSizeChanged(NSObservedChange obj)
		{
			// This is needed in case the ScrollView is set to auto-size through `VerticalOptions` or `HorizontalOptions`.
			// We use the same strategy in `CollectionView`.
			var newSize = ContentSize;
			if (newSize != _lastContentSize)
			{
				_lastContentSize = newSize;
				View?.InvalidateMeasure();
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

