using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal class PlatformRenderer : ViewGroup
	{
		readonly IPlatformLayout _canvas;
		Point _downPosition;

		DateTime _downTime;

		public PlatformRenderer(Context context, IPlatformLayout canvas) : base(context)
		{
			_canvas = canvas;
			Focusable = true;
			FocusableInTouchMode = true;
		}

		public override bool DispatchTouchEvent(MotionEvent e)
		{
			if (e.Action == MotionEventActions.Down)
			{
				_downTime = DateTime.UtcNow;
				_downPosition = new Point(e.RawX, e.RawY);
			}

			if (e.Action != MotionEventActions.Up)
				return base.DispatchTouchEvent(e);

			global::Android.Views.View currentView = ((Activity)Context).CurrentFocus;
			bool result = base.DispatchTouchEvent(e);

			do
			{
				if (!(currentView is EditText))
					break;

				global::Android.Views.View newCurrentView = ((Activity)Context).CurrentFocus;

				if (currentView != newCurrentView)
					break;

				double distance = _downPosition.Distance(new Point(e.RawX, e.RawY));

				if (distance > Context.ToPixels(20) || DateTime.UtcNow - _downTime > TimeSpan.FromMilliseconds(200))
					break;

				var location = new int[2];
				currentView.GetLocationOnScreen(location);

				float x = e.RawX + currentView.Left - location[0];
				float y = e.RawY + currentView.Top - location[1];

				var rect = new Rectangle(currentView.Left, currentView.Top, currentView.Width, currentView.Height);

				if (rect.Contains(x, y))
					break;

				Context.HideKeyboard(currentView);
				((Activity)Context).Window.DecorView.ClearFocus();
			} while (false);

			return result;
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			SetMeasuredDimension(r - l, b - t);
			_canvas?.OnLayout(changed, l, t, r, b);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));
		}
	}
}