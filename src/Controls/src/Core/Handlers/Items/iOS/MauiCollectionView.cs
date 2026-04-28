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
	//
	// NOTE: Current tracking is Y-axis only, so it won't affect existing X-axis
	// (horizontal CollectionView) behavior.

	// True while we're watching for a silent UIKit contentOffset reset after a programmatic scroll.
	// Cleared by DraggingStarted (user intent) or after a restore fires.
	bool _isTrackingScrollRestore;

	// The section, item, and alignment to re-scroll to if a silent reset is detected.
	int _pendingScrollRestoreSection;
	int _pendingScrollRestoreItem;
	UICollectionViewScrollPosition _pendingScrollRestorePosition;

	// The last observed Y offset while tracking — used as the reference to detect a sudden drop.
	nfloat _lastKnownOffsetY;

	// Threshold (in points) for detecting a silent UIKit contentOffset reset.
	// Any Y-offset drop larger than this is treated as a silent reset rather than normal movement.
	static readonly nfloat SilentResetThreshold = 10f;

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
		if (y >= _lastKnownOffsetY - SilentResetThreshold)
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
			if (Handle == IntPtr.Zero || Window is null)
			{
				return;
			}

			if (Tracking || Dragging || Decelerating)
			{
				return;
			}

			using var indexPath = NSIndexPath.Create(section, item);

			if (!IsIndexPathValidForRestore(indexPath))
			{
				return;
			}

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

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			StopContentOffsetObserver();
			ClearPendingScrollRestore();
		}
		base.Dispose(disposing);
	}

	/// <summary>
	/// Validates that the index path is within the current data source bounds
	/// before attempting a scroll restore. This prevents NSInternalInconsistencyException
	/// if the data source changed after the restore was armed.
	/// </summary>
	bool IsIndexPathValidForRestore(NSIndexPath indexPath)
	{
		var sectionCount = NumberOfSections();
		if (indexPath.Section < 0 || indexPath.Section >= sectionCount)
		{
			return false;
		}

		var itemCount = NumberOfItemsInSection(indexPath.Section);
		return indexPath.Item >= 0 && indexPath.Item < itemCount;
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
