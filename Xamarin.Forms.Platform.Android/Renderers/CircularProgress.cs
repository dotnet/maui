using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using AColor = Android.Graphics.Color;
using AProgressBar = Android.Widget.ProgressBar;

namespace Xamarin.Forms.Platform.Android
{
	internal class CircularProgress : AProgressBar
	{
		public int MaxSize { get; set; } = int.MaxValue;

		public int MinSize { get; set; } = 0;

		public AColor DefaultColor { get; set; }

		const int _paddingRatio = 10;

		const int _paddingRatio23 = 14;

		bool _isRunning;

		AColor _backgroudColor;

		public CircularProgress(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			Indeterminate = true;
		}

		public override void Draw(Canvas canvas)
		{
			base.Draw(canvas);
			if (_isRunning != IsRunning)
				IsRunning = _isRunning;
		}

		public void SetColor(Color color)
		{
			var progress = color.IsDefault ? DefaultColor : color.ToAndroid();

			if (Forms.IsLollipopOrNewer)
			{
				IndeterminateTintList = ColorStateList.ValueOf(progress);
			}
			else
			{
				(Indeterminate ? IndeterminateDrawable : ProgressDrawable).SetColorFilter(color.ToAndroid(), FilterMode.SrcIn);
			}
		}

		public void SetBackground(Color color, Brush brush)
		{
			if (Background is GradientDrawable gradientDrawable)
			{
				GradientDrawable backgroundDrawable = gradientDrawable.GetConstantState().NewDrawable() as GradientDrawable;

				if (!Brush.IsNullOrEmpty(brush))
					backgroundDrawable.UpdateBackground(brush, Height, Width);
				else
				{
					_backgroudColor = color.IsDefault ? AColor.Transparent : color.ToAndroid();
					backgroundDrawable.SetColor(_backgroudColor);
				}

				Background = backgroundDrawable;
			}
		}

		AnimatedVectorDrawable AnimatedDrawable => IndeterminateDrawable.Current as AnimatedVectorDrawable;

		public bool IsRunning
		{
			get => AnimatedDrawable?.IsRunning ?? false;
			set
			{
				if (AnimatedDrawable == null)
					return;

				_isRunning = value;
				if (_isRunning && !AnimatedDrawable.IsRunning)
					AnimatedDrawable.Start();
				else if (AnimatedDrawable.IsRunning)
					AnimatedDrawable.Stop();

				PostInvalidate();
			}
		}

		public override void Layout(int l, int t, int r, int b)
		{
			var width = r - l;
			var height = b - t;
			var squareSize = Math.Min(Math.Max(Math.Min(width, height), MinSize), MaxSize);
			l += (width - squareSize) / 2;
			t += (height - squareSize) / 2;
			int strokeWidth;
			if (Forms.SdkInt < BuildVersionCodes.N)
				strokeWidth = squareSize / _paddingRatio23;
			else
				strokeWidth = squareSize / _paddingRatio;

			squareSize += strokeWidth;
			base.Layout(l - strokeWidth, t - strokeWidth, l + squareSize, t + squareSize);
		}
	}
}