using System;
using System.Diagnostics.CodeAnalysis;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items;

public class MauiCollectionView : UICollectionView, IUIViewLifeCycleEvents, IPlatformMeasureInvalidationController
{
	bool _invalidateParentWhenMovedToWindow;

	// Tracks whether a programmatic ScrollTo is active and should be restored if UIKit
	// erroneously resets contentOffset to 0 (e.g., on Mac Catalyst after a Picker dismiss).
	// See https://github.com/dotnet/maui/issues/34271
	bool _isTrackingScrollRestore;
	// The last known valid (non-zero) content offset Y after a programmatic scroll.
	// Used to distinguish an intentional scroll-to-top from an erroneous UIKit reset.
	nfloat _pendingScrollRestoreTargetY = -1;
	IDisposable? _contentOffsetObserver;

	internal void SetPendingScrollRestore()
	{
		_isTrackingScrollRestore = true;
		_pendingScrollRestoreTargetY = -1; // Will be updated via KVO once layout settles
	}

	internal void ClearPendingScrollRestore()
	{
		_isTrackingScrollRestore = false;
		_pendingScrollRestoreTargetY = -1;
	}

	void StartContentOffsetObserver()
	{
		if (_contentOffsetObserver != null)
			return;
		_contentOffsetObserver = AddObserver("contentOffset", NSKeyValueObservingOptions.New,
			_ => OnContentOffsetChanged());
	}

	void StopContentOffsetObserver()
	{
		_contentOffsetObserver?.Dispose();
		_contentOffsetObserver = null;
	}

	void OnContentOffsetChanged()
	{
		if (!_isTrackingScrollRestore)
			return;

		var y = ContentOffset.Y;

		// Track the last known valid position while the offset is above zero.
		// This lets us distinguish an intentional scroll-to-top from an erroneous UIKit reset.
		if (y >= 1.0)
		{
			_pendingScrollRestoreTargetY = y;
			return;
		}

		// ContentOffset dropped to ~0. Only treat this as an erroneous reset if
		// we previously observed a non-zero settled position after the scroll.
		if (_pendingScrollRestoreTargetY < 1.0)
			return;

		var targetY = _pendingScrollRestoreTargetY;

		// Restore asynchronously so UIKit finishes the current event before we re-scroll.
		DispatchQueue.MainQueue.DispatchAsync(() =>
		{
			if (_isTrackingScrollRestore && targetY >= 1.0)
				SetContentOffset(new CGPoint(ContentOffset.X, targetY), false);
		});
	}

	readonly WeakEventManager _movedToWindowEventManager = new();

	internal bool NeedsCellLayout { get; set; }

	public MauiCollectionView(CGRect frame, UICollectionViewLayout layout) : base(frame, layout)
	{
	}

	public override void ScrollRectToVisible(CGRect rect, bool animated)
	{
		if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
			base.ScrollRectToVisible(rect, animated);
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
	event EventHandler? IUIViewLifeCycleEvents.MovedToWindow
	{
		add => _movedToWindowEventManager.AddEventHandler(value);
		remove => _movedToWindowEventManager.RemoveEventHandler(value);
	}

	public override void MovedToWindow()
	{
		base.MovedToWindow();
		_movedToWindowEventManager.HandleEvent(this, EventArgs.Empty, nameof(IUIViewLifeCycleEvents.MovedToWindow));

		if (_invalidateParentWhenMovedToWindow)
		{
			_invalidateParentWhenMovedToWindow = false;
			this.InvalidateAncestorsMeasures();
		}

		// Start/stop watching contentOffset when the view joins or leaves a window.
		// This KVO lets us detect silent UIKit resets of contentOffset (e.g., on Mac
		// Catalyst after a Picker dismiss) that don't fire any scroll/layout callbacks.
		if (Window != null)
			StartContentOffsetObserver();
		else
			StopContentOffsetObserver();
	}
}