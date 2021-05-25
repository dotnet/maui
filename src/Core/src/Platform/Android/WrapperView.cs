using Android.Content;
using Android.Views;

namespace Microsoft.Maui
{
	public partial class WrapperView : ViewGroup
	{
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
				return;

			child.Measure(widthMeasureSpec, heightMeasureSpec);

			SetMeasuredDimension(child.MeasuredWidth, child.MeasuredHeight);
		}
	}
}