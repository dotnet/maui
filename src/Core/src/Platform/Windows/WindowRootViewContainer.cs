using System;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Microsoft.Maui.Platform
{
	internal class WindowRootViewContainer : Panel
	{
		protected override Size MeasureOverride(Size availableSize)
		{
			var width = availableSize.Width;
			var height = availableSize.Height;

			if (double.IsInfinity(width))
				width = XamlRoot.Size.Width;

			if (double.IsInfinity(height))
				height = XamlRoot.Size.Height;

			var size = new Size(width, height);

			// measure the children to fit the container exactly
			foreach (var child in Children)
			{
				child.Measure(size);
			}

			return size;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			foreach (var child in Children)
			{
				child.Arrange(new Rect(new Point(0, 0), finalSize));
			}

			return finalSize;
		}

		internal void AddPage(FrameworkElement pageView)
		{
			if (!Children.Contains(pageView))
				Children.Add(pageView);
		}

		internal void RemovePage(FrameworkElement pageView)
		{
			Children.Remove(pageView);
		}

		internal void AddOverlay(FrameworkElement overlayView)
		{
			if (!Children.Contains(overlayView))
				Children.Add(overlayView);
		}

		internal void RemoveOverlay(FrameworkElement overlayView)
		{
			Children.Remove(overlayView);
		}
	}
}