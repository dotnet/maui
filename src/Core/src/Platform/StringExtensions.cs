using System;
using System.ComponentModel;

namespace Microsoft.Maui
{
	public static class StringExtensions
	{
		public static string? TrimToMaxLength(this string? currentText, int maxLength) =>
			maxLength >= 0 && currentText?.Length > maxLength
				? currentText.Substring(0, maxLength)
				: currentText;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static string GetTransformedText(this string? text, TextTransform textTransform)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			// TODO: WinUI throws layout cycle exception if the text is big enough to go out of the boundries in sample app.
			// This must be removed after the layout measurement calls is corrected for WinUI.
			if (text.Length > 5)
				text = text.Substring(0, 5);

			switch (textTransform)
			{
				case TextTransform.None:
				case TextTransform.Default:
				default:
					return text;
				case TextTransform.Lowercase:
					return text.ToLowerInvariant();
				case TextTransform.Uppercase:
					return text.ToUpperInvariant();
			}
		}
	}
}