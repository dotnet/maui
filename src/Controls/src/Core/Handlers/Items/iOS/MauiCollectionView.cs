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

	readonly WeakEventManager _movedToWindowEventManager = new();

#if MACCATALYST
	// KVO-based scroll restore for Mac Catalyst silent contentOffset reset (Issue #34271)
	// This bug is Mac Catalyst-only: UIKit silently shifts contentOffset during any
	// window state change (Picker dismiss, window minimize/maximize, window resize).

	// True while we're watching for a silent UIKit contentOffset reset after a programmatic scroll.
	// Cleared by DraggingStarted (user intent) or after a restore fires.
	bool _isTrackingScrollRestore;

	// The section, item, and alignment to re-scroll to if a silent reset is detected.
	int _pendingScrollRestoreSection;
	int _pendingScrollRestoreItem;
	UICollectionViewScrollPosition _pendingScrollRestorePosition;

	// The last observed Y offset while tracking — used as the reference to detect a sudden drop.
	nfloat _lastKnownOffsetY;

	// KVO observer token for contentOffset — active while the view is attached to a window.
	IDisposable? _contentOffsetObserver;

	internal void SetPendingScrollRestore(int section, int item, UICollectionViewScrollPosition position)
	{
		_pendingScrollRestoreSection = section;
		_pendingScrollRestoreItem = item;
		_pendingScrollRestorePosition = position;
		_lastKnownOffsetY = ContentOffset.Y;
		_isTrackingScrollRestore = true;
	}

	internal void ClearPendingScrollRestore()
	{
		_isTrackingScrollRestore = false;
		_lastKnownOffsetY = 0;
	}

	void OnContentOffsetChanged(NSObservedChange change)
	{
		if (!_isTrackingScrollRestore)
		{
			return;
		}

		var y = ContentOffset.Y;

		// Y is within 10px of the settled reference — normal movement, update reference.
		// DraggingStarted clears _isTrackingScrollRestore before any user drag fires KVO,
		// so any drop > 10px here is a silent UIKit reset.
		// This handles both Y≈0 resets AND non-zero clamp resets (e.g., Y=713→Y=300).
		if (y >= _lastKnownOffsetY - 10.0f)
		{
			_lastKnownOffsetY = y;
			return;
		}

		// Y dropped more than 10px — silent UIKit reset detected
		if (_lastKnownOffsetY <= 0)
		{
			return;
		}

		var section = _pendingScrollRestoreSection;
		var item = _pendingScrollRestoreItem;
		var scrollPosition = _pendingScrollRestorePosition;

		ClearPendingScrollRestore();

		DispatchQueue.MainQueue.DispatchAsync(() =>
		{
			using var indexPath = NSIndexPath.Create(section, item);
			ScrollToItem(indexPath, scrollPosition, false);
		});
	}

	void StartContentOffsetObserver()
	{
		if (_contentOffsetObserver is not null)
		{
			return;
		}

		_contentOffsetObserver = this.AddObserver("contentOffset", NSKeyValueObservingOptions.New, OnContentOffsetChanged);
	}

	void StopContentOffsetObserver()
	{
		_contentOffsetObserver?.Dispose();
		_contentOffsetObserver = null;
	}
#endif

	internal bool NeedsCellLayout { get; set; }

	public MauiCollectionView(CGRect frame, UICollectionViewLayout layout) : base(frame, layout)
	{
	}

	public override void ScrollRectToVisible(CGRect rect, bool animated)
	{
		if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
		{
			base.ScrollRectToVisible(rect, animated);
		}
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

#if MACCATALYST
		if (Window is not null)
		{
			StartContentOffsetObserver();
		}
		else
		{
			StopContentOffsetObserver();
			ClearPendingScrollRestore();
		}
#endif
	}
}
