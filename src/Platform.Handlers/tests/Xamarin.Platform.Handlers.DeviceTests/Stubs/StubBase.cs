using System;
using Xamarin.Forms;
using Xamarin.Platform;

namespace Xamarin.Platform.Handlers.DeviceTests.Stubs
{
	public class StubBase : IFrameworkElement
	{
		public bool IsEnabled { get; set; }

		public Color BackgroundColor { get; set; }

		public Rectangle Frame { get; set; } = new Rectangle(0, 0, 20, 20);

		public IViewHandler Handler { get; set; }

		public IFrameworkElement Parent { get; set; }

		public Size DesiredSize { get; set; } = new Size(20, 20);

		public bool IsMeasureValid { get; set; }

		public bool IsArrangeValid { get; set; }

		public double Width { get; set; }

		public double Height { get; set; }

		public Thickness Margin { get; set; }

		public void Arrange(Rectangle bounds)
		{
			Frame = bounds;
			DesiredSize = bounds.Size;
		}

		public void InvalidateArrange()
		{
		}

		public void InvalidateMeasure()
		{
		}

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			return new Size(widthConstraint, heightConstraint);
		}
	}
}
