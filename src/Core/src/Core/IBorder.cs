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
	}
}