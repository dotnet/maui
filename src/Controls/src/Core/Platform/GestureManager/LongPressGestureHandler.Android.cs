using System;
using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
class LongPressGestureHandler : IDisposable
{
readonly Func<View?> _viewGetter;
readonly Func<AView?> _controlGetter;
readonly Func<IViewHandler?> _handlerGetter;
System.Threading.Timer? _timer;
float _startX, _startY;
bool _isLongPressing;

public LongPressGestureHandler(Func<View?> viewGetter, Func<AView?> controlGetter, Func<IViewHandler?> handlerGetter)
{
_viewGetter = viewGetter;
_controlGetter = controlGetter;
_handlerGetter = handlerGetter;
}

public void OnTouchEvent(MotionEvent? e)
{
if (e == null)
return;

var view = _viewGetter();
if (view == null)
return;

var recognizers = view.GestureRecognizers;
if (recognizers == null || recognizers.Count == 0)
return;

switch (e.Action)
{
case MotionEventActions.Down:
_startX = e.GetX();
_startY = e.GetY();
_isLongPressing = false;
StartTimers(view, new Point(_startX, _startY));
break;

case MotionEventActions.Move:
// Check allowable movement for all long press recognizers
var deltaX = Math.Abs(e.GetX() - _startX);
var deltaY = Math.Abs(e.GetY() - _startY);

foreach (var recognizer in recognizers)
{
if (recognizer is LongPressGestureRecognizer longPress)
{
if (deltaX > longPress.AllowableMovement || deltaY > longPress.AllowableMovement)
{
CancelTimers();
break;
}
}
}
break;

case MotionEventActions.Up:
case MotionEventActions.Cancel:
CancelTimers();
break;
}
}

void StartTimers(View view, Point position)
{
var recognizers = view.GestureRecognizers;
if (recognizers == null)
return;

foreach (var recognizer in recognizers)
{
if (recognizer is LongPressGestureRecognizer longPress)
{
var duration = longPress.MinimumPressDuration;
_timer = new System.Threading.Timer(_ =>
{
// Dispatch to main thread using Android's Post
var control = _controlGetter();
control?.Post(() =>
{
if (_isLongPressing)
return; // Already fired

_isLongPressing = true;
longPress.SendLongPressed(view, position);
longPress.SendLongPressing(view, GestureStatus.Completed, position);
});
}, null, duration, System.Threading.Timeout.Infinite);
}
}
}

void CancelTimers()
{
_timer?.Dispose();
_timer = null;
_isLongPressing = false;
}

public void Dispose()
{
CancelTimers();
}
}
}
