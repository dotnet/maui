using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to set the view borders.
	/// </summary>
	public interface IBorder
	{
		/// <summary>
		/// Gets the the brush of a view's border.
		/// </summary>
		Paint? BorderBrush { get; }

		/// <summary>
		/// Gets the the width of a view's border.
		/// </summary>
		double BorderWidth { get; }

		/// <summary>
		/// Gets a collection of Double values that indicate the pattern of dashes and gaps 
		/// that is used to outline shapes.
		/// </summary>
		DoubleCollection BorderDashArray { get; }

		/// <summary>
		/// Gets a Double that specifies the distance within the dash pattern where a 
		/// dash begins.
		/// </summary>
		double BorderDashOffset { get; }

		/// <summary>
		/// Gets the shape that define a view's border.
		/// </summary>
		IShape BorderShape { get; }
	}
}