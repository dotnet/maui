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

		public static void UpdateMaximumLines(this TextBlock platformControl, ILabel label) 
		{
			var maxLines = label.MaximumLines;
			if (maxLines < 1)
			{
				maxLines = 0;
			}

			platformControl.MaxLines = maxLines;
		}

		public static void UpdateTextWrapMode(this TextBlock platformControl, ILabel label)
		{
			switch (label.TextWrapMode)
			{
				case TextWrapMode.None:
					platformControl.TextWrapping = TextWrapping.NoWrap;
					break;
				case TextWrapMode.Word:
					platformControl.TextWrapping = TextWrapping.WrapWholeWords;
					break;
				case TextWrapMode.Character:
					platformControl.TextWrapping = TextWrapping.Wrap;
					break;
			}
		}

		public static void UpdateTextOverflowMode(this TextBlock platformControl, ILabel label)
		{
			switch (label.TextOverflowMode)
			{
				case TextOverflowMode.None:
					platformControl.TextTrimming = TextTrimming.None;
					break;
				case TextOverflowMode.Truncate:
					platformControl.TextTrimming = TextTrimming.Clip;
					break;
				case TextOverflowMode.EllipsizeEnd:
					// This is a mode that Windows has built in
					platformControl.TextTrimming = TextTrimming.WordEllipsis;
					break;
				case TextOverflowMode.EllipsizeStart:
					// Start and middle aren't built into Windows. For the moment, we'll just
					// ignore that, do end, and document that these two don't really work. 
					// Eventually, we might be able to work out a scheme or a custom TextBlock
					// which _does_ handle these cases.
					platformControl.TextTrimming = TextTrimming.WordEllipsis;
					break;
				case TextOverflowMode.EllipsizeMiddle:
					platformControl.TextTrimming = TextTrimming.WordEllipsis;
					break;
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

		internal static void UpdateTextPlainText(this TextBlock platformControl, IText label)
		{
			platformControl.Text = label.Text;
		}
	}
}