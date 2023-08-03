using Android.Views;
using AView = Android.Views.View;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	class TapPageTracker
	{
		bool _hasFocus;
		AView? _platformView;
		GestureDetector? _gestureDetector;
		GestureListener? _gestureListener;
		ContentPage? _contentPage;

		public TapPageTracker(AView platformView, ContentPage contentPage)
		{
			_platformView = platformView;
			_contentPage = contentPage;

			_platformView.FocusChange += OnFocusChanged;
			_contentPage.DispatchTouchEvent += OnDispatchTouchEvent;
		}


		public void Disconnect()
		{
			SetupFocus(false);

			if (_platformView != null)
			{
				_platformView.FocusChange -= OnFocusChanged;
				_platformView = null;
			}

			if (_contentPage is not null)
			{
				_contentPage.DispatchTouchEvent -= OnDispatchTouchEvent;
				_contentPage = null;
			}
		}

		void OnFocusChanged(object? sender, AView.FocusChangeEventArgs e) =>
			SetupFocus(e.HasFocus);

		void SetupFocus(bool hasFocus)
		{
			System.Diagnostics.Debug.WriteLine($"FocusChanged {hasFocus}");
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
			if (!_hasFocus || _platformView is null || this is null || e is null)
				return;

			System.Diagnostics.Debug.WriteLine($"{e}");
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

			public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
			{
				return false;
			}

			public void OnLongPress(MotionEvent e)
			{
			}

			public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
			{
				return false;
			}

			public void OnShowPress(MotionEvent e)
			{
			}

			public bool OnSingleTapUp(MotionEvent e)
			{
				if (_platformView is null)
					return false;

				var location = GetBoundingBox(_platformView);
				var point = new Point(e.RawX, e.RawY);

				if (location.Contains(point))
					return true;

				if (KeyboardManager.IsSoftKeyboardVisible(_platformView))
					KeyboardManager.HideKeyboard(_platformView);

				return true;
			}

			Rect GetBoundingBox(AView view)
			{
				var context = view.Context;
				var rect = new Android.Graphics.Rect();
				view.GetGlobalVisibleRect(rect);

				return new Rect(
					 rect.ExactCenterX() - (rect.Width() / 2),
					 rect.ExactCenterY() - (rect.Height() / 2),
					 (float)rect.Width(),
					 (float)rect.Height());
			}
		}
	}
}
