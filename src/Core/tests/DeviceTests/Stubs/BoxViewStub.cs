using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class BoxViewStub : StubBase, IShapeView
	{
		public Color Color { get; set; }

		public CornerRadius CornerRadius { get; set; }

		public IShape Shape { get; set; }

		public PathAspect Aspect { get; set; }

		public Paint Fill { get; set; }

		public Paint Stroke { get; set; }

		public double StrokeThickness { get; set; }

		public LineCap StrokeLineCap { get; set; }

		public LineJoin StrokeLineJoin { get; set; }

		public float[] StrokeDashPattern { get; set; }

		public float StrokeMiterLimit { get; set; }

		public float StrokeDashOffset { get; set; }
	}
}
