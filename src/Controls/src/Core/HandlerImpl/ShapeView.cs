#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes2
{
	public class ShapeView : View, IShapeView
	{
		public ShapeView()
		{

		}

		public ShapeView(IShape shape)
		{
			Shape = shape;
		}

		public IShape? Shape { get; set; }

		public Paint? Fill { get; set; }

		public Color? Stroke { get; set; }

		public double StrokeThickness { get; set; }

		public float[]? StrokeDashPattern { get; set; }

		public LineCap StrokeLineCap { get; set; }

		public LineJoin StrokeLineJoin { get; set; }

		public float StrokeMiterLimit { get; set; }

		public Stretch Stretch { get; set; }
	}
}