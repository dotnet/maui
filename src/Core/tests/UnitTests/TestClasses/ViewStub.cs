using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Tests
{
	class ViewStub : IViewStub
	{
		public bool IsEnabled { get; set; }

		public Color BackgroundColor { get; set; }

		public Rectangle Frame { get; set; }

		public IViewHandler Handler { get; set; }

		public IFrameworkElement Parent { get; set; }

		public Size DesiredSize { get; set; }

		public bool IsMeasureValid { get; set; }

		public bool IsArrangeValid { get; set; }

		public double Width { get; set; }

		public double Height { get; set; }

		public Thickness Margin { get; set; }

		public string AutomationId { get; set; }

		public FlowDirection FlowDirection { get; set; }

		public IGeometry Clip { get; set; }

		public void Arrange(Rectangle bounds) { }

		public void InvalidateArrange() { }

		public void InvalidateMeasure() { }

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			return Size.Zero;
		}
	}
}