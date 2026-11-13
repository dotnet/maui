using Android.Views;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform;

sealed class PickerDragGestureFilter
{
	int _touchSlop;
	int _activePointerId = InvalidPointerId;
	float _anchorX;
	float _anchorY;
	bool _isDrag;

	const int InvalidPointerId = -1;

	public bool ShouldCancelClick(AView view, MotionEvent? e)
	{
		if (e is null)
			return false;

		switch (e.ActionMasked)
		{
			case MotionEventActions.Down:
				var context = view.Context;
				_touchSlop = context is null ? 0 : ViewConfiguration.Get(context)?.ScaledTouchSlop ?? 0;
				_activePointerId = e.GetPointerId(0);
				_anchorX = e.GetX(0);
				_anchorY = e.GetY(0);
				_isDrag = false;
				return false;
			case MotionEventActions.Move:
				return TryClassifyDrag(e);
			case MotionEventActions.PointerUp:
				var cancelClick = TryClassifyDrag(e);
				ReAnchor(e);
				return cancelClick;
			case MotionEventActions.Up:
				cancelClick = TryClassifyDrag(e);
				Reset();
				return cancelClick;
			case MotionEventActions.Cancel:
				Reset();
				return false;
		}

		return false;
	}

	bool TryClassifyDrag(MotionEvent e)
	{
		if (_isDrag || _activePointerId == InvalidPointerId)
			return false;

		var pointerIndex = e.FindPointerIndex(_activePointerId);

		if (pointerIndex < 0)
			return false;

		var deltaX = global::System.Math.Abs(e.GetX(pointerIndex) - _anchorX);
		var deltaY = global::System.Math.Abs(e.GetY(pointerIndex) - _anchorY);

		if (global::System.Math.Max(deltaX, deltaY) < _touchSlop)
			return false;

		_isDrag = true;
		return true;
	}

	void ReAnchor(MotionEvent e)
	{
		var actionIndex = e.ActionIndex;

		if (e.GetPointerId(actionIndex) != _activePointerId || e.PointerCount <= 1)
			return;

		var newPointerIndex = actionIndex == 0 ? 1 : 0;
		_activePointerId = e.GetPointerId(newPointerIndex);
		_anchorX = e.GetX(newPointerIndex);
		_anchorY = e.GetY(newPointerIndex);
	}

	void Reset()
	{
		_activePointerId = InvalidPointerId;
		_isDrag = false;
	}
}
