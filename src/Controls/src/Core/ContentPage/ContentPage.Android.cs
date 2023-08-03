using System;
using System.Collections.Generic;
using System.Text;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui.Controls
{
	public partial class ContentPage : IPlatformEventsListener
	{
		bool IPlatformEventsListener.DispatchTouchEvent(MotionEvent? e)
		{
			return false;
		}

		public partial class TapToCloseBehavior
		{
			View _view;
			NotifyingContentViewGroup _platformPageView;
			bool _hasFocus;
			AView _platformView;
			GestureDetector _gestureDetector;
			GestureListener _gestureListener;

			protected override void OnAttachedTo(View bindable, AView platformView)
			{
				_view = bindable;
				_platformView = platformView;

				if (platformView is EditText)
					_platformView = platformView;
				else if (platformView is AViewGroup vg)
				{
					_platformView = vg.GetChildrenOfType<EditText>().FirstOrDefault()
						?? _platformView;
				}

				_platformView.FocusChange += OnFocusChanged;

				base.OnAttachedTo(bindable, platformView);

				SetupFocus(_platformView.HasFocus);
			}

			protected override void OnDetachedFrom(View bindable, AView platformView)
			{
				base.OnDetachedFrom(bindable, platformView);

				SetupFocus(false);

				if (_platformView != null)
				{
					_platformView.FocusChange -= OnFocusChanged;
					_platformView = null;
				}

				_view = null;
			}

			void OnFocusChanged(object sender, AView.FocusChangeEventArgs e) =>
				SetupFocus(e.HasFocus);

			void SetupFocus(bool hasFocus)
			{
				if (hasFocus)
				{
					if (_platformPageView != null)
						return;

					if (GetPage(_view).Handler.PlatformView is NotifyingContentViewGroup pagePlatformView)
						_platformPageView = pagePlatformView;
					else
						throw new Exception("You need to call builder.ConfigureMauiWorkarounds");

					_platformPageView.DispatchTouch += OnDispatchTouch;
					_gestureListener = new GestureListener(_view, _platformView);
					_gestureDetector = new GestureDetector(_platformView.Context, _gestureListener);
				}
				else if (_platformPageView != null)
				{
					_platformPageView.DispatchTouch -= OnDispatchTouch;
					_platformPageView = null;

					_gestureListener.Disconnect();
					_gestureListener = null;
					_gestureDetector = null;
				}

				_hasFocus = hasFocus;
			}

			void OnDispatchTouch(object sender, MotionEvent e)
			{
				if (!_hasFocus || _platformPageView == null || _platformView == null || _view == null)
					return;

				_gestureDetector.OnTouchEvent(e);
			}

			static Page GetPage(Element view)
			{
				if (view is Page page)
					return page;

				return GetPage(view.Parent);
			}

			class GestureListener : Java.Lang.Object, IOnGestureListener
			{
				View _view;
				AView _platformView;

				public GestureListener(View view, AView platformView)
				{
					_view = view;
					_platformView = platformView;
				}

				public void Disconnect()
				{
					_view = null;
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
					var location = GetBoundingBox(_platformView);
					var point = new Point(e.RawX, e.RawY);

					if (location.Contains(point))
						return true;

					if (KeyboardManager.IsSoftKeyboardVisible(_view))
						KeyboardManager.HideKeyboard(_view);

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
}
