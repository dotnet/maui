using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;
using PointF = CoreGraphics.CGPoint;

namespace Microsoft.Maui.Platform;

/// <summary>
/// Manages flyout/detail split layout with pan gesture, tap-to-close, and animated show/hide.
/// Pure UIKit — no Controls references.
/// Communicates state changes back through <see cref="IFlyoutContainerDelegate"/>.
/// </summary>
internal class FlyoutContainerManager
{

	readonly WeakReference<IFlyoutContainerDelegate> _delegateRef;
	WeakReference<UIViewController>? _parentVCRef;

	// Container views (plain UIViews that hold child VC views)
	UIView? _flyoutContainerView;
	UIView? _detailContainerView;
	UIView? _clickOffView;

	// Cached colors so SetShadowBackgroundColor/SetScrimColor survive being called before
	// the containers exist (or after a future teardown/re-pack), instead of being lost.
	UIColor? _shadowBackgroundColor;
	UIColor? _scrimColor;

	// Whether the current layout pass wants the detail content dimmed via the click-off scrim
	// overlay (see PerformLayout). Recomputed on every layout since it depends on idiom/split state.
	bool _dimDetailWithScrim;

	// Gesture recognizers
	UIPanGestureRecognizer? _panGesture;
	UITapGestureRecognizer? _tapGesture;

	// Child VCs (managed via parent VC's containment API)
	UIViewController? _flyoutVC;
	UIViewController? _detailVC;

	bool _isPresented;
	bool _isGestureEnabled = true;
	bool _applyShadow;
	bool _skipShadowInSplitMode;
	bool _preservePresentedStateOnTransition;
	bool _initialLayoutFinished;
	bool _flyoutOverlapsDetail;

	FlyoutBehavior _flyoutBehavior = FlyoutBehavior.Flyout;
	FlowDirection _flowDirection = FlowDirection.MatchParent;
	double _flyoutWidth = -1; // -1 means platform default
	double _flyoutHeight = -1; // -1 means full height


	internal FlyoutContainerManager(IFlyoutContainerDelegate containerDelegate)
	{
		_delegateRef = new WeakReference<IFlyoutContainerDelegate>(containerDelegate);
	}


	/// <summary>
	/// On iPad, the flyout overlaps the detail (slides over from left).
	/// On iPhone, the detail slides right to reveal the flyout behind it.
	/// Consumers can override via <see cref="IFlyoutContainerDelegate.GetFlyoutOverlapsDetail"/> to force overlay mode.
	/// </summary>
	bool FlyoutOverlapsDetailsInPopoverMode =>
		_flyoutOverlapsDetail || UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;

	bool IsRTL => _flowDirection == FlowDirection.RightToLeft;

	UIView? ParentView => _parentVCRef is not null && _parentVCRef.TryGetTarget(out var vc) ? vc.View : null;

	/// <summary>
	/// The currently-hosted Detail child VC. Used by <see cref="FlyoutContainerViewController"/>
	/// to route status-bar/home-indicator delegate queries explicitly to Detail, matching the
	/// legacy renderer's behavior (it never asked the Flyout VC for these).
	/// </summary>
	internal UIViewController? ActiveDetailViewController => _detailVC;

	/// <summary>Exposes the scrim (click-off) view so callers can apply brush-based backgrounds (e.g. gradients). Used by Shell.</summary>
	internal UIView? ScrimView => _clickOffView;

	bool ShouldShowSplitMode
	{
		get
		{
			if (!FlyoutOverlapsDetailsInPopoverMode)
			{
				return false; // iPhone never splits
			}

			// FlyoutBehavior is already resolved by Controls: Locked = split, Flyout = not-split.
			// Trust it — don't recompute from raw bounds,
			// or Popover/SplitOnPortrait will split incorrectly.
			return _flyoutBehavior == FlyoutBehavior.Locked;
		}
	}


	/// <summary>
	/// Called from the container VC's ViewDidLoad. Sets up the view hierarchy
	/// and stores the parent VC reference for containment API calls.
	/// </summary>
	internal void SetupContainerViews(UIViewController parentVC)
	{
		_parentVCRef = new WeakReference<UIViewController>(parentVC);

		// Query Shell-vs-FlyoutPage behavior differences from the delegate before PackContainers
		// reads _flyoutOverlapsDetail to decide the z-order of flyout vs detail containers.
		if (_delegateRef.TryGetTarget(out var configDelegate))
		{
			_flyoutOverlapsDetail = configDelegate.GetFlyoutOverlapsDetail();
			_skipShadowInSplitMode = configDelegate.GetSkipShadowInSplitMode();
			_preservePresentedStateOnTransition = configDelegate.GetPreservePresentedStateOnTransition();
		}

		var parentView = parentVC.View!;
		_flyoutContainerView = new UIView { ClipsToBounds = true };
		_detailContainerView = new UIView { BackgroundColor = UIColor.Black, ClipsToBounds = true };
		_clickOffView = new UIView { BackgroundColor = new UIColor(0, 0, 0, 0) };

		PackContainers(parentView);
		SetupTapGesture();
		UpdatePanGesture();
	}

	/// <summary>
	/// Called from handler when parent VC's view lays out subviews.
	/// </summary>
	internal void OnParentViewDidLayoutSubviews()
	{
		LayoutPanes(animated: false);

		if (!_initialLayoutFinished)
		{
			_initialLayoutFinished = true;

			// Read IsPresented from virtual view at layout time, as the value
			// may differ from _isPresented due to FlyoutPage internal validation.
			bool isPresented = _isPresented;
			if (_delegateRef.TryGetTarget(out var del))
			{
				isPresented = del.GetCurrentIsPresented();
			}

			// Locked behavior ensures the flyout is always open; ShouldShowSplitMode is no longer
			// true for Shell (see _flyoutOverlapsDetail guard), so check _flyoutBehavior directly.
			if (ShouldShowSplitMode || _flyoutBehavior == FlyoutBehavior.Locked)
			{
				SetPresented(true, animated: false, notifyDelegate: false);
			}
			else
			{
				SetPresented(isPresented, animated: false, notifyDelegate: false);
			}
		}
	}

	/// <summary>
	/// Called from handler when parent VC transitions size (rotation, multitasking).
	/// </summary>
	internal void OnParentViewWillTransitionToSize(CGSize toSize)
	{

		if (!OperatingSystem.IsMacCatalyst())
		{
			bool shouldSplit = ShouldShowSplitMode;
			if (FlyoutOverlapsDetailsInPopoverMode)
			{
				// notifyDelegate: false — rotation is platform-initiated.
				// Preserve manual presented state only if the consumer opted in (see IFlyoutContainerDelegate.GetPreservePresentedStateOnTransition).
				bool targetPresented = shouldSplit || (_preservePresentedStateOnTransition && _isPresented);
				SetPresented(targetPresented, animated: true, notifyDelegate: false);
			}
			else if (!shouldSplit && _isPresented)
			{
				// iPhone rotation: notify delegate so virtual view stays in sync.
				SetPresented(false, animated: true, notifyDelegate: true);
			}
		}

		NotifyLeftBarButtonNeedsUpdate();
	}

	/// <summary>
	/// Called from container VC's ViewDidAppear.
	/// </summary>
	internal void OnViewDidAppear()
	{
		if (_delegateRef.TryGetTarget(out var del))
		{
			del.OnViewDidAppear();
		}
	}

	/// <summary>
	/// Called from container VC's ViewWillDisappear.
	/// </summary>
	internal void OnViewWillDisappear()
	{
		if (_delegateRef.TryGetTarget(out var del))
		{
			del.OnViewWillDisappear();
		}
	}


	internal void SetFlyoutViewController(UIViewController? flyoutVC)
	{
		if (_parentVCRef is null || !_parentVCRef.TryGetTarget(out var parentVC))
		{
			return;
		}

		if (_flyoutVC is not null)
		{
			_flyoutVC.WillMoveToParentViewController(null);
			_flyoutVC.View?.RemoveFromSuperview();
			_flyoutVC.RemoveFromParentViewController();
		}

		_flyoutVC = flyoutVC;

		if (_flyoutVC is not null && _flyoutContainerView is not null)
		{
			parentVC.AddChildViewController(_flyoutVC);
			if (_flyoutVC.View is not null)
			{
				_flyoutContainerView.AddSubview(_flyoutVC.View);
				_flyoutVC.View.Frame = _flyoutContainerView.Bounds;
			}
			_flyoutVC.DidMoveToParentViewController(parentVC);
		}

		NotifyLeftBarButtonNeedsUpdate();
	}

	internal void SetDetailViewController(UIViewController? detailVC)
	{
		if (_parentVCRef is null || !_parentVCRef.TryGetTarget(out var parentVC))
		{
			return;
		}

		if (_detailVC is not null)
		{
			_detailVC.WillMoveToParentViewController(null);
			_detailVC.View?.RemoveFromSuperview();
			_detailVC.RemoveFromParentViewController();
		}

		_detailVC = detailVC;

		if (_detailVC is not null && _detailContainerView is not null)
		{
			parentVC.AddChildViewController(_detailVC);
			if (_detailVC.View is not null)
			{
				_detailContainerView.AddSubview(_detailVC.View);
				_detailVC.View.Frame = _detailContainerView.Bounds;
			}
			_detailVC.DidMoveToParentViewController(parentVC);
		}

		// Detail drives status bar/home-indicator preferences, so invalidate both on swap.
		parentVC.SetNeedsStatusBarAppearanceUpdate();
		parentVC.SetNeedsUpdateOfHomeIndicatorAutoHidden();

		// A newly-attached Detail VC (and its UINavigationBar, if any) needs the
		// current flow direction applied — it won't have picked it up otherwise.
		ApplySemanticContentAttribute();

		ToggleAccessibilityElementsHidden();
		NotifyLeftBarButtonNeedsUpdate();
	}


	internal void UpdateIsPresented(bool isPresented, bool animated)
	{
		if (_isPresented == isPresented)
		{
			UpdateClickOffView();
			return;
		}

		// Cannot open when Disabled
		if (isPresented && _flyoutBehavior == FlyoutBehavior.Disabled)
		{
			return;
		}

		// Cannot close when Locked (Split) or in split mode
		if (!isPresented && (_flyoutBehavior == FlyoutBehavior.Locked || ShouldShowSplitMode))
		{
			return;
		}

		SetPresented(isPresented, animated, notifyDelegate: false);
	}

	internal void UpdateFlyoutBehavior(FlyoutBehavior behavior)
	{
		var previousBehavior = _flyoutBehavior;
		_flyoutBehavior = behavior;
		if (!_initialLayoutFinished)
		{
			return;
		}

		// Resolving to Flyout mode: preserve presented state only if opted in (see IFlyoutContainerDelegate.GetPreservePresentedStateOnTransition),
		// otherwise force-close/open to match ShouldShowSplitMode.
		bool shouldPresent;
		if (behavior == FlyoutBehavior.Disabled)
		{
			shouldPresent = false;
		}
		else if (behavior == FlyoutBehavior.Locked)
		{
			shouldPresent = true;
		}
		else
		{
			shouldPresent = _preservePresentedStateOnTransition ? _isPresented : ShouldShowSplitMode;
		}

		bool stateChanged = shouldPresent != _isPresented;

		if (stateChanged)
		{
			// Notify delegate so VirtualView.IsPresented and IsPresentedChanged stay in sync.
			// Safe because the mapper fires after ShouldShowSplitMode has settled;
			// the guard in OnPresentedChangedByGesture handles any remaining edge cases.
			SetPresented(shouldPresent, animated: true, notifyDelegate: true);
		}
		else
		{
			LayoutPanes(animated: true);
			UpdateClickOffView();
		}

		// Only update the bar button when behavior or presented state actually changed.
		// Skipping redundant calls prevents excessive ShouldShowToolbarButton invocations
		// when the same behavior is re-applied (e.g., repeated DisplayInfoChanged events).
		if (previousBehavior != behavior || stateChanged)
		{
			NotifyLeftBarButtonNeedsUpdate();
		}
	}

	internal void UpdateFlyoutWidth(double width)
	{
		_flyoutWidth = width;
		if (_initialLayoutFinished)
		{
			LayoutPanes(animated: false);
		}
	}

	/// <summary>Used by Shell, which supports a fixed flyout height; FlyoutPage always uses full height.</summary>
	internal void UpdateFlyoutHeight(double height)
	{
		_flyoutHeight = height;
		if (_initialLayoutFinished)
		{
			LayoutPanes(animated: false);
		}
	}

	internal void UpdateIsGestureEnabled(bool enabled)
	{
		_isGestureEnabled = enabled;
		UpdatePanGesture();
	}

	internal void UpdateFlowDirection(FlowDirection direction)
	{
		bool directionChanged = _flowDirection != direction;
		_flowDirection = direction;
		ApplySemanticContentAttribute();

		if (_initialLayoutFinished)
		{
			LayoutPanes(animated: false);

			// Recreate the hamburger bar button on real flow-direction changes so it
			// renders correctly. Initial setup is notified by SetDetailViewController /
			// SetFlyoutViewController.
			if (directionChanged)
			{
				NotifyLeftBarButtonNeedsUpdate();
			}
		}
	}

	/// <summary>
	/// Mirrors the Detail's UINavigationController view and NavigationBar for RTL/LTR,
	/// since neither inherits SemanticContentAttribute from its parent view.
	/// </summary>
	void ApplySemanticContentAttribute()
	{
		if (_detailVC is UINavigationController navController)
		{
			var semanticAttr = IsRTL
				? UISemanticContentAttribute.ForceRightToLeft
				: UISemanticContentAttribute.ForceLeftToRight;

			if (navController.View is not null)
			{
				navController.View.SemanticContentAttribute = semanticAttr;
			}
			navController.NavigationBar.SemanticContentAttribute = semanticAttr;
		}
	}

	internal void UpdateApplyShadow(bool applyShadow)
	{
		_applyShadow = applyShadow;
	}

	/// <summary>Sets the background color shown behind the detail content when <see cref="UpdateApplyShadow"/> dims it.
	/// Defaults to black. Pass <see cref="UIColor.SystemBackground"/> to replicate a light-overlay dim (Shell's historical behavior). Used by Shell.</summary>
	internal void SetShadowBackgroundColor(UIColor color)
	{
		_shadowBackgroundColor = color;

		if (_detailContainerView is not null)
		{
			_detailContainerView.BackgroundColor = color;
		}
	}

	/// <summary>Sets the scrim (click-off overlay) background color. Pass <c>null</c> to reset to transparent. Used by Shell.</summary>
	internal void SetScrimColor(UIColor? color)
	{
		_scrimColor = color;

		if (_clickOffView is not null)
		{
			_clickOffView.BackgroundColor = color ?? new UIColor(0, 0, 0, 0);
		}
	}

	internal void TearDown()
	{
		if (_tapGesture is not null)
		{
			_clickOffView?.RemoveGestureRecognizer(_tapGesture);
			_tapGesture.Dispose();
			_tapGesture = null;
		}

		if (_panGesture is not null)
		{
			ParentView?.RemoveGestureRecognizer(_panGesture);
			_panGesture.Dispose();
			_panGesture = null;
		}

		_clickOffView?.RemoveFromSuperview();
		_clickOffView?.Dispose();
		_clickOffView = null;

		// Remove child VCs via containment API
		SetFlyoutViewController(null);
		SetDetailViewController(null);

		_flyoutContainerView?.RemoveFromSuperview();
		_flyoutContainerView?.Dispose();
		_flyoutContainerView = null;

		_detailContainerView?.RemoveFromSuperview();
		_detailContainerView?.Dispose();
		_detailContainerView = null;
	}


	void LayoutPanes(bool animated)
	{
		var parentView = ParentView;
		if (parentView is null || _flyoutContainerView is null || _detailContainerView is null)
		{
			return;
		}

		var frame = parentView.Bounds;

		// Apply safe area insets when the page has opted in (IgnoreSafeArea = false).
		// By default on iOS, IgnoreSafeArea = true so this is skipped.
		bool ignoreSafeArea = _delegateRef.TryGetTarget(out var safeAreaDel) && safeAreaDel.GetIgnoreSafeArea();
		if (OperatingSystem.IsIOSVersionAtLeast(11) && !ignoreSafeArea)
		{
			var safeAreaTop = parentView.SafeAreaInsets.Top;
			if (safeAreaTop > 0)
			{
				frame.Y = safeAreaTop;
				frame.Height -= safeAreaTop;
			}
		}

		var flyoutFrame = frame;
		nfloat opacity = 1;
		_dimDetailWithScrim = false;

		// Apply custom flyout height when set; default uses full frame height.
		if (_flyoutHeight > 0)
		{
			flyoutFrame.Height = (nfloat)_flyoutHeight;
		}

		// Calculate flyout width
		flyoutFrame.Width = GetFlyoutWidth(frame);

		// RTL: flyout on right side (phone mode only)
		if (IsRTL && !FlyoutOverlapsDetailsInPopoverMode)
		{
			flyoutFrame.X = (int)(frame.Width - flyoutFrame.Width);
		}

		// Calculate detail frame
		var detailFrame = frame;
		if (_isPresented)
		{
			if (!FlyoutOverlapsDetailsInPopoverMode || ShouldShowSplitMode)
			{
				// iOS mirrors the locked RTL detail origin, while Mac Catalyst requires the
				// explicit flyout-width offset used by the legacy Shell renderer.
				if (IsRTL && ShouldShowSplitMode && !(OperatingSystem.IsMacCatalyst() && _flyoutOverlapsDetail))
				{
					detailFrame.X = 0;
				}
				else
				{
					detailFrame.X += flyoutFrame.Width;
				}
			}

			if (FlyoutOverlapsDetailsInPopoverMode && ShouldShowSplitMode)
			{
				detailFrame.Width -= flyoutFrame.Width;
			}

			// Shadow only makes sense in overlay mode (flyout on top of detail).
			// Skip dimming when opted in (see IFlyoutContainerDelegate.GetSkipShadowInSplitMode) and either:
			//  - the idiom is Pad or Mac Catalyst (desktop/tablet split-view UX never dims the detail pane), or
			//  - ShouldShowSplitMode is true (covers Shell's Locked+RTL split layout on iPhone too, not just Pad/Mac).
			bool isDesktopOrTabletIdiom = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad
				|| OperatingSystem.IsMacCatalyst();
			if (_applyShadow && !(_skipShadowInSplitMode && (isDesktopOrTabletIdiom || ShouldShowSplitMode)))
			{
				// Dim using the click-off scrim overlay, not Layer.Opacity — an overlay dims
				// everything underneath it (including the nav bar), unlike Layer.Opacity.
				_dimDetailWithScrim = true;
			}

			// RTL split mode: the narrow detail strip behind the flyout must be invisible so it
			// matches the baseline (white background), while remaining accessible for taps.
			// Layer.Opacity (unlike UIView.Alpha) doesn't affect UIKit's default hit-testing, so
			// zeroing it here hides the content visually without breaking tap/VoiceOver/Appium access.
			if (IsRTL && ShouldShowSplitMode && UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad)
			{
				opacity = 0;
			}
		}

		// RTL detail offset (phone mode)
		if (IsRTL && !FlyoutOverlapsDetailsInPopoverMode)
		{
			detailFrame.X = detailFrame.X * -1;
		}

		// Dim the detail content via the click-off scrim overlay rather than Layer.Opacity — see
		// the comment above where _dimDetailWithScrim is set for why.
		if (_clickOffView is not null)
		{
			_clickOffView.BackgroundColor = _dimDetailWithScrim ? ColorExtensions.BackgroundColor.ColorWithAlpha(0.5f) : UIColor.Clear;
		}

		// Animate or set detail frame
		var detailChildView = _detailVC?.View;
		if (animated && !FlyoutOverlapsDetailsInPopoverMode)
		{
			UIView.Animate(0.250, 0, UIViewAnimationOptions.CurveEaseOut, () =>
			{
				_detailContainerView.Frame = detailFrame;
				if (detailChildView is not null)
				{
					detailChildView.Layer.Opacity = (float)opacity;
				}
			}, () => { });
		}
		else
		{
			_detailContainerView.Frame = detailFrame;
			if (detailChildView is not null)
			{
				detailChildView.Layer.Opacity = (float)opacity;
			}
		}

		// Calculate flyout frame for overlap mode (iPad popover)
		if (FlyoutOverlapsDetailsInPopoverMode)
		{
			if (!_isPresented)
			{
				if (!IsRTL)
				{
					flyoutFrame.X -= flyoutFrame.Width;
				}
				else
				{
					flyoutFrame.X = frame.Width;
				}
			}
			else if (IsRTL)
			{
				if (ShouldShowSplitMode)
				{
					flyoutFrame.X = detailFrame.Width;
				}
				else
				{
					flyoutFrame.X = frame.Width - flyoutFrame.Width;
				}
			}
		}

		// Animate or set flyout frame
		if (animated && FlyoutOverlapsDetailsInPopoverMode)
		{
			UIView.Animate(0.250, 0, UIViewAnimationOptions.CurveEaseOut, () =>
			{
				_flyoutContainerView.Frame = flyoutFrame;
				if (detailChildView is not null)
				{
					detailChildView.Layer.Opacity = (float)opacity;
				}
			}, () => { });
		}
		else
		{
			_flyoutContainerView.Frame = flyoutFrame;
		}

		// Resize child VC views to fill containers
		ResizeChildToContainer(_flyoutVC, _flyoutContainerView);
		ResizeChildToContainer(_detailVC, _detailContainerView);

		// Notify delegate of bounds
		NotifyLayoutBoundsChanged(flyoutFrame, detailFrame, frame);

		if (_isPresented)
		{
			UpdateClickOffViewFrame();
		}
	}

	static void ResizeChildToContainer(UIViewController? childVC, UIView containerView)
	{
		if (childVC?.View is not null)
		{
			childVC.View.Frame = containerView.Bounds;
		}
	}

	nfloat GetFlyoutWidth(CGRect containerFrame)
	{
		if (_flyoutWidth > 0)
		{
			return (nfloat)_flyoutWidth;
		}

		// iPad: always 320pt (matches old SlideFlyoutTransition and UIKit popover default).
		// iPhone: 80% of shorter dimension regardless of overlay mode — matches old renderer.
		if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
		{
			return 320;
		}

		return (nfloat)(int)(Math.Min(containerFrame.Width, containerFrame.Height) * 0.8);
	}


	void SetPresented(bool value, bool animated, bool notifyDelegate)
	{
		if (_isPresented == value && _initialLayoutFinished)
		{
			UpdateClickOffView();
			return;
		}

		_isPresented = value;
		LayoutPanes(animated);
		UpdateClickOffView();
		ToggleAccessibilityElementsHidden();

		if (notifyDelegate)
		{
			if (_delegateRef.TryGetTarget(out var del))
			{
				del.OnPresentedChangedByGesture(value);
			}
		}
	}


	void UpdateClickOffView()
	{
		if (_clickOffView is null)
		{
			return;
		}

		if (FlyoutOverlapsDetailsInPopoverMode && ShouldShowSplitMode)
		{
			RemoveClickOffView();
			return;
		}

		if (_isPresented)
		{
			AddClickOffView();
		}
		else
		{
			RemoveClickOffView();
		}
	}

	void AddClickOffView()
	{
		var parentView = ParentView;
		if (_clickOffView is null || parentView is null)
		{
			return;
		}

		if (_clickOffView.Superview == parentView)
		{
			return;
		}

		// In overlay mode, insert below the flyout so the flyout covers the left portion of any
		// gradient backdrop — matching the old ShellFlyoutRenderer TapoffView behavior.
		if (FlyoutOverlapsDetailsInPopoverMode && _flyoutContainerView is not null)
		{
			parentView.InsertSubviewBelow(_clickOffView, _flyoutContainerView);
		}
		else
		{
			parentView.AddSubview(_clickOffView);
		}

		UpdateClickOffViewFrame();
	}

	void UpdateClickOffViewFrame()
	{
		if (_clickOffView is null || _flyoutContainerView is null || _detailContainerView is null)
		{
			return;
		}

		if (FlyoutOverlapsDetailsInPopoverMode)
		{
			// Full-width frame — the flyout container sits on top and covers the left portion.
			// This lets a gradient backdrop render across the full width while only the
			// area to the right of the flyout is visible, matching the old renderer behavior.
			var detailsFrame = _detailContainerView.Frame;
			_clickOffView.Frame = new CGRect(0, detailsFrame.Y, detailsFrame.Width, detailsFrame.Height);
		}
		else
		{
			_clickOffView.Frame = _detailContainerView.Frame;
		}
	}

	void RemoveClickOffView()
	{
		_clickOffView?.RemoveFromSuperview();
	}


	void PackContainers(UIView parentView)
	{
		if (_flyoutContainerView is null || _detailContainerView is null)
		{
			return;
		}

		if (!FlyoutOverlapsDetailsInPopoverMode)
		{
			// Phone: flyout behind, detail on top
			parentView.AddSubview(_flyoutContainerView);
			parentView.AddSubview(_detailContainerView);
		}
		else
		{
			// iPad: detail behind, flyout on top
			parentView.AddSubview(_detailContainerView);
			parentView.AddSubview(_flyoutContainerView);
		}

		// Re-apply any colors requested before the containers existed (or across a future
		// teardown/re-pack), so SetShadowBackgroundColor/SetScrimColor calls are never lost.
		if (_shadowBackgroundColor is not null)
		{
			_detailContainerView.BackgroundColor = _shadowBackgroundColor;
		}

		if (_clickOffView is not null && _scrimColor is not null)
		{
			_clickOffView.BackgroundColor = _scrimColor;
		}
	}


	void SetupTapGesture()
	{
		if (_clickOffView is null)
		{
			return;
		}

		_tapGesture = new UITapGestureRecognizer(() =>
		{
			SetPresented(false, animated: true, notifyDelegate: true);
		});

		if (FlyoutOverlapsDetailsInPopoverMode)
		{
			_tapGesture.ShouldReceiveTouch = (_, _) =>
				!ShouldShowSplitMode && _isPresented;
		}

		_clickOffView.AddGestureRecognizer(_tapGesture);
	}


	void UpdatePanGesture()
	{
		var parentView = ParentView;
		if (parentView is null)
		{
			return;
		}

		if (!_isGestureEnabled)
		{
			if (_panGesture is not null)
			{
				parentView.RemoveGestureRecognizer(_panGesture);
			}
			return;
		}

		if (_panGesture is not null)
		{
			parentView.AddGestureRecognizer(_panGesture);
			return;
		}

		var center = new PointF();
		_panGesture = new UIPanGestureRecognizer(g =>
		{
			int directionModifier = IsRTL ? -1 : 1;

			switch (g.State)
			{
				case UIGestureRecognizerState.Began:
					center = g.LocationInView(g.View);
					break;

				case UIGestureRecognizerState.Changed:
					HandlePanChanged(g, center, directionModifier);
					break;

				case UIGestureRecognizerState.Ended:
					HandlePanEnded(directionModifier);
					break;
			}
		});

		_panGesture.CancelsTouchesInView = false;
		_panGesture.ShouldReceiveTouch = (_, t) =>
			!(t.View is UISlider) &&
			!IsSwipeView(t.View) &&
			!ShouldShowSplitMode &&
			_flyoutBehavior != FlyoutBehavior.Disabled;
		_panGesture.MaximumNumberOfTouches = 2;

		parentView.AddGestureRecognizer(_panGesture);
	}

	void HandlePanChanged(UIPanGestureRecognizer g, PointF center, int directionModifier)
	{
		if (_flyoutContainerView is null || _detailContainerView is null)
		{
			return;
		}

		var currentPosition = g.LocationInView(g.View);
		var motion = (currentPosition.X - center.X) * directionModifier;

		if (!FlyoutOverlapsDetailsInPopoverMode)
		{
			// Phone mode: move detail view
			var targetFrame = _detailContainerView.Frame;
			var flyoutWidth = _flyoutContainerView.Frame.Width;

			if (_isPresented)
			{
				targetFrame.X = (nfloat)Math.Max(0, flyoutWidth + Math.Min(0, motion));
			}
			else
			{
				targetFrame.X = (nfloat)Math.Min(flyoutWidth, Math.Max(0, motion));
			}

			targetFrame.X = targetFrame.X * directionModifier;
			ApplyShadowDuringGesture(targetFrame);
			_detailContainerView.Frame = targetFrame;
		}
		else
		{
			// iPad popover mode: move flyout view
			var targetFrame = _flyoutContainerView.Frame;
			var flyoutWidth = _flyoutContainerView.Frame.Width;

			if (_isPresented)
			{
				targetFrame.X = (nfloat)Math.Max(-flyoutWidth, Math.Min(0, motion));
			}
			else
			{
				targetFrame.X = (nfloat)Math.Min(0, Math.Max(0, motion) - flyoutWidth);
			}

			if (IsRTL)
			{
				var containerWidth = ParentView!.Bounds.Width;
				targetFrame.X = (nfloat)(containerWidth - (flyoutWidth + targetFrame.X));
			}

			ApplyShadowDuringGesture(targetFrame);
			_flyoutContainerView.Frame = targetFrame;
		}
	}

	void HandlePanEnded(int directionModifier)
	{
		if (_flyoutContainerView is null || _detailContainerView is null)
		{
			return;
		}

		if (!FlyoutOverlapsDetailsInPopoverMode)
		{
			var detailFrame = _detailContainerView.Frame;
			var flyoutWidth = _flyoutContainerView.Frame.Width;

			if (_isPresented)
			{
				if (detailFrame.X * directionModifier < flyoutWidth * 0.75)
				{
					SetPresented(false, animated: true, notifyDelegate: true);
				}
				else
				{
					LayoutPanes(animated: true);
				}
			}
			else
			{
				if (detailFrame.X * directionModifier > flyoutWidth * 0.25)
				{
					SetPresented(true, animated: true, notifyDelegate: true);
				}
				else
				{
					LayoutPanes(animated: true);
				}
			}
		}
		else
		{
			var flyoutFrame = _flyoutContainerView.Frame;
			var flyoutOffsetX = flyoutFrame.X + flyoutFrame.Width;

			if (IsRTL)
			{
				flyoutOffsetX = (nfloat)(ParentView!.Bounds.Width - flyoutFrame.X);
			}

			var flyoutWidth = flyoutFrame.Width;

			if (_isPresented)
			{
				if (flyoutOffsetX < flyoutWidth * 0.75)
				{
					SetPresented(false, animated: true, notifyDelegate: true);
				}
				else
				{
					LayoutPanes(animated: true);
				}
			}
			else
			{
				if (flyoutOffsetX > flyoutWidth * 0.25)
				{
					SetPresented(true, animated: true, notifyDelegate: true);
				}
				else
				{
					LayoutPanes(animated: true);
				}
			}
		}
	}

	void ApplyShadowDuringGesture(CGRect targetFrame)
	{
		if (!_applyShadow || _flyoutContainerView is null)
		{
			return;
		}

		var detailChildView = _detailVC?.View;
		if (detailChildView is null)
		{
			return;
		}

		var flyoutWidth = _flyoutContainerView.Frame.Width;
		nfloat openProgress;

		if (!FlyoutOverlapsDetailsInPopoverMode)
		{
			openProgress = !IsRTL
				? targetFrame.X / flyoutWidth
				: (nfloat)((ParentView!.Bounds.Width - targetFrame.GetMaxX()) / flyoutWidth);
		}
		else
		{
			openProgress = !IsRTL
				? (targetFrame.X + flyoutWidth) / flyoutWidth
				: (nfloat)((ParentView!.Bounds.Width - targetFrame.X) / flyoutWidth);
		}

		var opacity = (float)(0.5 + (0.5 * (1 - openProgress)));
		detailChildView.Layer.Opacity = opacity;
	}


	void ToggleAccessibilityElementsHidden()
	{
		if (_flyoutContainerView is not null)
		{
			_flyoutContainerView.AccessibilityElementsHidden = !_isPresented;
		}

		if (_detailContainerView is not null)
		{
			// Use _flyoutOverlapsDetail (not the idiom-inclusive FlyoutOverlapsDetailsInPopoverMode)
			// so only Shell's explicit "always overlay" mode skips hiding. Otherwise FlyoutPage on
			// iPad would wrongly stay "accessible" while the scrim still blocks taps on it.
			// Also hide whenever the click-off scrim is actively dimming the detail pane
			// (_dimDetailWithScrim, computed in PerformLayout): Shell hard-codes
			// _flyoutOverlapsDetail = true, so without this OR, VoiceOver/Appium could still reach
			// detail content the scrim is visually blocking in Shell's always-overlay case.
			bool shouldHideDetail = _isPresented && !ShouldShowSplitMode && (!_flyoutOverlapsDetail || _dimDetailWithScrim);
			_detailContainerView.AccessibilityElementsHidden = shouldHideDetail;
		}
	}


	static bool IsSwipeView(UIView? view)
	{
		if (view is null)
		{
			return false;
		}

		if (view.Superview is MauiSwipeView)
		{
			return true;
		}

		return IsSwipeView(view.Superview);
	}

	void NotifyLayoutBoundsChanged(CGRect flyoutFrame, CGRect detailFrame, CGRect containerFrame)
	{
		if (!_delegateRef.TryGetTarget(out var del))
		{
			return;
		}

		var flyoutBounds = new Rect(flyoutFrame.X, 0, flyoutFrame.Width, flyoutFrame.Height);
		var detailBounds = new Rect(detailFrame.X, 0, containerFrame.Width, containerFrame.Height);
		del.OnLayoutBoundsChanged(flyoutBounds, detailBounds);
	}

	void NotifyLeftBarButtonNeedsUpdate()
	{
		if (_delegateRef.TryGetTarget(out var del))
		{
			del.OnLeftBarButtonNeedsUpdate();
		}
	}
}
