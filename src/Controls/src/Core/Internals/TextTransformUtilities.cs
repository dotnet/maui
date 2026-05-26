#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals;

[Obsolete("Class name contains a typo. Please use TextTransformUtilities instead, this will be removed in a future version.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TextTransformUtilites
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static string GetTransformedText(string source, TextTransform textTransform)
		=> TextTransformUtilities.GetTransformedText(source, textTransform);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void SetPlainText(InputView inputView, string platformText)
		=> TextTransformUtilities.SetPlainText(inputView, platformText);
}

/// <summary>
/// A utilities class for text transformations.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TextTransformUtilities
{
	/// <summary>
	/// Applies the <paramref name="textTransform"/> to <paramref name="source"/>.
	/// </summary>
	/// <param name="source">The text to transform.</param>
	/// <param name="textTransform">The transform to apply to <paramref name="source"/>.</param>
	/// <returns>The transformed text.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static string GetTransformedText(string source, TextTransform textTransform)
	{
		if (string.IsNullOrEmpty(source))
		{
			return string.Empty;
		}

		switch (textTransform)
		{
			case TextTransform.None:
			default:
				return source;
			case TextTransform.Lowercase:
				return source.ToLowerInvariant();
			case TextTransform.Uppercase:
				return source.ToUpperInvariant();
		}
	}

	/// <summary>
	/// Sets the plain text value to the specified input view.
	/// </summary>
	/// <param name="inputView">The view that will receive the text value.</param>
	/// <param name="platformText">The text that will be applied to the view.</param>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void SetPlainText(InputView inputView, string platformText)
	{
		if (inputView is null)
		{
			return;
		}

		var textTransform = inputView.TextTransform;
		var platformTextWithTransform = inputView.UpdateFormsText(platformText, textTransform);
		var formsText = inputView.UpdateFormsText(inputView.Text, textTransform);

		if ((string.IsNullOrEmpty(formsText) && platformText.Length == 0))
		{
			return;
		}

		if (formsText == platformTextWithTransform)
		{
			return;
		}

		inputView.SetValueFromRenderer(InputView.TextProperty, platformText);
	}
}