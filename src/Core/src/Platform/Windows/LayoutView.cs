using System;
using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class LayoutView : Panel
	{
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Action<Rectangle>? CrossPlatformArrange { get; set; }
		public Func<IReadOnlyList<IView>>? RetrieveChildren { get; internal set; }

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.MeasureOverride(availableSize);
			}

			var width = availableSize.Width;
			var height = availableSize.Height;

			var crossPlatformSize = CrossPlatformMeasure(width, height);
			return new Windows.Foundation.Size(crossPlatformSize.Width, crossPlatformSize.Height);
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			if (CrossPlatformMeasure == null || RetrieveChildren == null)
			{
				return base.ArrangeOverride(finalSize);
			}

			var width = finalSize.Width;
			var height = finalSize.Height;

			var size = CrossPlatformMeasure.Invoke(width, height);
			CrossPlatformArrange?.Invoke(new Rectangle(0, 0, width, height));

			// TODO Quick ugly way to call arrange again on all the children
			foreach(var element in RetrieveChildren())
			{
				var frame = element.Frame;
				foreach (var child in Children)
				{
					if (element.Handler == null)
						continue;

					if(element.Handler.NativeView is UIElement uIElement &&
						child == uIElement)
					{
						uIElement.Arrange(new Windows.Foundation.Rect(
							frame.X,
							frame.Y,
							frame.Width,
							frame.Height
							));
						break;
					}
				}
			}

			return new Windows.Foundation.Size(size.Width, size.Height);
		}
	}
}
