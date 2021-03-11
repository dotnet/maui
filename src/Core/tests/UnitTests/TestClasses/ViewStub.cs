using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Tests
{
	class ViewStub : IViewStub
	{
		public Thickness Margin { get; set; }

		public bool IsEnabled { get; set; }

		public IBrush Background { get; set; }

		public Rectangle Frame { get; set; }

		public double Width { get; set; }

		public double Height { get; set; }

		public IViewHandler Handler { get; set; }

		public IFrameworkElement Parent { get; }

		public Size DesiredSize { get; }

		public bool IsMeasureValid { get; }

		public bool IsArrangeValid { get; }

		public void Arrange(Rectangle bounds) { }

		public void InvalidateArrange() { }

		public void InvalidateMeasure() { }

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			return Size.Zero;
		}
	}
}