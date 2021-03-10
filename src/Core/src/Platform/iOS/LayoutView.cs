using System;
using CoreGraphics;
using Microsoft.Maui;
using UIKit;

namespace Microsoft.Maui
{
	public class LayoutView : UIView
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

			return base.SizeThatFits(crossPlatformSize.ToCGSize());
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var width = Frame.Width;
			var height = Frame.Height;

			CrossPlatformMeasure?.Invoke(width, height);
			CrossPlatformArrange?.Invoke(Frame.ToRectangle());
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Action<Rectangle>? CrossPlatformArrange { get; set; }
	}
}
