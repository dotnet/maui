using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using PoolMathApp.Models;

namespace PoolMathApp.Helpers
{
	static class FormattedTextExtensions
	{
		public static FormattedString ToFormattedString(this FormattedText text)
		{
			var fs = new FormattedString();

			var textSpans = text.Spans;

			foreach (var s in textSpans)
				fs.Spans.Add(s.ToSpan());

			return fs;
		}

		public static Span ToSpan(this TextSpan textSpan)
		{
			var s = new Span();

			if (!string.IsNullOrEmpty(textSpan.Text))
				s.Text = textSpan.Text;
			if (textSpan.Color is not null)
				s.TextColor = Color.FromArgb(textSpan.Color);
			if (textSpan.ColorKey is not null)
				s.SetDynamicResource(Span.TextColorProperty, textSpan.ColorKey);
			if (textSpan.FontFamily is not null)
				s.FontFamily = textSpan.FontFamily;
			if (textSpan.FontWeight is not null)
				s.FontFamily = textSpan.FontWeight.Value.ToFontAlias();
			if (textSpan.FontSize is not null)
				s.FontSize = textSpan.FontSize.Value;
			if (textSpan.NamedFontSize is not null)
				s.SetDynamicResource(Span.FontSizeProperty, textSpan.NamedFontSize.Value.ToFontSizeResourceKey());
			if (textSpan.Bold ?? false)
				s.FontAttributes |= FontAttributes.Bold;
			if (textSpan.Italic ?? false)
				s.FontAttributes |= FontAttributes.Italic;
			if (textSpan.Underline ?? false)
				s.TextDecorations |= TextDecorations.Underline;

			return s;
		}
	}
}
