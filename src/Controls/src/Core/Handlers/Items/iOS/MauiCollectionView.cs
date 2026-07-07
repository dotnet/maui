using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items;

public class MauiCollectionView : UICollectionView, IUIViewLifeCycleEvents, IPlatformMeasureInvalidationController
{
	bool _invalidateParentWhenMovedToWindow;
	bool? _isSwipeEnabled;

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
	}
}