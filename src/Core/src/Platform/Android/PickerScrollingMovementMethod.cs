using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Platform;

sealed class PickerScrollingMovementMethod : ScrollingMovementMethod
{
	float _downX;
	float _downY;
	int _touchSlop;
	int _activePointerId = InvalidPointerId;
	int _gestureId;
	bool _suppressClick;

	const int InvalidPointerId = -1;

	public bool ConsumeClick()
	{
		if (!_suppressClick)
			return false;

		_suppressClick = false;
		return true;
	}

	public override bool OnTouchEvent(TextView? widget, ISpannable? buffer, MotionEvent? e)
	{
		if (widget is null || buffer is null || e is null)
		{
			return base.OnTouchEvent(widget, buffer, e);
		}

		switch (e.ActionMasked)
		{
			case MotionEventActions.Down:
				_gestureId++;
				_activePointerId = e.GetPointerId(0);
				_downX = e.GetX(0);
				_downY = e.GetY(0);
				var context = widget.Context;
				_touchSlop = context is null ? 0 : ViewConfiguration.Get(context)?.ScaledTouchSlop ?? 0;
				_suppressClick = false;
				break;
			case MotionEventActions.Move:
				UpdateClickSuppression(e);
				break;
			case MotionEventActions.PointerUp:
				UpdateActivePointer(e);
				break;
			case MotionEventActions.Up:
				UpdateClickSuppression(e);
				_activePointerId = InvalidPointerId;
				break;
			case MotionEventActions.Cancel:
				_activePointerId = InvalidPointerId;
				_suppressClick = false;
				break;
		}

		var handled = base.OnTouchEvent(widget, buffer, e);

		if (e.ActionMasked == MotionEventActions.Up && _suppressClick)
		{
			var gestureId = _gestureId;
			widget.Post(new Java.Lang.Runnable(() =>
			{
				if (_gestureId == gestureId)
					_suppressClick = false;
			}));
		}

		return handled;
	}

	void UpdateClickSuppression(MotionEvent e)
	{
		var pointerIndex = e.FindPointerIndex(_activePointerId);

		if (pointerIndex < 0)
			pointerIndex = 0;

		var deltaX = global::System.Math.Abs(e.GetX(pointerIndex) - _downX);
		var deltaY = global::System.Math.Abs(e.GetY(pointerIndex) - _downY);
		_suppressClick |= global::System.Math.Max(deltaX, deltaY) > _touchSlop;
	}

	void UpdateActivePointer(MotionEvent e)
	{
		var actionIndex = e.ActionIndex;

		if (e.GetPointerId(actionIndex) != _activePointerId || e.PointerCount <= 1)
			return;

		var newPointerIndex = actionIndex == 0 ? 1 : 0;
		_activePointerId = e.GetPointerId(newPointerIndex);
		_downX = e.GetX(newPointerIndex);
		_downY = e.GetY(newPointerIndex);
	}
}
