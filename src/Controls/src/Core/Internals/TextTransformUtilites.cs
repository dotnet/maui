using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>
	/// A utilities class for text transformations.
	/// </summary>
	/// <remarks>For internal use by the .NET MAUI platform.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class TextTransformUtilites
	{
		/// <summary>
		/// Applies the <paramref name="textTransform"/> to <paramref name="source"/>.
		/// </summary>
		/// <remarks>For internal use by the .NET MAUI platform mostly.</remarks>
		/// <param name="source">The text to transform.</param>
		/// <param name="textTransform">The transform to apply to <paramref name="source"/>.</param>
		/// <returns>The transformed text.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string GetTransformedText(string source, TextTransform textTransform)
		{
			if (string.IsNullOrEmpty(source))
				return string.Empty;

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
		/// <remarks>For internal use by the .NET MAUI platform.</remarks>
		/// <param name="inputView">The view that will receive the text value.</param>
		/// <param name="platformText">The text that will be applied to the view.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetPlainText(InputView inputView, string platformText)
		{
			if (inputView == null)
				return;

			var textTransform = inputView.TextTransform;
			var platformTextWithTransform = inputView.UpdateFormsText(platformText, textTransform);
			var formsText = inputView.UpdateFormsText(inputView.Text, textTransform);

			if ((string.IsNullOrEmpty(formsText) && platformText.Length == 0))
				return;

			if (formsText == platformTextWithTransform)
				return;

			inputView.SetValueFromRenderer(InputView.TextProperty, platformText);
		}
	}
}