using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items;

internal partial class MauiCollectionView : UICollectionView, IUIViewLifeCycleEvents, IPlatformMeasureInvalidationController
{
	bool _invalidateParentWhenMovedToWindow;

	WeakReference<ICustomMauiCollectionViewDelegate>? _customDelegate;

	internal bool NeedsCellLayout { get; set; }

	public override CGPoint ContentOffset
	{
		get => base.ContentOffset;
		set
		{
			if (IsUIKitInterferingWithKeyboardAutoManagerScroll())
			{
				// UIKit is trying to mess up with our keyboard auto scroll manger, so we ignore it.
				return;
			}

			base.ContentOffset = value;
		}
	}

	public MauiCollectionView(CGRect frame, UICollectionViewLayout layout) : base(frame, layout)
	{
	}

	public override void ScrollRectToVisible(CGRect rect, bool animated)
	{
		if (KeyboardAutoManagerScroll.IsKeyboardAutoScrollAnimating)
		{
			// UIKit is trying to mess up with our keyboard auto scroll manger, so we ignore it.
			return;
		}

		base.ScrollRectToVisible(rect, animated);
	}

	public override void SetContentOffset(CGPoint contentOffset, bool animated)
	{
		if (KeyboardAutoManagerScroll.IsKeyboardAutoScrollAnimating)
		{
			// UIKit is trying to mess up with our keyboard auto scroll manger, so we ignore it.
			return;
		}
		
		// Auto scrolling messes up with CV1, so we can leverage this only in CV2.
		if (KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling && CollectionViewLayout is not UICollectionViewFlowLayout)
		{
			KeyboardAutoManagerScroll.EnsureTextViewCursorIsVisible();
			return;
		}

		base.SetContentOffset(contentOffset, animated);
	}

	void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
	{
		_invalidateParentWhenMovedToWindow = true;
	}

	bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
	{
		if (isPropagating)
		{
			NeedsCellLayout = true;
		}

		SetNeedsLayout();
		return !isPropagating;
	}

	[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
	EventHandler? _movedToWindow;

	event EventHandler? IUIViewLifeCycleEvents.MovedToWindow
	{
		add => _movedToWindow += value;
		remove => _movedToWindow -= value;
	}

	public override void MovedToWindow()
	{
		base.MovedToWindow();
		_movedToWindow?.Invoke(this, EventArgs.Empty);

		if (_customDelegate?.TryGetTarget(out var target) == true)
		{
			target.MovedToWindow(this);
		}

		if (_invalidateParentWhenMovedToWindow)
		{
			_invalidateParentWhenMovedToWindow = false;
			this.InvalidateAncestorsMeasures();
		}
	}

	internal void SetCustomDelegate(ICustomMauiCollectionViewDelegate customDelegate)
	{
		_customDelegate = new WeakReference<ICustomMauiCollectionViewDelegate>(customDelegate);
	}


	internal interface ICustomMauiCollectionViewDelegate
	{
		void MovedToWindow(UIView view);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IsUIKitInterferingWithKeyboardAutoManagerScroll()
	{
		// Using `NSThread.NativeCallStack` is not ideal due to its performance cost and reliance on native internals.
		// However, it's currently the only reliable way to detect interference from UIKit's internal keyboard animation handling.
		// To avoid unnecessary overhead, we restrict this check to our own controlled animation scopes only.
		if (KeyboardAutoManagerScroll.IsKeyboardAutoScrollAnimating)
		{
			var stackTrace = NSThread.NativeCallStack;
			var stackTraceLength = stackTrace.Length;
			var regex = IsUIKitKeyboardManagerAutoScrollRegex();
			for (int i = 0; i < stackTraceLength; i++)
			{
				var call = stackTrace[i];
				if (regex.IsMatch(call))
				{
					// UIKit is trying to mess up with our content offset, so we ignore it.
					return true;
				}
			}
		}

		return false;
	}

	[GeneratedRegex("(UIAutoRespondingScrollViewControllerKeyboardSupport|_restoreOrAdjustContentOffsetIfNecessaryWithInsets)")]
	private static partial Regex IsUIKitKeyboardManagerAutoScrollRegex();
}