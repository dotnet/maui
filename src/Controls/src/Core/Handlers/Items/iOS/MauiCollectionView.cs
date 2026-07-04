using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items;

public class MauiCollectionView : UICollectionView, IUIViewLifeCycleEvents, IPlatformMeasureInvalidationController
{
	bool _invalidateParentWhenMovedToWindow;
	bool? _isSwipeEnabled;
	bool? _isBounceEnabled;

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
	// the cached _isSwipeEnabled state to any embedded UIScrollView at creation time. On-device
	// testing showed Bounces/AlwaysBounce* must be reapplied here AND on the outer collection
	// view (this) AND on every LayoutSubviews pass (both enabled and disabled) - setting them in
	// only one location/state does not reliably suppress the rubber-band bounce.
	public override void AddSubview(UIView view)
	{
		base.AddSubview(view);

		if (_isSwipeEnabled.HasValue)
		{
			ApplyOuterBounceState(_isSwipeEnabled.Value);

			if (view is UIScrollView scrollView && view is not UICollectionView)
			{
				ApplyState(scrollView, _isSwipeEnabled.Value);
			}
		}
	}

	// UIKit's compositional layout does not always add its embedded orthogonal UIScrollView(s)
	// through AddSubview - depending on the iOS version/layout it can be inserted via
	// InsertSubview/InsertSubviewBelow, which bypasses the AddSubview override above. This is
	// most noticeable when IsSwipeEnabled is set to false before the CarouselView is ever
	// displayed (e.g. at load time in XAML), because the embedded scroller can be created
	// during the very first layout pass, before SetSwipeEnabled(false) below runs. We reapply on
	// every layout pass, for both enabled and disabled, since Bounces can otherwise revert to its
	// default (true) after UIKit reconfigures the embedded scroller.
	public override void LayoutSubviews()
	{
		base.LayoutSubviews();

		if (_isSwipeEnabled.HasValue)
		{
			ApplySwipeEnabledToEmbeddedScrollViews(_isSwipeEnabled.Value);
		}
	}

	// Controls ScrollEnabled on the embedded orthogonal UIScrollView(s) created by
	// UICollectionViewCompositionalLayout for CarouselView2 paging. Caches the state
	// so that scrollers added later (via AddSubview/LayoutSubviews) also get the correct value.
	internal void SetSwipeEnabled(bool enabled)
	{
		_isSwipeEnabled = enabled;
		ApplySwipeEnabledToEmbeddedScrollViews(enabled);
	}

	// Caches the CarouselView's IsBounceEnabled value so the swipe-enabled path can restore the
	// user's intended bounce state instead of forcing it on. A disabled swipe always keeps bounce
	// off regardless of this value.
	internal void SetBounceEnabled(bool enabled)
	{
		_isBounceEnabled = enabled;

		if (_isSwipeEnabled != false)
		{
			Bounces = enabled;
		}
	}

	void ApplySwipeEnabledToEmbeddedScrollViews(bool enabled)
	{
		// Bounce must be reapplied on the outer UICollectionView itself (it is a UIScrollView) as
		// well as any embedded orthogonal scroller - confirmed empirically that setting it on only
		// one of the two does not reliably suppress bounce. Only the embedded scroller's
		// ScrollEnabled is toggled; the outer CollectionView's ScrollEnabled is left untouched so
		// unrelated scrolling (e.g. main-axis list scrolling) keeps working.
		ApplyOuterBounceState(enabled);

		foreach (var subview in Subviews)
		{
			if (subview is UIScrollView scrollView && subview is not UICollectionView)
			{
				ApplyState(scrollView, enabled);
			}
		}
	}

	// The outer collection view's bounce is owned by IsBounceEnabled when swipe is enabled, so we
	// only force it off while swipe is disabled and otherwise honor the cached IsBounceEnabled
	// value (defaulting to the UIKit default of true). AlwaysBounce* stay at their default (false).
	void ApplyOuterBounceState(bool enabled)
	{
		Bounces = enabled ? (_isBounceEnabled ?? true) : false;
		AlwaysBounceHorizontal = false;
		AlwaysBounceVertical = false;
	}

	// ScrollEnabled alone does not stop rubber-band bouncing; Bounces controls that independently.
	// AlwaysBounce* are always left at their UIKit default (false) - forcing them true would make
	// the scroller rubber-band on axes/sizes where it otherwise would not.
	static void ApplyState(UIScrollView scrollView, bool enabled)
	{
		scrollView.ScrollEnabled = enabled;
		scrollView.Bounces = enabled;
		scrollView.AlwaysBounceHorizontal = false;
		scrollView.AlwaysBounceVertical = false;
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