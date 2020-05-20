using CoreGraphics;
using UIKit;

namespace System.Maui.Platform
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

			var sizeRequest = CrossPlatformMeasure(width, height);

			return base.SizeThatFits(sizeRequest.Request.ToCGSize());
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var width = Frame.Width;
			var height = Frame.Height;
			CrossPlatformMeasure(width, height);

			CrossPlatformArrange?.Invoke(Frame.ToRectangle());
		}

		internal Func<double, double, SizeRequest> CrossPlatformMeasure { get; set; }
		internal Action<Rectangle> CrossPlatformArrange { get; set; }
	}
}
