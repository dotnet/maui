using System;
using Android.Content;
using Android.Text;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public class MauiTextView : PlatformAppCompatTextView
	{
		private Layout? _cachedLayout;
		private string? _lastText;
		private int _lastAvailableWidth;
		private int _lastTotalPadding;

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
			if (MeasureSpec.GetMode(widthMeasureSpec) == MeasureSpecMode.AtMost && !string.IsNullOrEmpty(Text))
			{
				int availableWidth = MeasureSpec.GetSize(widthMeasureSpec);
				int totalPadding = CompoundPaddingLeft + CompoundPaddingRight;

				if (availableWidth > totalPadding)
				{
					// Only create new layout if text, width or padding changed
					if (_lastText != Text || _lastAvailableWidth != availableWidth || _lastTotalPadding != totalPadding || _cachedLayout is null)
					{
						_cachedLayout = TextLayoutUtils.CreateLayout(Text, Paint, availableWidth - totalPadding, Android.Text.Layout.Alignment.AlignNormal);
						_lastText = Text;
						_lastAvailableWidth = availableWidth;
						_lastTotalPadding = totalPadding;
					}
					// since the original issue 27614 occurs when the text is multiline, we only apply custom width measurement for multiline text
					if (_cachedLayout.LineCount > 1)
					{
						int contentWidth = (int)Math.Ceiling(GetMaxLineWidth(_cachedLayout));
						int requiredWidth = contentWidth + totalPadding;
						int desiredWidth = Math.Min(requiredWidth, availableWidth);
						widthMeasureSpec = MeasureSpec.MakeMeasureSpec(desiredWidth, MeasureSpecMode.AtMost);
					}

				}
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
