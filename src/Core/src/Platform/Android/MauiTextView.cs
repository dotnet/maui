using System;
using Android.Content;
using Android.Text;
using Android.Views;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform
{
	public class MauiTextView : PlatformAppCompatTextView
	{
		public MauiTextView(Context context) : base(context)
		{
		}

		internal event EventHandler<LayoutChangedEventArgs>? LayoutChanged;

		internal override void OnLayoutFormatted(bool changed, int l, int t, int r, int b)
		{
			LayoutChanged?.Invoke(this, new LayoutChangedEventArgs(l, t, r, b));
		}
		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (MeasureSpec.GetMode(widthMeasureSpec) == MeasureSpecMode.AtMost && Layout is not null)
			{
				int maxWidth = (int)Math.Ceiling(GetMaxLineWidth(Layout)) + CompoundPaddingLeft + CompoundPaddingRight;
				widthMeasureSpec = MeasureSpec.MakeMeasureSpec(maxWidth, MeasureSpecMode.AtMost);
			}
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
		}

		static float GetMaxLineWidth(Layout layout)
		{
			float maxWidth = 0;
			//Calculates the maximum width needed to display the content based on the widest line."
			for (int i = 0, count = layout.LineCount; i < count; i++)
			{
				maxWidth = Math.Max(maxWidth, layout.GetLineWidth(i));
			}
			return maxWidth;
		}
	}

	public class LayoutChangedEventArgs : EventArgs
	{
		public LayoutChangedEventArgs()
		{

		}

		public LayoutChangedEventArgs(int l, int t, int r, int b)
		{
			Left = l;
			Top = t;
			Right = r;
			Bottom = b;
		}

		public int Left { get; set; }
		public int Top { get; set; }
		public int Right { get; set; }
		public int Bottom { get; set; }
	}
}
