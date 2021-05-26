using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
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

			global::Android.Views.View currentView = Context.GetActivity().CurrentFocus;
			bool result = base.DispatchTouchEvent(e);

			do
			{
				if (!(currentView is EditText))
					break;

				global::Android.Views.View newCurrentView = Context.GetActivity().CurrentFocus;

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
				Context.GetActivity().Window.DecorView.ClearFocus();
			} while (false);

			return result;
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			Profile.FrameBegin();
			SetMeasuredDimension(r - l, b - t);
			_canvas?.OnLayout(changed, l, t, r, b);
			Profile.FrameEnd();
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			SetMeasuredDimension(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));

			var width = MeasureSpecFactory.GetSize(widthMeasureSpec);
			var height = MeasureSpecFactory.GetSize(heightMeasureSpec);

			for (int i = 0; i < ChildCount; i++)
			{
				var child = GetChildAt(i);
				child.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
					MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
			}
		}
	}
}