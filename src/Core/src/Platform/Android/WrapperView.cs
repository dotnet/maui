using Android.Content;
using Android.Views;
using Android.Widget;

namespace Microsoft.Maui
{
	public partial class WrapperView : FrameLayout
	{
		public WrapperView(Context context)
			: base(context)
		{
		}

		public override void OnViewAdded(View? child)
		{
			base.OnViewAdded(child);

			if (child != null)
			{
				child.LayoutParameters = new LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent);
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (ChildCount == 0 || GetChildAt(0) is not View child)
				return;

			child.Measure(widthMeasureSpec, heightMeasureSpec);

			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			SetMeasuredDimension(child.MeasuredWidth, child.MeasuredHeight);
		}
	}
}