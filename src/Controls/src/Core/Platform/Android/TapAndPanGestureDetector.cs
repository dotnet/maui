using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Views;

namespace Microsoft.Maui.Controls.Platform
{

	class TapAndPanGestureDetector : GestureDetector
	{
		InnerGestureListener? _listener;
		PointerGestureHandler? _pointerGestureHandler;

		public TapAndPanGestureDetector(Context context, InnerGestureListener listener) : base(context, listener)
		{
			_listener = listener;
			UpdateLongPressSettings();
		}

		public void SetPointerGestureHandler(PointerGestureHandler pointerGestureHandler)
		{
			_pointerGestureHandler = pointerGestureHandler;
		}

		public void UpdateLongPressSettings()
		{
			if (_listener == null)
				return;

			// Right now this just disables long press, since we don't support a long press gesture
			// in Forms. If we ever do, we'll need to selectively enable it, probably by hooking into the 
			// InnerGestureListener and listening for the addition of any long press gesture recognizers.
			// (since a long press will prevent a pan gesture from starting, we can't just leave support for it 
			// on by default).
			// Also, since the property is virtual we shouldn't just set it from the constructor.

			IsLongpressEnabled = _listener.EnableLongPressGestures;
		}

		public override bool OnTouchEvent(MotionEvent ev)
		{
			var handled = base.OnTouchEvent(ev);

			if (_pointerGestureHandler != null && ev?.Action is
				MotionEventActions.Up or MotionEventActions.Down or MotionEventActions.Cancel)
			{
				_pointerGestureHandler.OnTouch(ev);
			}

			// Always call EndScrolling on ACTION_UP to ensure swipe gestures are completed,
			// regardless of whether the base gesture detector consumed the event.
			// This fixes SwipeGestureRecognizer not working on scrollable views like CollectionView.
			if (_listener != null && ev?.Action == MotionEventActions.Up)
				_listener.EndScrolling();

			return handled;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (_listener != null)
				{
					_listener.Dispose();
					_listener = null;
				}
				_pointerGestureHandler = null;
			}
		}
	}
}
