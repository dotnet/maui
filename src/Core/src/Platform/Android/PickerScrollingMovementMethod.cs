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
	bool _isDragging;

	public override bool OnTouchEvent(TextView? widget, ISpannable? buffer, MotionEvent? e)
	{
		if (widget is null || buffer is null || e is null)
		{
			return base.OnTouchEvent(widget, buffer, e);
		}

		switch (e.ActionMasked)
		{
			case MotionEventActions.Down:
				_downX = e.GetX();
				_downY = e.GetY();
				_touchSlop = ViewConfiguration.Get(widget.Context!)!.ScaledTouchSlop;
				_isDragging = false;
				break;
			case MotionEventActions.Move:
				var deltaX = global::System.Math.Abs(e.GetX() - _downX);
				var deltaY = global::System.Math.Abs(e.GetY() - _downY);
				_isDragging |= deltaX > _touchSlop && deltaX > deltaY;

				if (_isDragging)
				{
					// TextView posts PerformClick on Up unless pressed state is cleared first.
					widget.Pressed = false;
				}
				break;
		}

		var handled = base.OnTouchEvent(widget, buffer, e);

		if (e.ActionMasked is MotionEventActions.Up or MotionEventActions.Cancel)
		{
			_isDragging = false;
		}

		return handled;
	}
}
