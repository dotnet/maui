using System;
using Microsoft.Maui;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class LayoutView : Panel
	{
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Action<Rectangle>? CrossPlatformArrange { get; set; }


		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.MeasureOverride(availableSize);
			}

			var width = availableSize.Width;
			var height = availableSize.Height;

			var crossPlatformSize = CrossPlatformMeasure(width, height);
			var returnValue = base.MeasureOverride(new Windows.Foundation.Size(crossPlatformSize.Width, crossPlatformSize.Height));
			return returnValue;
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			var width = finalSize.Width;
			var height = finalSize.Height;

			CrossPlatformMeasure?.Invoke(width, height);
			CrossPlatformArrange?.Invoke(new Rectangle(0, 0, width, height));
			return base.ArrangeOverride(finalSize);
		}
	}
}
