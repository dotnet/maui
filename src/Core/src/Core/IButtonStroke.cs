using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to customize a Button border.
	/// </summary>
	public interface IButtonStroke
	{
		/// <summary>
		/// Gets a color that describes the border stroke color of the button.
		/// </summary>
		Color StrokeColor { get; }

		/// <summary>
		/// Gets or sets the width of the border.
		/// </summary>
		double StrokeThickness { get; }

		/// <summary>
		/// Gets the corner radius for the button, in device-independent units.
		/// </summary>
		int CornerRadius { get; }
	}
}