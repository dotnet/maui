using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class BorderStub : StubBase, IBorderView
	{
		public object Content { get; set; }

		public IView PresentedContent { get; set; }

		public Thickness Padding { get; set; }

		public IShape Shape { get; set; }

		public Paint Stroke { get; set; }

		public double StrokeThickness { get; set; }

		public LineCap StrokeLineCap { get; set; }

		public LineJoin StrokeLineJoin { get; set; }

		public float[] StrokeDashPattern { get; set; }

		public float StrokeDashOffset { get; set; }

		public float StrokeMiterLimit { get; set; }

#if !WINDOWS
		public Size CrossPlatformArrange(Rect bounds)
		{
			return Size.Zero;
		}

		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return Size.Zero;
		}
#else
		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return (PresentedContent ?? (Content as IView))?.Measure(widthConstraint, heightConstraint) ?? Size.Zero;
		}

		public Size CrossPlatformArrange(Rect bounds)
		{
			return (PresentedContent ?? (Content as IView))?.Arrange(bounds) ?? Size.Zero;
		}
#endif
	}
}