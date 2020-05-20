using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace System.Maui.Platform
{
	public class LayoutViewGroup : ViewGroup
	{
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

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (CrossPlatformMeasure == null)
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			}

			var widthMode = widthMeasureSpec.GetMode();
			var heightMode = heightMeasureSpec.GetMode();

			var width = widthMeasureSpec.GetSize();
			var height = heightMeasureSpec.GetSize();

			var deviceIndependentWidth = Context.FromPixels(width);
			var deviceIndependentHeight = Context.FromPixels(height);

			var sizeRequest = CrossPlatformMeasure(deviceIndependentWidth, deviceIndependentHeight);

			var nativeWidth = Context.ToPixels(sizeRequest.Request.Width);
			var nativeHeight = Context.ToPixels(sizeRequest.Request.Height);

			SetMeasuredDimension((int)nativeWidth, (int)nativeHeight);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (CrossPlatformArrange == null)
			{
				return;
			}

			var deviceIndependentLeft = Context.FromPixels(l);
			var deviceIndependentTop = Context.FromPixels(t);
			var deviceIndependentRight = Context.FromPixels(r);
			var deviceIndependentBottom = Context.FromPixels(b);

			var destination = Rectangle.FromLTRB(deviceIndependentLeft, deviceIndependentTop,
				deviceIndependentRight, deviceIndependentBottom);

			CrossPlatformArrange(destination);
		}

		internal Func<double, double, SizeRequest> CrossPlatformMeasure { get; set; }
		internal Action<Rectangle> CrossPlatformArrange { get; set; }
	}
}
