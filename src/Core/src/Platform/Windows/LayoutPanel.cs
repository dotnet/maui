#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class LayoutPanel : Panel
	{
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }
		
		// Since we have to use Arrange to set the actual position/size of the native view, and Arrange can
		// be triggered by the call to CrossPlatformArrange below, we can set a flag to prevent Arrange loops
		bool _isArranging;

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.MeasureOverride(availableSize);
			}

			var width = availableSize.Width;
			var height = availableSize.Height;

			var crossPlatformSize = CrossPlatformMeasure(width, height);

			width = crossPlatformSize.Width;
			height = crossPlatformSize.Height;

			return new Windows.Foundation.Size(width, height);
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			if (_isArranging || CrossPlatformArrange == null)
			{
				return base.ArrangeOverride(finalSize);
			}

			var width = finalSize.Width;
			var height = finalSize.Height;

			_isArranging = true;
			var actual = CrossPlatformArrange(new Rectangle(0, 0, width, height));
			_isArranging = false;

			return new Windows.Foundation.Size(actual.Width, actual.Height);
		}
	}
}
