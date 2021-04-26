using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the properties to define a border.
	/// </summary>
	public interface IBorder : IView
	{
		/// <summary>
		/// Gets the border color.
		/// </summary>
		Color BorderColor { get; }

		/// <summary>
		/// Gets the corner radius of the border.
		/// </summary>
		float CornerRadius { get; }
	}
}