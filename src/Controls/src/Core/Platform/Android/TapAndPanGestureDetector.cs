﻿using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Views;

namespace Microsoft.Maui.Controls.Platform
{

	class TapAndPanGestureDetector : GestureDetector
	{
		InnerGestureListener? _listener;
		public TapAndPanGestureDetector(Context context, InnerGestureListener listener) : base(context, listener)
		{
			_listener = listener;
			UpdateLongPressSettings();
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
			if (base.OnTouchEvent(ev))
				return true;

			if (_listener != null && ev?.Action == MotionEventActions.Up)
				_listener.EndScrolling();

			return false;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				_listener?.Dispose();
				_listener = null;
			}
		}
	}
}
