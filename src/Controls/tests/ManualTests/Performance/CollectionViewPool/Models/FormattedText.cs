using System.Collections.Generic;
using System.Linq;

namespace PoolMathApp.Models
{
	public class FormattedText
	{
		public FormattedText()
		: this(Array.Empty<TextSpan>())
		{
		}

		public FormattedText(
			string defaultColor = null,
			string defaultColorKey = null,
			string defaultFontFamily = null,
			double? defaultFontSize = default,
			NamedFontSize? defaultNamedFontSize = default,
			bool? defaultBold = null,
			bool? defaultItalic = null,
			bool? defaultUnderline = null,
			FontWeight? defaultFontWeight = null,
			params TextSpan[] spans)
		{
			DefaultColor = defaultColor;
			DefaultColorKey = defaultColorKey;
			DefaultFontFamily = defaultFontFamily;
			DefaultFontSize = defaultFontSize;
			DefaultNamedFontSize = defaultNamedFontSize;
			DefaultBold = defaultBold;
			DefaultItalic = defaultItalic;
			DefaultUnderline = defaultUnderline;
			DefaultFontWeight = defaultFontWeight;

			InitSpans(spans);
		}

		public readonly string DefaultColor;
		public readonly string DefaultColorKey;
		public readonly string DefaultFontFamily;
		public readonly double? DefaultFontSize;
		public readonly NamedFontSize? DefaultNamedFontSize;
		public readonly bool? DefaultBold;
		public readonly bool? DefaultItalic;
		public readonly bool? DefaultUnderline;
		public readonly FontWeight? DefaultFontWeight;

		public FormattedText(IEnumerable<TextSpan> spans)
			=> InitSpans(spans);

		public FormattedText(params TextSpan[] spans)
			=> InitSpans(spans);

		void InitSpans(IEnumerable<TextSpan> spans)
		{
			textSpans = new List<TextSpan>();
			if (spans?.Any() ?? false)
			{
				foreach (var s in spans)
					textSpans.Add(UpdateSpanDefaults(s));
			}
		}

		List<TextSpan> textSpans = new();


		public IReadOnlyList<TextSpan> Spans => textSpans;

		public void Add(TextSpan span)
			=> textSpans.Add(UpdateSpanDefaults(span));

		TextSpan UpdateSpanDefaults(TextSpan span)
		{
			if (span.Color is null && DefaultColor is not null)
				span.Color = DefaultColor;
			if (span.ColorKey is null && DefaultColorKey is not null)
				span.ColorKey = DefaultColorKey;

			if (span.FontFamily is null && DefaultFontFamily is not null)
				span.FontFamily = DefaultFontFamily;
			if (span.FontSize is null && DefaultFontSize is not null)
				span.FontSize = DefaultFontSize;
			if (span.FontWeight is null && DefaultFontWeight is not null)
				span.FontWeight = DefaultFontWeight;
			if (span.NamedFontSize is null && DefaultNamedFontSize is not null)
				span.NamedFontSize = DefaultNamedFontSize;

			if (span.Bold is null && DefaultBold is not null)
				span.Bold = DefaultBold;
			if (span.Italic is null && DefaultItalic is not null)
				span.Italic = DefaultItalic;
			if (span.Underline is null && DefaultUnderline is not null)
				span.Underline = DefaultUnderline;

			return span;
		}

		public void Add(string text,
			string color = null,
			string colorKey = null,
			string fontFamily = null,
			double? fontSize = default,
			NamedFontSize? namedFontSize = default,
			bool bold = false,
			bool italic = false,
			bool underline = false,
			FontWeight? fontWeight = null)
			=> textSpans.Add(UpdateSpanDefaults(new TextSpan(text, color, colorKey, fontFamily, fontSize, namedFontSize, bold, italic, underline, fontWeight)));
	}
}
