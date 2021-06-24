#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class RootPanel : Panel
	{
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rectangle, Size>? CrossPlatformArrange { get; set; }

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			var width = availableSize.Width;
			var height = availableSize.Height;

			if (double.IsInfinity(width))
			{
				width = XamlRoot.Size.Width;
			}

			if (double.IsInfinity(height))
			{
				height = XamlRoot.Size.Height;
			}

			return new Windows.Foundation.Size(width, height);
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			foreach (var child in Children)
			{
				child.Arrange(new Windows.Foundation.Rect(new Windows.Foundation.Point(0,0), finalSize));
			}

			return finalSize;
		}
	}
}
