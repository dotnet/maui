using System;

namespace PoolMathApp.Models
{
	public class TextSpan
	{
		public TextSpan()
		{
		}

		public TextSpan(
			string text,
			string color = null,
			string colorKey = null,
			string fontFamily = null,
			double? fontSize = default,
			NamedFontSize? namedFontSize = default,
			bool? bold = false,
			bool? italic = false,
			bool? underline = false,
			FontWeight? fontWeight = null)
		{
			Text = text;
			Color = color;
			ColorKey = colorKey;
			FontFamily = fontFamily;
			FontSize = fontSize;
			NamedFontSize = namedFontSize;
			Bold = bold;
			Italic = italic;
			Underline = underline;
			FontWeight = fontWeight;
		}

		public string Text { get; set; }

		public string Color { get; set; }
		public string ColorKey { get; set; }

		public string FontFamily { get; set; }

		public double? FontSize { get; set; }
		public NamedFontSize? NamedFontSize { get; set; }
		public FontWeight? FontWeight { get; set; }

		public bool? Bold { get; set; }
		public bool? Italic { get; set; }
		public bool? Underline { get; set; }
	}
}
