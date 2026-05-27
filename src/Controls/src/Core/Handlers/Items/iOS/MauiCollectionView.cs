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
	bool? _isSwipeEnabled;

	readonly WeakEventManager _movedToWindowEventManager = new();

#if MACCATALYST
	// KVO-based scroll restore for Mac Catalyst silent contentOffset reset (Issue #34271)
	// This bug is Mac Catalyst-only: UIKit silently shifts contentOffset during any
	// window state change (Picker dismiss, window minimize/maximize, window resize).
	//
	// NOTE: Current tracking is Y-axis only, so it won't affect existing X-axis
	// (horizontal CollectionView) behavior.

	// True while we're watching for a silent UIKit contentOffset reset after a programmatic scroll.
	// Stays armed across multiple silent clamps; cleared by DraggingStarted (user intent),
	// view detach, Dispose, or a new ScrollTo request.
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

		// Y dropped more than 10px — silent UIKit reset detected.
		// Guard: if the last known reference Y is at or below 0, the original target was already
		// at the top of the list (or never moved away from it). In that case there is nothing
		// meaningful to "restore" — UIKit ending up at Y=0 IS the target state, so we skip the
		// async ScrollToItem to avoid an unnecessary scroll and a potential restore loop.
		// `<= 0` (rather than `< 0` or `== 0`) covers any negative bounce/overscroll values
		// UIKit may briefly report.
		if (_lastKnownOffsetY <= 0)
		{
			return;
		}

		var section = _pendingScrollRestoreSection;
		var item = _pendingScrollRestoreItem;
		var scrollPosition = _pendingScrollRestorePosition;

		// NOTE: Do NOT clear the pending restore here. A single window interaction
		// (Picker hover/select, resize, minimize/restore) can cause UIKit to recalculate
		// bounds multiple times, producing several silent clamps in succession. Keeping the
		// target armed allows each subsequent clamp to be restored.
		// The pending restore is cleared on: user drag (DraggingStarted), view detach
		// (MovedToWindow with Window == null), Dispose, or a new ScrollTo request.

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

			// Refresh the reference Y so subsequent KVO callbacks measure
			// the next drop against the restored offset, not the pre-restore one.
			if (_isTrackingScrollRestore)
			{
				_lastKnownOffsetY = ContentOffset.Y;
			}
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

	// UICollectionViewCompositionalLayout with OrthogonalScrollingBehavior creates a private
	// embedded UIScrollView for horizontal/vertical paging. Setting ScrollEnabled on the outer
	// UICollectionView does not affect this embedded scroller. We intercept AddSubview to apply
	// the cached _isSwipeEnabled state to any embedded UIScrollView at creation time.
	public override void AddSubview(UIView view)
	{
		base.AddSubview(view);

		if (_isSwipeEnabled.HasValue && view is UIScrollView scrollView && view is not UICollectionView)
		{
			scrollView.ScrollEnabled = _isSwipeEnabled.Value;
		}
	}

	// Controls ScrollEnabled on the embedded orthogonal UIScrollView(s) created by
	// UICollectionViewCompositionalLayout for CarouselView2 paging. Caches the state
	// so that scrollers added later (via AddSubview) also get the correct value.
	internal void SetSwipeEnabled(bool enabled)
	{
		_isSwipeEnabled = enabled;

		foreach (var subview in Subviews)
		{
			if (subview is UIScrollView scrollView && subview is not UICollectionView)
			{
				scrollView.ScrollEnabled = enabled;
			}
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
