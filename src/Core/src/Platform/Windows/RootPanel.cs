#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	[Obsolete("Use Microsoft.Maui.Platform.ContentPanel")]
	public class RootPanel : Panel
	{
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rect, Size>? CrossPlatformArrange { get; set; }

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
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

			var size = new global::Windows.Foundation.Size(width, height);

			// Measure the children (should only be one, the Page)
			foreach (var child in Children)
			{
				child.Measure(size);
			}

			return size;
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			foreach (var child in Children)
			{
				child.Arrange(new global::Windows.Foundation.Rect(new global::Windows.Foundation.Point(0, 0), finalSize));
			}

			return finalSize;
		}
	}
}