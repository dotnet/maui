using System;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// Pango helpers shared by <see cref="CairoCanvas"/> and <see cref="PangoStringSizeService"/>.
	/// </summary>
	internal static class PangoText
	{
		public const double PangoScale = 1024d;

		/// <summary>The family used when an <see cref="IFont"/> has no name (a fontconfig/Pango alias).</summary>
		public const string DefaultFamily = "Sans";

		public static Pango.FontDescription CreateFontDescription(IFont font, float size)
		{
			var description = new Pango.FontDescription();
			description.Family = string.IsNullOrEmpty(font?.Name) ? DefaultFamily : font.Name;

			var weight = font?.Weight ?? FontWeights.Normal;
			if (weight <= 0)
				weight = FontWeights.Normal;
			// Pango.Weight uses the same 100..1000 scale as FontWeights.
			description.Weight = (Pango.Weight)Math.Min(Math.Max(weight, 100), 1000);

			description.Style = (font?.StyleType ?? FontStyleType.Normal) switch
			{
				FontStyleType.Italic => Pango.Style.Italic,
				FontStyleType.Oblique => Pango.Style.Oblique,
				_ => Pango.Style.Normal,
			};

			// Absolute size: one font unit is one cairo user unit, whatever the display resolution.
			description.AbsoluteSize = Math.Max(size, 0.01f) * PangoScale;
			return description;
		}

		/// <summary>
		/// Pins the font options Pango reads from the Cairo context: grayscale (or no) anti-aliasing, because text is
		/// drawn on transparent surfaces where sub-pixel (LCD) rendering would give per-channel coverage.
		/// </summary>
		public static void ApplyTextOptions(CairoContext context, bool antialias)
		{
			using (var options = new global::Cairo.FontOptions())
			{
				options.Antialias = antialias ? global::Cairo.Antialias.Gray : global::Cairo.Antialias.None;
				context.FontOptions = options;
			}
		}

		public static Pango.Layout CreateLayout(CairoContext context, string text, IFont font, float size)
		{
			Pango.Layout layout;
			context.Save();
			try
			{
				ApplyTextOptions(context, true);
				layout = Pango.CairoHelper.CreateLayout(context);
			}
			finally
			{
				context.Restore();
			}

			Pango.CairoHelper.ContextSetResolution(layout.Context, 96);
			using (var description = CreateFontDescription(font, size))
				layout.FontDescription = description;
			layout.SetText(text ?? string.Empty);
			return layout;
		}

		/// <summary>Gets the logical size (user units) and the baseline offset of a laid out text.</summary>
		public static void Measure(Pango.Layout layout, out double width, out double height, out double baseline)
		{
			layout.GetExtents(out _, out var logical);
			width = logical.Width / PangoScale;
			height = logical.Height / PangoScale;
			baseline = layout.Baseline / PangoScale;
		}

		/// <summary>Builds Pango markup for <paramref name="text"/> out of its attribute runs.</summary>
		public static string ToMarkup(IAttributedText text)
		{
			var value = text.Text ?? string.Empty;
			var runs = text.Runs;
			var builder = new StringBuilder(value.Length + 32);
			var position = 0;

			if (runs != null)
			{
				for (var i = 0; i < runs.Count; i++)
				{
					var run = runs[i];
					var start = Math.Max(run.Start, position);
					var end = Math.Min(run.Start + run.Length, value.Length);
					if (end <= start)
						continue;

					if (start > position)
						Escape(builder, value, position, start - position);

					AppendRun(builder, run.Attributes, value, start, end - start);
					position = end;
				}
			}

			if (position < value.Length)
				Escape(builder, value, position, value.Length - position);

			return builder.ToString();
		}

		private static void AppendRun(StringBuilder builder, Microsoft.Maui.Graphics.Text.ITextAttributes attributes, string value, int start, int length)
		{
			if (attributes == null || attributes.Count == 0)
			{
				Escape(builder, value, start, length);
				return;
			}

			var close = new StringBuilder();
			var superscript = attributes.GetSuperscript();
			var subscript = attributes.GetSubscript();

			builder.Append("<span");

			var fontName = attributes.GetFontName();
			var hasFontDesc = !string.IsNullOrEmpty(fontName) || attributes.ContainsKey(TextAttribute.FontSize);
			if (hasFontDesc)
			{
				var size = attributes.GetFontSize();
				var family = string.IsNullOrEmpty(fontName) ? null : fontName;
				if (family != null)
					builder.Append(" font_family=\"").Append(EscapeAttribute(family)).Append('"');
				if (attributes.ContainsKey(TextAttribute.FontSize))
					// Markup sizes are in 1024ths of a point; the layout context is pinned to 96 dpi (see CreateLayout), so 1 px = 0.75 pt.
					builder.Append(" font_size=\"").Append((int)Math.Round(size * 0.75 * PangoScale)).Append('"');
			}

			if (attributes.GetBold())
				builder.Append(" font_weight=\"bold\"");
			if (attributes.GetItalic())
				builder.Append(" font_style=\"italic\"");
			if (attributes.GetUnderline())
				builder.Append(" underline=\"single\"");
			if (attributes.GetStrikethrough())
				builder.Append(" strikethrough=\"true\"");

			var color = GetColor(attributes, TextAttribute.Color);
			if (color != null)
				builder.Append(" foreground=\"").Append(color).Append('"');

			var background = GetColor(attributes, TextAttribute.Background);
			if (background != null)
				builder.Append(" background=\"").Append(background).Append('"');

			builder.Append('>');

			if (superscript)
			{
				builder.Append("<sup>");
				close.Append("</sup>");
			}
			else if (subscript)
			{
				builder.Append("<sub>");
				close.Append("</sub>");
			}

			Escape(builder, value, start, length);
			builder.Append(close).Append("</span>");
		}

		private static bool GetSuperscript(this Microsoft.Maui.Graphics.Text.ITextAttributes attributes) => attributes.GetBoolAttribute(TextAttribute.Superscript);

		private static bool GetSubscript(this Microsoft.Maui.Graphics.Text.ITextAttributes attributes) => attributes.GetBoolAttribute(TextAttribute.Subscript);

		private static string GetColor(Microsoft.Maui.Graphics.Text.ITextAttributes attributes, TextAttribute attribute)
		{
			if (!attributes.TryGetValue(attribute, out var text) || string.IsNullOrEmpty(text))
				return null;

			Color color;
			try
			{
				color = Color.FromArgb(text);
			}
			catch (FormatException)
			{
				// Not a hex color: ignore the attribute rather than failing the whole text.
				return null;
			}

			if (color == null)
				return null;

			// Pango accepts #RRGGBBAA.
			return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}",
				ToByte(color.Red), ToByte(color.Green), ToByte(color.Blue), ToByte(color.Alpha));
		}

		private static int ToByte(float value) => (int)Math.Round(Math.Min(Math.Max(value, 0f), 1f) * 255f);

		private static void Escape(StringBuilder builder, string value, int start, int length)
		{
			for (var i = start; i < start + length; i++)
			{
				var c = value[i];
				switch (c)
				{
					case '&':
						builder.Append("&amp;");
						break;
					case '<':
						builder.Append("&lt;");
						break;
					case '>':
						builder.Append("&gt;");
						break;
					default:
						builder.Append(c);
						break;
				}
			}
		}

		private static string EscapeAttribute(string value)
		{
			var builder = new StringBuilder(value.Length + 8);
			foreach (var c in value)
			{
				switch (c)
				{
					case '&':
						builder.Append("&amp;");
						break;
					case '<':
						builder.Append("&lt;");
						break;
					case '>':
						builder.Append("&gt;");
						break;
					case '"':
						builder.Append("&quot;");
						break;
					default:
						builder.Append(c);
						break;
				}
			}

			return builder.ToString();
		}
	}
}
