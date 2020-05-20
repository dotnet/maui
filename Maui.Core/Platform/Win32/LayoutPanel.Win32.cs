using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace System.Maui.Platform
{
	public class LayoutPanel : Canvas
	{
		internal Func<double, double, SizeRequest> CrossPlatformMeasure { get; set; }
		internal Action<Rectangle> CrossPlatformArrange { get; set; }

		protected override Windows.Size MeasureOverride(Windows.Size constraint)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.MeasureOverride(constraint);
			}
			
			var sizeRequest = CrossPlatformMeasure(constraint.Width, constraint.Height);

			return new Windows.Size(sizeRequest.Request.Width, sizeRequest.Request.Height);
		}

		protected override Windows.Size ArrangeOverride(Windows.Size arrangeSize)
		{
			if (CrossPlatformArrange == null)
			{
				return base.ArrangeOverride(arrangeSize);
			}

			CrossPlatformArrange(new Rectangle(0, 0, arrangeSize.Width, arrangeSize.Height));

			return arrangeSize;
		}
	}
}
