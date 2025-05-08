#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls;

/// <summary>
/// Defines properties and methods for elements that display text.
/// </summary>
/// <remarks>
/// This interface is implemented by UI elements that can display text and allows consistent 
/// text styling and formatting across different controls.
/// </remarks>
public interface ITextElement
{
	/// <summary>
	/// Gets the color of the text.
	/// </summary>
	/// <value>The <see cref="Color"/> of the text. The default value depends on the implementing control.</value>
	/// <remarks>Implementors should implement this property publicly.</remarks>
	Color TextColor { get; }

	/// <summary>
	/// Called when the <see cref="TextColor"/> property changes.
	/// </summary>
	/// <param name="oldValue">The old value of the property.</param>
	/// <param name="newValue">The new value of the property.</param>
	/// <remarks>Implementors should implement this method explicitly.</remarks>
	void OnTextColorPropertyChanged(Color oldValue, Color newValue);

	/// <summary>
	/// Gets the character spacing.
	/// </summary>
	/// <value>The spacing between characters in the text, in device-independent units. The default is 0.</value>
	double CharacterSpacing { get; }

	/// <summary>
	/// Called when the <see cref="CharacterSpacing"/> property changes.
	/// </summary>
	/// <param name="oldValue">The old value of the property.</param>
	/// <param name="newValue">The new value of the property.</param>
	/// <remarks>Implementors should implement this method explicitly.</remarks>
	void OnCharacterSpacingPropertyChanged(double oldValue, double newValue);

	/// <summary>
	/// Gets or sets the text transformation.
	/// </summary>
	/// <value>A <see cref="TextTransform"/> value that indicates how the text is transformed. The default is <see cref="TextTransform.None"/>.</value>
	TextTransform TextTransform { get; set; }

	/// <summary>
	/// Called when the <see cref="TextTransform"/> property changes.
	/// </summary>
	/// <param name="oldValue">The old value of the property.</param>
	/// <param name="newValue">The new value of the property.</param>
	void OnTextTransformChanged(TextTransform oldValue, TextTransform newValue);

	/// <summary>
	/// Updates the text according to the specified transformation.
	/// </summary>
	/// <param name="original">The original text to transform.</param>
	/// <param name="transform">The transformation to apply.</param>
	/// <returns>The transformed text.</returns>
	string UpdateFormsText(string original, TextTransform transform);
}
