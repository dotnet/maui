using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Define how the outline is painted on elements.
	/// </summary>
	public interface IStroke
	{
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
		/// Gets a value that specifies the distance within the dash pattern where a dash begins.
		/// </summary>
		float StrokeDashOffset { get; }

		/// <summary>
		/// Specifies the limit on the ratio of the miter length to half the StrokeThickness
		/// of a shape. 
		/// </summary>
		float StrokeMiterLimit { get; }
	}
}