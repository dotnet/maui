using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;
using ARect = Android.Graphics.Rect;
using Rectangle = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui.Platform
{
	public class LayoutViewGroup : ViewGroup
	{
		Point _downPosition;
		DateTime _downTime;

		readonly ARect _clipRect = new();

		public bool InputTransparent { get; set; }

		public LayoutViewGroup(Context context) : base(context)
		{
		}

		public LayoutViewGroup(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public LayoutViewGroup(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public LayoutViewGroup(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		public LayoutViewGroup(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
		{
		}

		public bool ClipsToBounds { get; set; }

		public override bool DispatchTouchEvent(MotionEvent? e)
		{
			if (e?.Action == MotionEventActions.Down)
			{
				_downTime = DateTime.UtcNow;
				_downPosition = new Point(e.RawX, e.RawY);
			}

			if (e?.Action != MotionEventActions.Up)
				return base.DispatchTouchEvent(e);

			View? currentView = Context?.GetActivity()?.CurrentFocus;
			bool result = base.DispatchTouchEvent(e);

			do
			{
				if (currentView is not EditText)
					break;

				View? newCurrentView = Context?.GetActivity()?.CurrentFocus;

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

				Context?.HideKeyboard(currentView);
				Context?.GetActivity()?.Window?.DecorView.ClearFocus();
			} while (false);

			return result;
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Context == null)
			{
				return;
			}

			if (CrossPlatformMeasure == null)
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
				return;
			}

			var deviceIndependentWidth = widthMeasureSpec.ToDouble(Context);
			var deviceIndependentHeight = heightMeasureSpec.ToDouble(Context);

			var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
			var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

			var measure = CrossPlatformMeasure(deviceIndependentWidth, deviceIndependentHeight);

			// If the measure spec was exact, we should return the explicit size value, even if the content
			// measure came out to a different size
			var width = widthMode == MeasureSpecMode.Exactly ? deviceIndependentWidth : measure.Width;
			var height = heightMode == MeasureSpecMode.Exactly ? deviceIndependentHeight : measure.Height;

			var platformWidth = Context.ToPixels(width);
			var platformHeight = Context.ToPixels(height);

			// Minimum values win over everything
			platformWidth = Math.Max(MinimumWidth, platformWidth);
			platformHeight = Math.Max(MinimumHeight, platformHeight);

			SetMeasuredDimension((int)platformWidth, (int)platformHeight);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (CrossPlatformArrange == null || Context == null)
			{
				return;
			}

			var destination = Context!.ToCrossPlatformRectInReferenceFrame(l, t, r, b);

			CrossPlatformArrange(destination);

			if (ClipsToBounds)
			{
				_clipRect.Right = r - l;
				_clipRect.Bottom = b - t;
				ClipBounds = _clipRect;
			}
			else
			{
				ClipBounds = null;
			}
		}

		public override bool OnTouchEvent(MotionEvent? e)
		{
			if (InputTransparent)
			{
				return false;
			}

			return base.OnTouchEvent(e);
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }
	}
}
