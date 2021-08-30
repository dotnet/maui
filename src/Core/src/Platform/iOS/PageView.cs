using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class PageView : MauiView
	{
		public override CGSize SizeThatFits(CGSize size)
		{
			return size;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var bounds = AdjustForSafeArea(Bounds).ToRectangle();

			CrossPlatformMeasure?.Invoke(bounds.Width, bounds.Height);
			CrossPlatformArrange?.Invoke(bounds);
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }
	}
}