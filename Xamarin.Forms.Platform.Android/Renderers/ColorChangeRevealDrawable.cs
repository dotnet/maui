using Android.Animation;
using Android.Graphics;
using Android.Graphics.Drawables;
using System;
using AColor = Android.Graphics.Color;

namespace Xamarin.Forms.Platform.Android
{
	public class ColorChangeRevealDrawable : AnimationDrawable
	{
		readonly Point _center;
		readonly AColor _endColor;
		readonly AColor _startColor;
		float _progress;

		public ColorChangeRevealDrawable(AColor startColor, AColor endColor, Point center) : base()
		{
			_startColor = startColor;
			_endColor = endColor;

			if (_startColor != _endColor)
			{
				ValueAnimator animator = ValueAnimator.OfFloat(0, 1);
				animator.SetInterpolator(new global::Android.Views.Animations.DecelerateInterpolator());
				animator.SetDuration(500);
				animator.Update += OnUpdate;
				animator.Start();
				_center = center;
			}
			else
			{
				_progress = 1;
			}
		}

		public override void Draw(Canvas canvas)
		{
			if (_progress == 1)
			{
				canvas.DrawColor(_endColor);
				return;
			}

			canvas.DrawColor(_startColor);
			var bounds = Bounds;
			float centerX = (float)_center.X;
			float centerY = (float)_center.Y;

			float width = bounds.Width();
			float distanceFromCenter = (float)Math.Abs(width / 2 - _center.X);
			float radius = (width / 2 + distanceFromCenter) * 1.1f;

			var paint = new Paint
			{
				Color = _endColor
			};
			canvas.DrawCircle(centerX, centerY, radius * _progress, paint);
		}

		void OnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs e)
		{
			_progress = (float)e.Animation.AnimatedValue;
			if (!this.IsDisposed())
				InvalidateSelf();
		}
	}
}