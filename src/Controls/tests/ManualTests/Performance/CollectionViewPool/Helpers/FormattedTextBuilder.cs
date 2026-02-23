using System;
using System.Collections.Generic;
using System.Text;


namespace PoolMathApp.Xaml
{

	class SpanBuilder
	{
		public static Span Create(string text, string colorResourceKey = default, string fontSizeResource = null, string fontResourceKey = null)
		{
			var s = new Span();
			s.Text = text;

			if (!string.IsNullOrEmpty(fontSizeResource))
				s.SetDynamicResource(Span.FontSizeProperty, fontSizeResource);

			if (!string.IsNullOrEmpty(colorResourceKey))
				s.SetDynamicResource(Span.TextColorProperty, colorResourceKey);

			if (!string.IsNullOrEmpty(fontResourceKey))
				s.SetDynamicResource(Span.FontFamilyProperty, fontResourceKey);

			return s;
		}
	}
}
