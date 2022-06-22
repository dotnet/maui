using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public class ContentView : MauiView
	{
		internal event EventHandler? LayoutSubviewsChanged;

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

			var bounds = AdjustForSafeArea(Bounds).ToRectangle();

			CrossPlatformMeasure?.Invoke(bounds.Width, bounds.Height);
			CrossPlatformArrange?.Invoke(bounds);

			LayoutSubviewsChanged?.Invoke(this, EventArgs.Empty);
		}

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();
			Superview?.SetNeedsLayout();
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rect, Size>? CrossPlatformArrange { get; set; }
	}
}