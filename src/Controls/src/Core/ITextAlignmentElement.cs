namespace Microsoft.Maui.Controls;

/// <summary>
/// Defines properties and methods for elements that support text alignment.
/// </summary>
/// <remarks>
/// This interface is implemented by UI elements that need to control the horizontal and vertical 
/// alignment of displayed text.
/// </remarks>
public interface ITextAlignmentElement
{
	/// <summary>
	/// Gets the horizontal alignment of the text.
	/// </summary>
	/// <value>A <see cref="TextAlignment"/> value that specifies how the text is horizontally aligned. The default value depends on the implementing control.</value>
	/// <remarks>Implementors should implement this property publicly.</remarks>
	TextAlignment HorizontalTextAlignment { get; }

	/// <summary>
	/// Gets the vertical alignment of the text.
	/// </summary>
	/// <value>A <see cref="TextAlignment"/> value that specifies how the text is vertically aligned. The default value depends on the implementing control.</value>
	/// <remarks>Implementors should implement this property publicly.</remarks>
	TextAlignment VerticalTextAlignment { get; }

	/// <summary>
	/// Called when the <see cref="HorizontalTextAlignment"/> property changes.
	/// </summary>
	/// <param name="oldValue">The old value of the property.</param>
	/// <param name="newValue">The new value of the property.</param>
	/// <remarks>Implementors should implement this method explicitly.</remarks>
	void OnHorizontalTextAlignmentPropertyChanged(TextAlignment oldValue, TextAlignment newValue);
}
