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

			// Enable long press when drag gestures or LongPressGestureRecognizer are present.
			// Note: long press being enabled will prevent pan gestures from starting until
			// the long press timeout elapses, so we only enable it when needed.
			IsLongpressEnabled = _listener.EnableLongPressGestures;
		}

		public override bool OnTouchEvent(MotionEvent ev)
		{
			if (base.OnTouchEvent(ev))
				return true;

			if (_pointerGestureHandler != null && ev?.Action is
				MotionEventActions.Up or MotionEventActions.Down or MotionEventActions.Cancel)
			{
				_pointerGestureHandler.OnTouch(ev);
			}

			if (_listener != null && ev?.Action == MotionEventActions.Up)
				_listener.EndScrolling();

			return false;
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
