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
		public static void UpdateFont(this TextBlock platformControl, Font font, IFontManager fontManager)
		{
			platformControl.FontSize = fontManager.GetFontSize(font);
			platformControl.FontFamily = fontManager.GetFontFamily(font);
			platformControl.FontStyle = font.ToFontStyle();
			platformControl.FontWeight = font.ToFontWeight();
			platformControl.IsTextScaleFactorEnabled = font.AutoScalingEnabled;
		}

		public static void UpdateFont(this TextBlock platformControl, IText text, IFontManager fontManager) =>
			platformControl.UpdateFont(text.Font, fontManager);

		public static void UpdateText(this TextBlock platformControl, ILabel label)
		{
			platformControl.UpdateTextPlainText(label);
		}

		public static void UpdateTextColor(this TextBlock platformControl, ITextStyle text) =>
			platformControl.UpdateProperty(TextBlock.ForegroundProperty, text.TextColor);

		public static void UpdatePadding(this TextBlock platformControl, ILabel label) =>
			platformControl.UpdateProperty(TextBlock.PaddingProperty, label.Padding.ToPlatform());

		public static void UpdateCharacterSpacing(this TextBlock platformControl, ITextStyle label)
		{
			platformControl.CharacterSpacing = label.CharacterSpacing.ToEm();
		}

		public static void UpdateMaxLines(this TextBlock platformControl, ILabel label)
		{
			if (label.MaxLines >= 0)
			{
				platformControl.MaxLines = label.MaxLines;
			}
			else
			{
				platformControl.MaxLines = 0;
			}
		}

		public static void UpdateTextDecorations(this TextBlock platformControl, ILabel label)
		{
			var elementTextDecorations = label.TextDecorations;

			if ((elementTextDecorations & TextDecorations.Underline) == 0)
				platformControl.TextDecorations &= ~global::Windows.UI.Text.TextDecorations.Underline;
			else
				platformControl.TextDecorations |= global::Windows.UI.Text.TextDecorations.Underline;

			if ((elementTextDecorations & TextDecorations.Strikethrough) == 0)
				platformControl.TextDecorations &= ~global::Windows.UI.Text.TextDecorations.Strikethrough;
			else
				platformControl.TextDecorations |= global::Windows.UI.Text.TextDecorations.Strikethrough;

			// TextDecorations are not updated in the UI until the text changes
			if (platformControl.Inlines != null && platformControl.Inlines.Count > 0)
			{
				foreach (var inline in platformControl.Inlines)
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
				platformControl.Text = platformControl.Text;
			}
		}

		public static void UpdateLineHeight(this TextBlock platformControl, ILabel label)
		{
			if (label.LineHeight >= 0)
			{
				platformControl.LineHeight = label.LineHeight * platformControl.FontSize;
			}
		}

		public static void UpdateHorizontalTextAlignment(this TextBlock platformControl, ILabel label)
		{
			// We don't have a FlowDirection yet, so there's nothing to pass in here. 
			// TODO: Update this when FlowDirection is available 
			platformControl.TextAlignment = label.HorizontalTextAlignment.ToPlatform(true);
		}

		public static void UpdateVerticalTextAlignment(this TextBlock platformControl, ILabel label)
		{
			platformControl.VerticalAlignment = label.VerticalTextAlignment.ToPlatformVerticalAlignment();
		}

		internal static void UpdateTextHtml(this TextBlock platformControl, ILabel label)
		{
			var text = label.Text ?? string.Empty;

			// Just in case we are not given text with elements.
			var modifiedText = string.Format("<div>{0}</div>", text);
			modifiedText = Regex.Replace(modifiedText, "<br>", "<br></br>", RegexOptions.IgnoreCase);

			// Reset the text because we will add to it.
			platformControl.Inlines.Clear();

			try
			{
				var element = XElement.Parse(modifiedText);
				LabelHtmlHelper.ParseText(element, platformControl.Inlines, label);
			}
			catch (Exception)
			{
				// If anything goes wrong just show the html
				platformControl.Text = global::Windows.Data.Html.HtmlUtilities.ConvertToText(label.Text);
			}
		}

		public static void UpdateLineBreakMode(this TextBlock platformControl, ILabel label)
		{
			var lineBreakMode = label.LineBreakMode;

			switch (lineBreakMode)
			{
				case LineBreakMode.NoWrap:
					platformControl.TextTrimming = TextTrimming.Clip;
					platformControl.TextWrapping = TextWrapping.NoWrap;
					break;
				case LineBreakMode.WordWrap:
					platformControl.TextTrimming = TextTrimming.None;
					platformControl.TextWrapping = TextWrapping.Wrap;
					break;
				case LineBreakMode.CharacterWrap:
					platformControl.TextTrimming = TextTrimming.WordEllipsis;
					platformControl.TextWrapping = TextWrapping.Wrap;
					break;
				case LineBreakMode.HeadTruncation:
					// TODO: This truncates at the end.
					platformControl.TextTrimming = TextTrimming.WordEllipsis;
					platformControl.DetermineTruncatedTextWrapping();
					break;
				case LineBreakMode.TailTruncation:
					platformControl.TextTrimming = TextTrimming.CharacterEllipsis;
					platformControl.DetermineTruncatedTextWrapping();
					break;
				case LineBreakMode.MiddleTruncation:
					// TODO: This truncates at the end.
					platformControl.TextTrimming = TextTrimming.WordEllipsis;
					platformControl.DetermineTruncatedTextWrapping();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal static void DetermineTruncatedTextWrapping(this TextBlock textBlock) =>
			textBlock.TextWrapping = textBlock.MaxLines > 1 ? TextWrapping.Wrap : TextWrapping.NoWrap;

		internal static void UpdateTextPlainText(this TextBlock platformControl, IText label)
		{
			platformControl.Text = label.Text;
		}
	}
}