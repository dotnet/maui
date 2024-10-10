using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Resolvers;
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

		public static void UpdateFont(this TextBlock platformControl, ILabel label, IFontManager fontManager)
		{
			var font = label.Font;

			var fontSize = fontManager.GetFontSize(font);

			if (!label.IsPlatformViewNew || fontSize != fontManager.DefaultFontSize)
			{
				platformControl.FontSize = fontSize;
			}

			UI.Xaml.Media.FontFamily fontFamily = fontManager.GetFontFamily(font);

			if (!label.IsPlatformViewNew || fontFamily != fontManager.DefaultFontFamily)
			{
				platformControl.FontFamily = fontFamily;
			}

			var fontStyle = font.ToFontStyle();

			if (!label.IsPlatformViewNew || fontStyle != global::Windows.UI.Text.FontStyle.Normal)
			{
				platformControl.FontStyle = fontStyle;
			}

			if (!label.IsPlatformViewNew || font.Weight != FontWeight.Regular)
			{
				platformControl.FontWeight = font.ToFontWeight();
			}

			if (!label.IsPlatformViewNew || !font.AutoScalingEnabled)
			{
				platformControl.IsTextScaleFactorEnabled = font.AutoScalingEnabled;
			}
		}

		public static void UpdateText(this TextBlock platformControl, ILabel label)
		{
			platformControl.UpdateTextPlainText(label);
		}

		public static void UpdateTextColor(this TextBlock platformControl, ILabel label)
		{
			if (!label.IsPlatformViewNew || label.TextColor is not null)
			{
				platformControl.UpdateProperty(TextBlock.ForegroundProperty, label.TextColor);
			}
		}

		public static void UpdatePadding(this TextBlock platformControl, ILabel label)
		{
			if (!label.IsPlatformViewNew || !label.Padding.IsEmpty)
			{
				platformControl.UpdateProperty(TextBlock.PaddingProperty, label.Padding.ToPlatform());
			}
		}

		public static void UpdateCharacterSpacing(this TextBlock platformControl, ILabel label)
		{
			if (!label.IsPlatformViewNew || label.CharacterSpacing != 0)
			{
				platformControl.CharacterSpacing = label.CharacterSpacing.ToEm();
			}
		}

		public static void UpdateTextDecorations(this TextBlock platformControl, ILabel label)
		{
			var elementTextDecorations = label.TextDecorations;

			if (!label.IsPlatformViewNew || elementTextDecorations != TextDecorations.None)
			{
				if ((elementTextDecorations & TextDecorations.Underline) == 0)
					platformControl.TextDecorations &= ~global::Windows.UI.Text.TextDecorations.Underline;
				else
					platformControl.TextDecorations |= global::Windows.UI.Text.TextDecorations.Underline;

				if ((elementTextDecorations & TextDecorations.Strikethrough) == 0)
					platformControl.TextDecorations &= ~global::Windows.UI.Text.TextDecorations.Strikethrough;
				else
					platformControl.TextDecorations |= global::Windows.UI.Text.TextDecorations.Strikethrough;
			}
		}

		public static void UpdateLineHeight(this TextBlock platformControl, ILabel label)
		{
			if (!label.IsPlatformViewNew || label.LineHeight > 0)
			{
				if (label.LineHeight >= 0)
				{
					platformControl.LineHeight = label.LineHeight * platformControl.FontSize;
				}
			}
		}

		public static void UpdateHorizontalTextAlignment(this TextBlock platformControl, ILabel label)
		{
			if (!label.IsPlatformViewNew || label.HorizontalTextAlignment != TextAlignment.Start)
			{
				// We don't have a FlowDirection yet, so there's nothing to pass in here. 
				// TODO: Update this when FlowDirection is available 
				platformControl.TextAlignment = label.HorizontalTextAlignment.ToPlatform(true);
			}
		}

		public static void UpdateVerticalTextAlignment(this TextBlock platformControl, ILabel label)
		{
			// Default is stretch.
			//if (!label.IsPlatformViewNew || label.VerticalTextAlignment != TextAlignment.)
			{
				platformControl.VerticalAlignment = label.VerticalTextAlignment.ToPlatformVerticalAlignment();
			}
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
				var element = ParseXhtml(modifiedText);
				LabelHtmlHelper.ParseText(element, platformControl.Inlines, label);
			}
			catch (Exception)
			{
				// If anything goes wrong just show the html
				platformControl.Text = label.Text;
			}
		}

		internal static void UpdateTextPlainText(this TextBlock platformControl, IText label)
		{
			platformControl.Text = label.Text;
		}

		static XElement? ParseXhtml(string? html)
		{
			if (string.IsNullOrEmpty(html))
				return null;

			XmlNameTable nt = new NameTable();
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
			var xmlParserContext = new XmlParserContext(null, nsmgr, null, XmlSpace.None);
			XmlParserContext context = xmlParserContext;
			context.DocTypeName = "html";
			context.PublicId = "-//W3C//DTD XHTML 1.0 Strict//EN";
			context.SystemId = "xhtml1-strict.dtd";
			XmlParserContext xhtmlContext = context;

			StringReader stringReader = new StringReader(html);

			XmlReaderSettings settings = new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Parse,
				ValidationType = ValidationType.DTD,
				XmlResolver = new XmlPreloadedResolver(XmlKnownDtds.All)
			};

			XmlReader reader = XmlReader.Create(stringReader, settings, xhtmlContext);

			return XElement.Load(reader);
		}
	}
}