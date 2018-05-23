using System;
using System.Text;
using EColor = ElmSharp.Color;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Represent a text with attributes applied.
	/// </summary>
	public class Span
	{
		static EColor s_defaultLineColor = EColor.Black;
		string _text;

		/// <summary>
		/// Gets or sets the formatted text.
		/// </summary>
		public FormattedString FormattedText { get; set; }

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <remarks>
		/// Setting Text to a non-null value will set the FormattedText property to null.
		/// </remarks>
		public string Text
		{
			get
			{
				if (FormattedText != null)
				{
					return FormattedText.ToString();
				}
				else
				{
					return _text;
				}
			}
			set
			{
				if (value == null)
				{
					value = "";
				}
				else
				{
					FormattedText = null;
				}
				_text = value;
			}
		}

		/// <summary>
		/// Gets or sets the color for the text.
		/// </summary>
		public EColor ForegroundColor { get; set; }

		/// <summary>
		/// Gets or sets the background color for the text.
		/// </summary>
		public EColor BackgroundColor { get; set; }

		/// <summary>
		/// Gets or sets the font family for the text.
		/// </summary>
		public string FontFamily { get; set; }

		/// <summary>
		/// Gets or sets the font attributes for the text.
		/// See <see cref="FontAttributes"/> for information about FontAttributes.
		/// </summary>
		public FontAttributes FontAttributes { get; set; }

		/// <summary>
		/// Gets or sets the font size for the text.
		/// </summary>
		public double FontSize { get; set; }

		/// <summary>
		/// Gets or sets the font weight for the text.
		/// </summary>
		public string FontWeight { get; set; }

		/// <summary>
		/// Gets or sets the line height.
		/// </summary>
		public double LineHeight { get; set; }

		/// <summary>
		/// Gets or sets the line break mode for the text.
		/// See <see cref="LineBreakMode"/> for information about LineBreakMode.
		/// </summary>
		public LineBreakMode LineBreakMode { get; set; }

		/// <summary>
		/// Gets or sets the horizontal alignment mode for the text.
		/// See <see cref="TextAlignment"/> for information about TextAlignment.
		/// </summary>
		public TextAlignment HorizontalTextAlignment { get; set; }

		/// <summary>
		/// Gets or sets the value that indicates whether the text has underline.
		/// </summary>
		public bool Underline { get; set; }

		/// <summary>
		/// Gets or sets the value that indicates whether the text has strike line though it.
		/// </summary>
		public bool Strikethrough { get; set; }

		/// <summary>
		/// Create a new Span instance with default attributes.
		/// </summary>
		public Span()
		{
			Text = "";
			FontFamily = "";
			FontSize = -1;
			FontWeight = Specific.FontWeight.None;
			FontAttributes = FontAttributes.None;
			ForegroundColor = EColor.Default;
			BackgroundColor = EColor.Default;
			HorizontalTextAlignment = TextAlignment.None;
			LineBreakMode = LineBreakMode.None;
			Underline = false;
			Strikethrough = false;
			LineHeight = -1.0d;
		}

		/// <summary>
		/// This method return marked up text
		/// </summary>
		internal string GetMarkupText()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("<span ");

			sb = PrepareFormattingString(sb);

			sb.Append(">");

			sb.Append(GetDecoratedText());

			sb.Append("</span>");

			return sb.ToString();
		}

		/// <summary>
		/// This method return text decorated with markup if FormattedText is set or plain text otherwise.
		/// </summary>
		public string GetDecoratedText()
		{
			if (FormattedText != null)
			{
				return FormattedText.ToMarkupString();
			}
			else
			{
				return ConvertTags(Text);
			}
		}

		StringBuilder PrepareFormattingString(StringBuilder _formattingString)
		{
			if (!ForegroundColor.IsDefault)
			{
				_formattingString.AppendFormat("color={0} ", ForegroundColor.ToHex());
			}

			if (!BackgroundColor.IsDefault)
			{
				_formattingString.AppendFormat("backing_color={0} backing=on ", BackgroundColor.ToHex());
			}

			if (!string.IsNullOrEmpty(FontFamily))
			{
				_formattingString.AppendFormat("font={0} ", FontFamily);
			}

			if (FontSize != -1)
			{
				_formattingString.AppendFormat("font_size={0} ", Forms.ConvertToEflFontPoint(FontSize));
			}

			if ((FontAttributes & FontAttributes.Bold) != 0)
			{
				_formattingString.Append("font_weight=Bold ");
			}
			else
			{
				// FontWeight is only available in case of FontAttributes.Bold is not used.
				if (FontWeight != Specific.FontWeight.None)
				{
					_formattingString.AppendFormat("font_weight={0} ", FontWeight);
				}
			}

			if ((FontAttributes & FontAttributes.Italic) != 0)
			{
				_formattingString.Append("font_style=italic ");
			}

			if (Underline)
			{
				_formattingString.AppendFormat("underline=on underline_color={0} ",
					ForegroundColor.IsDefault ? s_defaultLineColor.ToHex() : ForegroundColor.ToHex());
			}

			if (Strikethrough)
			{
				_formattingString.AppendFormat("strikethrough=on strikethrough_color={0} ",
					ForegroundColor.IsDefault ? s_defaultLineColor.ToHex() : ForegroundColor.ToHex());
			}

			switch (HorizontalTextAlignment)
			{
				case TextAlignment.Auto:
					_formattingString.Append("align=auto ");
					break;

				case TextAlignment.Start:
					_formattingString.Append("align=left ");
					break;

				case TextAlignment.End:
					_formattingString.Append("align=right ");
					break;

				case TextAlignment.Center:
					_formattingString.Append("align=center ");
					break;

				case TextAlignment.None:
					break;
			}

			if (LineHeight != -1.0d)
			{
				_formattingString.Append($"linerelsize={(int)(LineHeight*100)}%");
			}

			switch (LineBreakMode)
			{
				case LineBreakMode.HeadTruncation:
					_formattingString.Append("ellipsis=0.0");
					break;

				case LineBreakMode.MiddleTruncation:
					_formattingString.Append("ellipsis=0.5");
					break;

				case LineBreakMode.TailTruncation:
					_formattingString.Append("ellipsis=1.0");
					break;
				case LineBreakMode.None:
					break;
			}

			return _formattingString;
		}

		string ConvertTags(string text)
		{
			return text.Replace("&", "&amp;")
					   .Replace("<", "&lt;")
					   .Replace(">", "&gt;")
					   .Replace(Environment.NewLine, "<br>");
		}

		public string GetStyle()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("DEFAULT='");

			PrepareFormattingString(sb);

			sb.Append("'");

			return sb.ToString();
		}

		/// <summary>
		/// Converts string value to Span.
		/// </summary>
		/// <param name="text">The string text</param>
		public static implicit operator Span(string text)
		{
			return new Span { Text = text };
		}
	}
}