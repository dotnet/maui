namespace Microsoft.Maui.Controls;

/// <summary>
/// Defines properties for elements that can have rounded corners.
/// </summary>
/// <remarks>
/// This interface is implemented by UI elements that support corner radius customization,
/// allowing for consistent styling of corners across different controls.
/// </remarks>
public interface ICornerElement
{
	/// <summary>
	/// Gets the radius for the corners of the element.
	/// </summary>
	/// <value>A <see cref="CornerRadius"/> value that specifies the radius for each corner of the element. The default value depends on the implementing control.</value>
	/// <remarks>
	/// Implementors should implement this property publicly.
	/// When specifying corner radii, the order of values is top left, top right, bottom left, and bottom right.
	/// </remarks>
	CornerRadius CornerRadius { get; }
}
