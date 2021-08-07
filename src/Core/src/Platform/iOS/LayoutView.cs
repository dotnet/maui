using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class LayoutView : MauiView
	{
		public override CGSize SizeThatFits(CGSize size)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.SizeThatFits(size);
			}

			var width = size.Width;
			var height = size.Height;

			var crossPlatformSize = CrossPlatformMeasure(width, height);

			return crossPlatformSize.ToCGSize();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			// TODO ezhart 2021-07-07 This Frame may not make sense if we're applying a transform to this UIView; we should determine the rectangle from Bounds/Center instead
			Frame = AdjustForSafeArea(Frame);

			var bounds = Frame.ToRectangle();

			CrossPlatformMeasure?.Invoke(bounds.Width, bounds.Height);
			CrossPlatformArrange?.Invoke(bounds.Size);
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Size, Size>? CrossPlatformArrange { get; set; }
	}
}