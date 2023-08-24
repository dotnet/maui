using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls
{
	class TapWindowTracker
	{
		bool _hasFocus;
		bool _initialFocusSetup;
		AView? _platformView;
		GestureDetector? _gestureDetector;
		GestureListener? _gestureListener;
		HideSoftInputOnTappedChangedManager? _hideSoftInputOnTappedChangedManager;

		public TapWindowTracker(AView platformView, HideSoftInputOnTappedChangedManager hideSoftInputOnTappedChangedManager)
		{
			_platformView = platformView;
			_hideSoftInputOnTappedChangedManager = hideSoftInputOnTappedChangedManager;

			_platformView.FocusChange += OnFocusChanged;
			_hideSoftInputOnTappedChangedManager.DispatchTouchEvent += OnDispatchTouchEvent;
		}


		public void Disconnect()
		{
			SetupFocus(false);

			if (_platformView != null)
			{
				_platformView.FocusChange -= OnFocusChanged;
				_platformView = null;
			}

			if (_hideSoftInputOnTappedChangedManager is not null)
			{
				_hideSoftInputOnTappedChangedManager.DispatchTouchEvent -= OnDispatchTouchEvent;
				_hideSoftInputOnTappedChangedManager = null;
			}
		}

		void OnFocusChanged(object? sender, AView.FocusChangeEventArgs e) =>
			SetupFocus(e.HasFocus);

		void SetupFocus(bool hasFocus)
		{
			_initialFocusSetup = true;
			if (hasFocus && _platformView is not null)
			{
				_gestureListener = new GestureListener(_platformView);
				_gestureDetector = new GestureDetector(_platformView.Context, _gestureListener);
			}
			else
			{
				_gestureListener?.Disconnect();
				_gestureListener = null;
				_gestureDetector = null;
			}

			_hasFocus = hasFocus;
		}

		public void OnDispatchTouchEvent(object? sender, MotionEvent? e)
		{
			if (!_initialFocusSetup && _platformView is not null)
			{
				SetupFocus(_platformView.HasFocus);
			}

			if (!_hasFocus || _platformView is null || this is null || e is null)
				return;

			_gestureDetector?.OnTouchEvent(e);
		}

		class GestureListener : Java.Lang.Object, GestureDetector.IOnGestureListener
		{
			AView? _platformView;

			public GestureListener(AView platformView)
			{
				_platformView = platformView;
			}

			public void Disconnect()
			{
				_platformView = null;
			}

			public bool OnDown(MotionEvent e)
			{
				return false;
			}

			public bool OnFling(MotionEvent? e1, MotionEvent e2, float velocityX, float velocityY)
			{
				return false;
			}

			public void OnLongPress(MotionEvent e)
			{
			}

			public bool OnScroll(MotionEvent? e1, MotionEvent e2, float distanceX, float distanceY)
			{
				return false;
			}

			public void OnShowPress(MotionEvent e)
			{
			}

			public bool OnSingleTapUp(MotionEvent e)
			{
				var context = _platformView?.Context;
				if (context is null || _platformView is null)
					return false;

				var location = _platformView.GetBoundingBox();

				var point =
					new Point(
						context.FromPixels(e.RawX),
						context.FromPixels(e.RawY)
					);

				if (location.Contains(point))
					return true;

				if (_platformView.IsSoftInputShowing())
					_platformView.HideSoftInput();

				return true;
			}
		}
	}
}
