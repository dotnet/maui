using Android.Content;
using Android.Graphics;
using Android.Views;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;
using APath = Android.Graphics.Path;

namespace Microsoft.Maui
{
	public partial class WrapperView : ViewGroup
	{
		APath? _currentPath;
		SizeF _lastPathSize;

		public WrapperView(Context context)
			: base(context)
		{
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (ChildCount == 0 || GetChildAt(0) is not View child)
				return;

			var widthMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(right - left);
			var heightMeasureSpec = MeasureSpecMode.Exactly.MakeMeasureSpec(bottom - top);

			child.Measure(widthMeasureSpec, heightMeasureSpec);
			child.Layout(0, 0, child.MeasuredWidth, child.MeasuredHeight);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (ChildCount == 0 || GetChildAt(0) is not View child)
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
				return;
			}

			child.Measure(widthMeasureSpec, heightMeasureSpec);

			SetMeasuredDimension(child.MeasuredWidth, child.MeasuredHeight);
		}

		protected override void DispatchDraw(Canvas? canvas)
		{
			if (canvas != null && Clip != null)
			{
				var bounds = new RectangleF(0, 0, canvas.Width, canvas.Height);

				if (_lastPathSize != bounds.Size || _currentPath == null)
				{
					var path = Clip.PathForBounds(bounds);
					_currentPath = path?.AsAndroidPath();
					_lastPathSize = bounds.Size;
				}

				if (_currentPath != null)
					canvas.ClipPath(_currentPath);
			}

			base.DispatchDraw(canvas);
		}
	}
}