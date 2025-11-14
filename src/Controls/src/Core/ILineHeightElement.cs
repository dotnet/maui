namespace Microsoft.Maui.Controls;

/// <summary>
/// Defines properties and methods for elements that support line height customization.
/// </summary>
/// <remarks>
/// This interface is implemented by UI elements that need to control the spacing between lines of text,
/// providing consistent line height behavior across different text-based controls.
/// </remarks>
public interface ILineHeightElement
{
	/// <summary>
	/// Gets the line height for text displayed by this element.
	/// </summary>
	/// <value>A multiplier that determines the spacing between lines of text. A value of 1.0 represents standard line height.</value>
	double LineHeight { get; }

	/// <summary>
	/// Called when the <see cref="LineHeight"/> property changes.
	/// </summary>
	/// <param name="oldValue">The old value of the property.</param>
	/// <param name="newValue">The new value of the property.</param>
	/// <remarks>Implementors should implement this method explicitly.</remarks>
	void OnLineHeightChanged(double oldValue, double newValue);
}
