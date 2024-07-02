using System;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Microsoft.Maui.Platform
{
	internal class WindowRootViewContainer : Panel
	{
		FrameworkElement? _topPage;
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
			{
				int indexOFTopPage = 0;
				if (_topPage != null)
					indexOFTopPage = Children.IndexOf(_topPage) + 1;

				Children.Insert(indexOFTopPage, pageView);
				_topPage = pageView;
			}
		}

		internal void RemovePage(FrameworkElement pageView)
		{
			int indexOFTopPage = -1;
			if (_topPage != null)
				indexOFTopPage = Children.IndexOf(_topPage) - 1;

			Children.Remove(pageView);

			if (indexOFTopPage >= 0)
				_topPage = (FrameworkElement)Children[indexOFTopPage];
			else
				_topPage = null;
		}

		internal void AddOverlay(FrameworkElement overlayView)
		{
			if (!Children.Contains(overlayView))
				Children.Add(overlayView);
		}

		internal static void RemoveOverlay(FrameworkElement overlayView)
		{
			Children.Remove(overlayView);
		}
	}
}