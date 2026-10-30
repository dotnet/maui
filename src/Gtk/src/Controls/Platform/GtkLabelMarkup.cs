using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Gtk.Platform
{
	/// <summary>
	/// Builds Pango markup for a <c>Gtk.Label</c> from the formatted text (<see cref="FormattedString"/>) and the HTML text of a Controls
	/// <see cref="Label"/>. Every span becomes one <c>&lt;span&gt;</c> carrying only its own discrete attributes, so the label's own font and
	/// colour (CSS) show through where a span sets nothing.
	/// </summary>
	public static class GtkLabelMarkup
	{
		// Pango sizes are 1024ths of a point; the CSS and MAUI lengths of the backend are pixels at 96 dpi (1 px = 0.75 pt).
		const double PointsPerPixel = 0.75;

		/// <summary>Escapes text for Pango markup.</summary>
		/// <param name="text">The text.</param>
		public static string Escape(string? text)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			var builder = new StringBuilder(text!.Length + 8);
			foreach (var c in text)
			{
				switch (c)
				{
					case '&': builder.Append("&amp;"); break;
					case '<': builder.Append("&lt;"); break;
					case '>': builder.Append("&gt;"); break;
					case '"': builder.Append("&quot;"); break;
					case '\'': builder.Append("&#39;"); break;
					default: builder.Append(c); break;
				}
			}

			return builder.ToString();
		}

		/// <summary>The Pango markup of the spans of <paramref name="label"/>.</summary>
		/// <param name="label">The label whose <see cref="Label.FormattedText"/> is converted.</param>
		/// <param name="resolveFontFamily">Maps a MAUI font family (alias) to the installed family name, or <see langword="null"/> to use it as is.</param>
		public static string FromFormattedText(Label label, Func<string?, string?>? resolveFontFamily = null)
		{
			if (label is null)
				throw new ArgumentNullException(nameof(label));

			var builder = new StringBuilder();
			var spans = label.FormattedText?.Spans;
			if (spans is null)
				return string.Empty;

			foreach (var span in spans)
			{
				var text = TextTransformUtilities.GetTransformedText(span.Text, label.TextTransform);
				if (string.IsNullOrEmpty(text))
					continue;

				var attributes = new StringBuilder();

				if (span.IsSet(Span.TextColorProperty) && span.TextColor is { } color)
					attributes.Append(" foreground=\"").Append(ColorToMarkup(color)).Append('"');

				if (span.IsSet(Span.BackgroundColorProperty) && span.BackgroundColor is { } background)
					attributes.Append(" background=\"").Append(ColorToMarkup(background)).Append('"');

				if (span.IsSet(Span.FontFamilyProperty) && !string.IsNullOrEmpty(span.FontFamily))
				{
					var family = resolveFontFamily?.Invoke(span.FontFamily) ?? span.FontFamily;
					attributes.Append(" font_family=\"").Append(Escape(family)).Append('"');
				}

				if (span.IsSet(Span.FontSizeProperty) && span.FontSize > 0)
					attributes.Append(" size=\"").Append(ToPangoUnits(span.FontSize).ToString(CultureInfo.InvariantCulture)).Append('"');

				if (span.IsSet(Span.FontAttributesProperty))
				{
					if (span.FontAttributes.HasFlag(FontAttributes.Bold))
						attributes.Append(" weight=\"bold\"");
					if (span.FontAttributes.HasFlag(FontAttributes.Italic))
						attributes.Append(" style=\"italic\"");
				}

				if (span.IsSet(Span.TextDecorationsProperty))
				{
					if (span.TextDecorations.HasFlag(TextDecorations.Underline))
						attributes.Append(" underline=\"single\"");
					if (span.TextDecorations.HasFlag(TextDecorations.Strikethrough))
						attributes.Append(" strikethrough=\"true\"");
				}

				if (span.IsSet(Span.CharacterSpacingProperty) && span.CharacterSpacing != 0)
					attributes.Append(" letter_spacing=\"").Append(ToPangoUnits(span.CharacterSpacing).ToString(CultureInfo.InvariantCulture)).Append('"');

				if (span.IsSet(Span.LineHeightProperty) && span.LineHeight > 0)
					attributes.Append(" line_height=\"").Append(span.LineHeight.ToString("0.###", CultureInfo.InvariantCulture)).Append('"');

				if (attributes.Length == 0)
					builder.Append(Escape(text));
				else
					builder.Append("<span").Append(attributes).Append('>').Append(Escape(text)).Append("</span>");
			}

			return builder.ToString();
		}

		static int ToPangoUnits(double pixels) => (int)Math.Round(pixels * PointsPerPixel * 1024);

		/// <summary>A Pango colour specification (<c>#rrggbb</c> or <c>#rrggbbaa</c>).</summary>
		/// <param name="color">The colour.</param>
		public static string ColorToMarkup(Color color)
		{
			int r = (int)Math.Round(color.Red * 255);
			int g = (int)Math.Round(color.Green * 255);
			int b = (int)Math.Round(color.Blue * 255);
			int a = (int)Math.Round(color.Alpha * 255);
			return a >= 255
				? string.Format(CultureInfo.InvariantCulture, "#{0:x2}{1:x2}{2:x2}", r, g, b)
				: string.Format(CultureInfo.InvariantCulture, "#{0:x2}{1:x2}{2:x2}{3:x2}", r, g, b, a);
		}

		/// <summary>
		/// Converts a small, forgiving HTML subset to Pango markup: <c>b strong i em u ins s strike del sub sup small big code tt a br p div
		/// h1-h6 li ul ol font span</c> (inline <c>color</c>, <c>background-color</c>, <c>font-weight</c>, <c>font-style</c> and
		/// <c>text-decoration</c> styles). Other tags are dropped and their text kept; entities are decoded and the result is escaped.
		/// </summary>
		/// <param name="html">The HTML text.</param>
		public static string FromHtml(string? html)
		{
			if (string.IsNullOrEmpty(html))
				return string.Empty;

			var output = new StringBuilder(html!.Length + 16);
			var open = new List<(string Tag, string Close)>();
			int i = 0;
			bool pendingBreak = false;
			int listDepth = 0;

			void StartBlock()
			{
				if (output.Length > 0 && output[output.Length - 1] != '\n')
					output.Append('\n');
			}

			while (i < html.Length)
			{
				char c = html[i];
				if (c != '<')
				{
					int next = html.IndexOf('<', i);
					if (next < 0)
						next = html.Length;

					AppendText(output, html.Substring(i, next - i), ref pendingBreak);
					i = next;
					continue;
				}

				int end = html.IndexOf('>', i);
				if (end < 0)
				{
					AppendText(output, html.Substring(i), ref pendingBreak);
					break;
				}

				var raw = html.Substring(i + 1, end - i - 1).Trim();
				i = end + 1;

				if (raw.StartsWith("!", StringComparison.Ordinal) || raw.StartsWith("?", StringComparison.Ordinal))
					continue;

				bool closing = raw.StartsWith("/", StringComparison.Ordinal);
				if (closing)
					raw = raw.Substring(1).TrimStart();
				bool selfClosing = raw.EndsWith("/", StringComparison.Ordinal);
				if (selfClosing)
					raw = raw.Substring(0, raw.Length - 1).TrimEnd();

				int space = 0;
				while (space < raw.Length && !char.IsWhiteSpace(raw[space]))
					space++;
				var name = raw.Substring(0, space).ToLowerInvariant();
				var attributes = space < raw.Length ? raw.Substring(space) : string.Empty;

				if (closing)
				{
					CloseTag(output, open, name);
					if (name is "p" or "div" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6" or "li")
						StartBlock();
					if (name is "ul" or "ol")
						listDepth = Math.Max(0, listDepth - 1);
					continue;
				}

				switch (name)
				{
					case "br":
						output.Append('\n');
						break;
					case "p":
					case "div":
						StartBlock();
						break;
					case "ul":
					case "ol":
						StartBlock();
						listDepth++;
						break;
					case "li":
						StartBlock();
						output.Append(new string(' ', Math.Max(0, (listDepth - 1) * 2))).Append("• ");
						break;
					case "h1":
					case "h2":
					case "h3":
					case "h4":
					case "h5":
					case "h6":
						StartBlock();
						Open(output, open, name, "<span weight=\"bold\" size=\"" + (name == "h1" ? "xx-large" : name == "h2" ? "x-large" : name == "h3" ? "large" : "medium") + "\">", "</span>");
						break;
					case "b":
					case "strong":
						Open(output, open, name, "<b>", "</b>");
						break;
					case "i":
					case "em":
					case "cite":
					case "dfn":
						Open(output, open, name, "<i>", "</i>");
						break;
					case "u":
					case "ins":
						Open(output, open, name, "<u>", "</u>");
						break;
					case "s":
					case "strike":
					case "del":
						Open(output, open, name, "<s>", "</s>");
						break;
					case "sub":
						Open(output, open, name, "<sub>", "</sub>");
						break;
					case "sup":
						Open(output, open, name, "<sup>", "</sup>");
						break;
					case "small":
						Open(output, open, name, "<small>", "</small>");
						break;
					case "big":
						Open(output, open, name, "<big>", "</big>");
						break;
					case "code":
					case "tt":
					case "kbd":
					case "samp":
						Open(output, open, name, "<tt>", "</tt>");
						break;
					case "a":
						{
							var href = GetAttribute(attributes, "href");
							if (href is not null)
								Open(output, open, name, "<a href=\"" + Escape(WebUtility.HtmlDecode(href)) + "\">", "</a>");
							else
								Open(output, open, name, string.Empty, string.Empty);
							break;
						}
					case "font":
						{
							var color = GetAttribute(attributes, "color");
							Open(output, open, name, color is null ? string.Empty : "<span foreground=\"" + Escape(color) + "\">", color is null ? string.Empty : "</span>");
							break;
						}
					case "span":
						{
							var style = GetAttribute(attributes, "style");
							var pango = StyleToSpan(style);
							Open(output, open, name, pango, pango.Length == 0 ? string.Empty : "</span>");
							break;
						}
					default:
						// Unknown tag: keep the text, remember the tag so the matching close is balanced.
						if (!selfClosing)
							Open(output, open, name, string.Empty, string.Empty);
						break;
				}
			}

			for (int j = open.Count - 1; j >= 0; j--)
				output.Append(open[j].Close);

			// Trailing line breaks that block elements leave behind.
			int length = output.Length;
			while (length > 0 && output[length - 1] == '\n')
				length--;
			output.Length = length;

			return output.ToString();
		}

		static void Open(StringBuilder output, List<(string Tag, string Close)> open, string tag, string start, string close)
		{
			output.Append(start);
			open.Add((tag, close));
		}

		static void CloseTag(StringBuilder output, List<(string Tag, string Close)> open, string tag)
		{
			int index = -1;
			for (int j = open.Count - 1; j >= 0; j--)
			{
				if (open[j].Tag == tag)
				{
					index = j;
					break;
				}
			}

			if (index < 0)
				return;

			for (int j = open.Count - 1; j >= index; j--)
				output.Append(open[j].Close);

			open.RemoveRange(index, open.Count - index);
		}

		static void AppendText(StringBuilder output, string text, ref bool pendingBreak)
		{
			var decoded = WebUtility.HtmlDecode(text);
			var collapsed = new StringBuilder(decoded.Length);
			bool lastSpace = output.Length == 0 || output[output.Length - 1] == '\n' || output[output.Length - 1] == ' ';
			foreach (var c in decoded)
			{
				if (char.IsWhiteSpace(c))
				{
					if (!lastSpace)
						collapsed.Append(' ');
					lastSpace = true;
				}
				else
				{
					collapsed.Append(c);
					lastSpace = false;
				}
			}

			output.Append(Escape(collapsed.ToString()));
			pendingBreak = false;
		}

		static string? GetAttribute(string attributes, string name)
		{
			int index = 0;
			while (index < attributes.Length)
			{
				index = attributes.IndexOf(name, index, StringComparison.OrdinalIgnoreCase);
				if (index < 0)
					return null;

				bool startsToken = index == 0 || char.IsWhiteSpace(attributes[index - 1]);
				int cursor = index + name.Length;
				while (cursor < attributes.Length && char.IsWhiteSpace(attributes[cursor]))
					cursor++;

				if (!startsToken || cursor >= attributes.Length || attributes[cursor] != '=')
				{
					index += name.Length;
					continue;
				}

				cursor++;
				while (cursor < attributes.Length && char.IsWhiteSpace(attributes[cursor]))
					cursor++;
				if (cursor >= attributes.Length)
					return string.Empty;

				char quote = attributes[cursor];
				if (quote == '"' || quote == '\'')
				{
					int close = attributes.IndexOf(quote, cursor + 1);
					return close < 0 ? attributes.Substring(cursor + 1) : attributes.Substring(cursor + 1, close - cursor - 1);
				}

				int stop = cursor;
				while (stop < attributes.Length && !char.IsWhiteSpace(attributes[stop]))
					stop++;
				return attributes.Substring(cursor, stop - cursor);
			}

			return null;
		}

		static string StyleToSpan(string? style)
		{
			if (string.IsNullOrWhiteSpace(style))
				return string.Empty;

			var attributes = new StringBuilder();
			foreach (var declaration in style!.Split(';'))
			{
				int colon = declaration.IndexOf(':', StringComparison.Ordinal);
				if (colon < 0)
					continue;

				var property = declaration.Substring(0, colon).Trim().ToLowerInvariant();
				var value = declaration.Substring(colon + 1).Trim();

				switch (property)
				{
					case "color":
						attributes.Append(" foreground=\"").Append(Escape(value)).Append('"');
						break;
					case "background-color":
					case "background":
						attributes.Append(" background=\"").Append(Escape(value)).Append('"');
						break;
					case "font-weight":
						if (value.Equals("bold", StringComparison.OrdinalIgnoreCase) || value.Equals("bolder", StringComparison.OrdinalIgnoreCase)
							|| (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var weight) && weight >= 600))
							attributes.Append(" weight=\"bold\"");
						break;
					case "font-style":
						if (value.Equals("italic", StringComparison.OrdinalIgnoreCase) || value.Equals("oblique", StringComparison.OrdinalIgnoreCase))
							attributes.Append(" style=\"italic\"");
						break;
					case "text-decoration":
						if (value.Contains("underline", StringComparison.OrdinalIgnoreCase))
							attributes.Append(" underline=\"single\"");
						if (value.Contains("line-through", StringComparison.OrdinalIgnoreCase))
							attributes.Append(" strikethrough=\"true\"");
						break;
				}
			}

			return attributes.Length == 0 ? string.Empty : "<span" + attributes + ">";
		}
	}
}
