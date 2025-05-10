using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items;

internal class MauiCollectionView : UICollectionView, IUIViewLifeCycleEvents, IPlatformMeasureInvalidationController
{
	bool _invalidateParentWhenMovedToWindow;

	WeakReference<ICustomMauiCollectionViewDelegate>? _customDelegate;

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
}