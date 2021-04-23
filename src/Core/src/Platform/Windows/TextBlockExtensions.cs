using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace Microsoft.Maui
{
	public static class TextBlockExtensions
	{
		public static void UpdateFont(this TextBlock nativeControl, Font font, IFontManager fontManager)
		{
			nativeControl.FontSize = fontManager.GetFontSize(font);
			nativeControl.FontFamily = fontManager.GetFontFamily(font);
			nativeControl.FontStyle = font.FontAttributes.ToFontStyle();
			nativeControl.FontWeight = font.FontAttributes.ToFontWeight();
		}

		public static void UpdateFont(this TextBlock nativeControl, IText text, IFontManager fontManager) =>
			nativeControl.UpdateFont(text.Font, fontManager);

		public static void UpdateText(this TextBlock nativeControl, IText text) =>
			nativeControl.Text = text.Text;

		public static void UpdateTextColor(this TextBlock nativeControl, IText text) =>
			nativeControl.UpdateProperty(TextBlock.ForegroundProperty, text.TextColor);

		public static void UpdatePadding(this TextBlock nativeControl, ILabel label) =>
			nativeControl.UpdateProperty(TextBlock.PaddingProperty, label.Padding.ToNative());

		public static void UpdateLineHeight(this TextBlock nativeControl, ILabel label)
		{
			if (label.LineHeight >= 0)
			{
				nativeControl.LineHeight = label.LineHeight * nativeControl.FontSize;
			}
    }
    
		public static void UpdateCharacterSpacing(this TextBlock nativeControl, ILabel label)
		{
			nativeControl.CharacterSpacing = label.CharacterSpacing.ToEm();
		}

		public static void UpdateMaxLines(this TextBlock nativeControl, ILabel label)
		{
			if (label.MaxLines >= 0)
			{
				nativeControl.MaxLines = label.MaxLines;
			}
			else
			{
				nativeControl.MaxLines = 0;
			}
		}

		public static void UpdateTextDecorations(this TextBlock nativeControl, ILabel label)
		{
			var elementTextDecorations = label.TextDecorations;

			if ((elementTextDecorations & TextDecorations.Underline) == 0)
				nativeControl.TextDecorations &= ~Windows.UI.Text.TextDecorations.Underline;
			else
				nativeControl.TextDecorations |= Windows.UI.Text.TextDecorations.Underline;

			if ((elementTextDecorations & TextDecorations.Strikethrough) == 0)
				nativeControl.TextDecorations &= ~Windows.UI.Text.TextDecorations.Strikethrough;
			else
				nativeControl.TextDecorations |= Windows.UI.Text.TextDecorations.Strikethrough;

			// TextDecorations are not updated in the UI until the text changes
			if (nativeControl.Inlines != null && nativeControl.Inlines.Count > 0)
			{
				for (var i = 0; i < nativeControl.Inlines.Count; i++)
				{
					var run = (Run)nativeControl.Inlines[i];
					run.Text = run.Text;
				}
			}
			else
			{
				nativeControl.Text = nativeControl.Text;
			}
		}
	}
}