namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a View that enables you to draw a shape to the screen.
	/// </summary>
	public interface IShapeView : IView
	{
		/// <summary>
		/// Gets the Shape definition to render.
		/// </summary>
		IShape? Shape { get; }

		/// <summary>
		/// Determines how a Shape's contents are stretched to fill the view's layout space.
		/// </summary>
		PathAspect Aspect { get; }

		/// <summary>
		/// Indicates the brush used to paint the shape's interior.
		/// </summary>
		Paint? Fill { get; }

		/// <summary>
		/// Indicates the color used to paint the shape's outline.
		/// </summary>
		Paint? Stroke { get; }

		/// <summary>
		/// Represents a collection of double values that indicate the pattern of dashes and gaps
		/// that are used to outline a shape.
		/// </summary>
		double StrokeThickness { get; }

		/// <summary>
		/// Describes the shape at the start and end of a line or segment.
		/// </summary>
		LineCap StrokeLineCap { get; }

		/// <summary>
		/// Specifies the type of join that is used at the vertices of a shape.
		/// </summary>
		LineJoin StrokeLineJoin { get; }

		/// <summary>
		/// Specifies the distance within the dash pattern where a dash begins.
		/// </summary>
		float[]? StrokeDashPattern { get; }

		/// <summary>
		/// Specifies the limit on the ratio of the miter length to half the StrokeThickness
		/// of a shape. 
		/// </summary>
		float StrokeMiterLimit { get; }
	}
}