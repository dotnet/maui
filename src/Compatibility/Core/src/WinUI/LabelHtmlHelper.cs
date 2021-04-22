using System.Xml.Linq;
using Microsoft.UI.Xaml.Documents;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP
{
	internal static class LabelHtmlHelper
	{
		// All the supported HTML tags
		internal const string ElementB = "B";
		internal const string ElementBr = "BR";
		internal const string ElementEm = "EM";
		internal const string ElementI = "I";
		internal const string ElementP = "P";
		internal const string ElementStrong = "STRONG";
		internal const string ElementU = "U";
		internal const string ElementUl = "UL";
		internal const string ElementLi = "LI";
		internal const string ElementDiv = "DIV";

		public static void ParseText(XElement element, InlineCollection inlines, Label label)
		{
			if (element == null)
				return;

			var currentInlines = inlines;
			var elementName = element.Name.ToString().ToUpper();
			switch (elementName)
			{
				case ElementB:
				case ElementStrong:
					var bold = new Bold();
					inlines.Add(bold);
					currentInlines = bold.Inlines;
					break;
				case ElementI:
				case ElementEm:
					var italic = new Italic();
					inlines.Add(italic);
					currentInlines = italic.Inlines;
					break;
				case ElementU:
					var underline = new Underline();
					inlines.Add(underline);
					currentInlines = underline.Inlines;
					break;
				case ElementBr:
					inlines.Add(new LineBreak());
					break;
				case ElementP:
					// Add two line breaks, one for the current text and the second for the gap.
					if (AddLineBreakIfNeeded(inlines))
					{
						inlines.Add(new LineBreak());
					}

					var paragraphSpan = new Microsoft.UI.Xaml.Documents.Span();
					inlines.Add(paragraphSpan);
					currentInlines = paragraphSpan.Inlines;
					break;
				case ElementLi:
					inlines.Add(new LineBreak());
					inlines.Add(new Run { Text = " • " });
					break;
				case ElementUl:
				case ElementDiv:
					AddLineBreakIfNeeded(inlines);
					var divSpan = new Microsoft.UI.Xaml.Documents.Span();
					inlines.Add(divSpan);
					currentInlines = divSpan.Inlines;
					break;
			}
			foreach (var node in element.Nodes())
			{
				if (node is XText textElement)
				{
					currentInlines.Add(new Run { Text = textElement.Value });
				}
				else
				{
					ParseText(node as XElement, currentInlines, label);
				}
			}
			// Add newlines for paragraph tags
			if (elementName == "ElementP")
			{
				currentInlines.Add(new LineBreak());
			}
		}

		static bool AddLineBreakIfNeeded(InlineCollection inlines)
		{
			if (inlines.Count <= 0)
				return false;

			var lastInline = inlines[inlines.Count - 1];
			while ((lastInline is Microsoft.UI.Xaml.Documents.Span))
			{
				var span = (Microsoft.UI.Xaml.Documents.Span)lastInline;
				if (span.Inlines.Count > 0)
				{
					lastInline = span.Inlines[span.Inlines.Count - 1];
				}
			}

			if (lastInline is LineBreak)
				return false;

			inlines.Add(new LineBreak());
			return true;
		}
	}
}