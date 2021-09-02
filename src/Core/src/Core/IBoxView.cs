using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// BoxView renders a simple rectangle of a specified width, height, and color. 
	/// </summary>
	public interface IBoxView : IView
	{
		/// <summary>
		/// Gets the color which will fill the rectangle.
		/// </summary>
		Color Color { get; }

		/// <summary>
		/// Gets the corner radius for the box view.
		/// </summary>
		CornerRadius CornerRadius { get; }
	}
}