using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

namespace Microsoft.Maui.Platform
{
	public static class TextBlockExtensions
	{
		public static void UpdateFont(this TextBlock nativeControl, Font font, IFontManager fontManager)
		{
			nativeControl.FontSize = fontManager.GetFontSize(font);
			nativeControl.FontFamily = fontManager.GetFontFamily(font);
			nativeControl.FontStyle = font.ToFontStyle();
			nativeControl.FontWeight = font.ToFontWeight();
			nativeControl.IsTextScaleFactorEnabled = font.AutoScalingEnabled;
		}

		public static void UpdateFont(this TextBlock nativeControl, IText text, IFontManager fontManager) =>
			nativeControl.UpdateFont(text.Font, fontManager);

		public static void UpdateText(this TextBlock nativeControl, ILabel label)
		{
			nativeControl.UpdateTextPlainText(label);
		}

		public static void UpdateTextColor(this TextBlock nativeControl, ITextStyle text) =>
			nativeControl.UpdateProperty(TextBlock.ForegroundProperty, text.TextColor);

		public static void UpdatePadding(this TextBlock nativeControl, ILabel label) =>
			nativeControl.UpdateProperty(TextBlock.PaddingProperty, label.Padding.ToPlatform());

		public static void UpdateCharacterSpacing(this TextBlock nativeControl, ITextStyle label)
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
				nativeControl.TextDecorations &= ~global::Windows.UI.Text.TextDecorations.Underline;
			else
				nativeControl.TextDecorations |= global::Windows.UI.Text.TextDecorations.Underline;

			if ((elementTextDecorations & TextDecorations.Strikethrough) == 0)
				nativeControl.TextDecorations &= ~global::Windows.UI.Text.TextDecorations.Strikethrough;
			else
				nativeControl.TextDecorations |= global::Windows.UI.Text.TextDecorations.Strikethrough;

			// TextDecorations are not updated in the UI until the text changes
			if (nativeControl.Inlines != null && nativeControl.Inlines.Count > 0)
			{
				foreach (var inline in nativeControl.Inlines)
				{
					if (inline is Run run)
					{
						run.Text = run.Text;
					}
					else if (inline is Span span)
					{
						foreach (var inline2 in span.Inlines)
						{
							if (inline2 is Run run2)
							{
								run2.Text = run2.Text;
							}
						}
					}
				}
			}
			else
			{
				nativeControl.Text = nativeControl.Text;
			}
		}

		public static void UpdateLineHeight(this TextBlock nativeControl, ILabel label)
		{
			if (label.LineHeight >= 0)
			{
				nativeControl.LineHeight = label.LineHeight * nativeControl.FontSize;
			}
		}

		public static void UpdateHorizontalTextAlignment(this TextBlock nativeControl, ILabel label)
		{
			// We don't have a FlowDirection yet, so there's nothing to pass in here. 
			// TODO: Update this when FlowDirection is available 
			nativeControl.TextAlignment = label.HorizontalTextAlignment.ToPlatform(true);
		}

		public static void UpdateVerticalTextAlignment(this TextBlock nativeControl, ILabel label)
		{
			nativeControl.VerticalAlignment = label.VerticalTextAlignment.ToPlatformVerticalAlignment();
		}

		internal static void UpdateTextHtml(this TextBlock nativeControl, ILabel label)
		{
			var text = label.Text ?? string.Empty;

			// Just in case we are not given text with elements.
			var modifiedText = string.Format("<div>{0}</div>", text);
			modifiedText = Regex.Replace(modifiedText, "<br>", "<br></br>", RegexOptions.IgnoreCase);

			// Reset the text because we will add to it.
			nativeControl.Inlines.Clear();

			try
			{
				var element = XElement.Parse(modifiedText);
				LabelHtmlHelper.ParseText(element, nativeControl.Inlines, label);
			}
			catch (Exception)
			{
				// If anything goes wrong just show the html
				nativeControl.Text = global::Windows.Data.Html.HtmlUtilities.ConvertToText(label.Text);
			}
		}

		public static void UpdateLineBreakMode(this TextBlock nativeControl, ILabel label)
		{
			var lineBreakMode = label.LineBreakMode;

			switch (lineBreakMode)
			{
				case LineBreakMode.NoWrap:
					nativeControl.TextTrimming = TextTrimming.Clip;
					nativeControl.TextWrapping = TextWrapping.NoWrap;
					break;
				case LineBreakMode.WordWrap:
					nativeControl.TextTrimming = TextTrimming.None;
					nativeControl.TextWrapping = TextWrapping.Wrap;
					break;
				case LineBreakMode.CharacterWrap:
					nativeControl.TextTrimming = TextTrimming.WordEllipsis;
					nativeControl.TextWrapping = TextWrapping.Wrap;
					break;
				case LineBreakMode.HeadTruncation:
					// TODO: This truncates at the end.
					nativeControl.TextTrimming = TextTrimming.WordEllipsis;
					nativeControl.DetermineTruncatedTextWrapping();
					break;
				case LineBreakMode.TailTruncation:
					nativeControl.TextTrimming = TextTrimming.CharacterEllipsis;
					nativeControl.DetermineTruncatedTextWrapping();
					break;
				case LineBreakMode.MiddleTruncation:
					// TODO: This truncates at the end.
					nativeControl.TextTrimming = TextTrimming.WordEllipsis;
					nativeControl.DetermineTruncatedTextWrapping();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal static void DetermineTruncatedTextWrapping(this TextBlock textBlock) =>
			textBlock.TextWrapping = textBlock.MaxLines > 1 ? TextWrapping.Wrap : TextWrapping.NoWrap;

		internal static void UpdateTextPlainText(this TextBlock nativeControl, IText label)
		{
			nativeControl.Text = label.Text;
		}
	}
}