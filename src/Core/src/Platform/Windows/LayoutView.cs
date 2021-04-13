using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public class LayoutView : Panel
	{
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Action<Rectangle>? CrossPlatformArrange { get; set; }
		internal Action? CrossPlatformArrangeChildren { get; set; }
		public Action? CrossPlatformInvalidateChildrenMeasure { get; internal set; }

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.MeasureOverride(availableSize);
			}

			var width = availableSize.Width;
			var height = availableSize.Height;

			// AFAICT you have to call measure on every child every single time
			// WinUI calls MeasureOverride
			CrossPlatformInvalidateChildrenMeasure?.Invoke();
			var crossPlatformSize = CrossPlatformMeasure(width, height);

			width = crossPlatformSize.Width;
			height = crossPlatformSize.Height;


			return new Windows.Foundation.Size(width, height);
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.ArrangeOverride(finalSize);
			}

			var width = finalSize.Width;
			var height = finalSize.Height;

			var size = CrossPlatformMeasure.Invoke(width, height);
			CrossPlatformArrange?.Invoke(new Rectangle(0, 0, width, height));


			// AFAICT you have to call arrange on every child every single time
			// WinUI calls ArrangeOverride
			CrossPlatformArrangeChildren?.Invoke();

			return new Windows.Foundation.Size(size.Width, size.Height);
		}
	}
}
