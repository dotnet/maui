#nullable disable
using Microsoft.Maui.Controls.Internals;
using TFormattedString = Tizen.UIExtensions.Common.FormattedString;
using TSpan = Tizen.UIExtensions.Common.Span;

namespace Microsoft.Maui.Controls.Platform
{
	public static class FormattedStringExtensions
	{
		public static TFormattedString ToFormattedString(this Label label)
			=> ToFormattedText(
				label.FormattedText,
				label.TextColor,
				label.RequireFontManager(),
				label.ToFont(),
				label.TextTransform,
				label.TextDecorations);

		internal static TFormattedString ToFormattedText(
			this FormattedString formattedString,
			Graphics.Color defaultColor,
			IFontManager fontManager,
			Font? defaultFont = null,
			TextTransform defaultTextTransform = TextTransform.Default,
			TextDecorations defaultTextDecorations = TextDecorations.None)
		{
			if (formattedString == null)
				return new TFormattedString();

			var defaultFontSize = defaultFont?.Size ?? fontManager.DefaultFontSize;
			var formattedText = new TFormattedString();

			for (int i = 0; i < formattedString.Spans.Count; i++)
			{
				Span span = formattedString.Spans[i];
				var transform = span.TextTransform != TextTransform.Default ? span.TextTransform : defaultTextTransform;
				var text = TextTransformUtilities.GetTransformedText(span.Text, transform);

				if (text == null)
					continue;

				var nativeSpan = new TSpan() { Text = text };
				var textColor = span.TextColor ?? defaultColor;

				if (textColor is not null)
					nativeSpan.ForegroundColor = textColor.ToPlatform();

				if (span.BackgroundColor is not null)
					nativeSpan.BackgroundColor = span.BackgroundColor.ToPlatform();

				var font = span.ToFont(defaultFontSize);
				if (font.IsDefault && defaultFont.HasValue)
					font = defaultFont.Value;

				if (!font.IsDefault)
				{
					nativeSpan.FontSize = font.Size.ToScaledPoint();
					nativeSpan.FontFamily = fontManager.GetFontFamily(span.FontFamily);
				}

				nativeSpan.LineHeight = span.LineHeight;

				var textDecorations = span.IsSet(Span.TextDecorationsProperty)
					? span.TextDecorations
					: defaultTextDecorations;
				if (textDecorations.HasFlag(TextDecorations.Strikethrough) || textDecorations.HasFlag(TextDecorations.Underline))
					nativeSpan.TextDecorations = span.TextDecorations.ToPlatform();

				formattedText.Spans.Add(nativeSpan);
			}
			return formattedText;
		}
	}
}
