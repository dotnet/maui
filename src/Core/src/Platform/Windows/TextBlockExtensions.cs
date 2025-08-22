using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Resolvers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using WThickness = Microsoft.UI.Xaml.Thickness;

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

		public static void UpdatePadding(this TextBlock platformControl, ILabel label)
		{
			// Label padding values do not support negative values; if specified, negative values will be replaced with zero
			var padding = new WThickness(
				Math.Max(0, label.Padding.Left),
				Math.Max(0, label.Padding.Top),
				Math.Max(0, label.Padding.Right),
				Math.Max(0, label.Padding.Bottom)
			);

			platformControl.UpdateProperty(TextBlock.PaddingProperty, padding);
		}

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
		}

		public static void UpdateLineHeight(this TextBlock platformControl, ILabel label)
		{
			if (label.LineHeight >= 0)
			{
				platformControl.LineHeight = label.LineHeight * platformControl.FontSize;
				platformControl.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
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