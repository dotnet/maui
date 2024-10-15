using System;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items;

internal class MauiCollectionView : UICollectionView, IUIViewLifeCycleEvents
{
	WeakReference<ICustomMauiCollectionViewDelegate>? _customDelegate;
	bool _cellMeasureInvalidated;

	public MauiCollectionView(CGRect frame, UICollectionViewLayout layout) : base(frame, layout)
	{
	}

	public override void ScrollRectToVisible(CGRect rect, bool animated)
	{
		if (!KeyboardAutoManagerScroll.IsKeyboardAutoScrollHandling)
			base.ScrollRectToVisible(rect, animated);
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

		if(_customDelegate?.TryGetTarget(out var target) == true)
		{
			target.MovedToWindow(this);
		}
	}

	public override void LayoutSubviews()
	{
		if (_cellMeasureInvalidated)
		{
			_cellMeasureInvalidated = false;

			var cells = VisibleCells;
			var cellsLength = cells.Length;
			for (int i = 0; i < cellsLength; i++)
			{
				(cells[i] as TemplatedCell)?.MeasureIfNeeded();
			}
		}

		base.LayoutSubviews();
	}

	internal void CellMeasureInvalidated()
	{
		_cellMeasureInvalidated = true;
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