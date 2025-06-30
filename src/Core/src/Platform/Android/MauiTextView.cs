using System;
using Android.Content;
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
