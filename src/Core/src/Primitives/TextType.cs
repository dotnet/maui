namespace Microsoft.Maui;

/// <summary>
/// Specifies the format type of text content.
/// </summary>
public enum TextType
{
	/// <summary>
	/// Plain text content without markup.
	/// </summary>
	Text,

	/// <summary>
	/// HTML-formatted text content that may include markup.
	/// </summary>
	/// <remarks>
	/// The subset of supported HTML tags varies by platform. Each platform's native text rendering engine
	/// determines which HTML tags and attributes are supported.
	/// </remarks>
	Html
}
