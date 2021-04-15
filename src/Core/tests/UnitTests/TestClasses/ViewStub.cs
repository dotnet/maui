using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.UnitTests
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

		public string AutomationId { get; }

		public FlowDirection FlowDirection { get; set; }

		public LayoutAlignment HorizontalLayoutAlignment { get; set; }

		public LayoutAlignment VerticalLayoutAlignment { get; set; }

		public Semantics Semantics { get; set; } = new Semantics();

		public void Arrange(Rectangle bounds) { }

		public void InvalidateArrange() { }

		public void InvalidateMeasure() { }

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			return Size.Zero;
		}
	}
}