using Android.Content;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal class PageContainer : ViewGroup
	{
		public PageContainer(Context context, IVisualElementRenderer child, bool inFragment = false) : base(context)
		{
			AddView(child.ViewGroup);
			Child = child;
			IsInFragment = inFragment;
		}

		public IVisualElementRenderer Child { get; set; }

		public bool IsInFragment { get; set; }

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			Child.UpdateLayout();
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			Child.ViewGroup.Measure(widthMeasureSpec, heightMeasureSpec);
			SetMeasuredDimension(Child.ViewGroup.MeasuredWidth, Child.ViewGroup.MeasuredHeight);
		}
	}
}