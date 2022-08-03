using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using TLineBreakMode = Tizen.UIExtensions.Common.LineBreakMode;
using TTextDecorationse = Tizen.UIExtensions.Common.TextDecorations;

namespace Microsoft.Maui.Platform
{
	public static class LabelExtensions
	{
		public static void UpdateText(this Label platformLabel, ILabel label)
		{
			platformLabel.Text = label.Text ?? "";
		}

		public static void UpdateTextColor(this Label platformLabel, ILabel label)
		{
			platformLabel.TextColor = label.TextColor.ToPlatform();
		}

		public static void UpdateFont(this Label platformLabel, ILabel label, IFontManager fontManager)
		{
			platformLabel.BatchBegin();
			platformLabel.FontSize = label.Font.Size > 0 ? label.Font.Size : 25.ToDPFont();
			platformLabel.FontAttributes = label.Font.GetFontAttributes();
			platformLabel.FontFamily = fontManager.GetFontFamily(label.Font.Family) ?? "";
			platformLabel.BatchCommit();
		}

		public static void UpdateHorizontalTextAlignment(this Label platformLabel, ILabel label)
		{
			platformLabel.HorizontalTextAlignment = label.HorizontalTextAlignment.ToPlatform();
		}

		public static void UpdateVerticalTextAlignment(this Label platformLabel, ILabel label)
		{
			platformLabel.VerticalTextAlignment = label.VerticalTextAlignment.ToPlatform();
		}

		public static void UpdateTextDecorations(this Label platformLabel, ILabel label)
		{
			platformLabel.TextDecorations = label.TextDecorations.ToPlatform();
		}

		public static FontAttributes GetFontAttributes(this Font font)
		{
			FontAttributes attributes = font.Weight == FontWeight.Bold ? FontAttributes.Bold : FontAttributes.None;
			if (font.Slant != FontSlant.Default)
			{
				if (attributes == FontAttributes.None)
					attributes = FontAttributes.Italic;
				else
					attributes = attributes | FontAttributes.Italic;
			}
			return attributes;
		}

		public static TLineBreakMode ToPlatform(this LineBreakMode mode)
		{
			switch (mode)
			{
				case LineBreakMode.CharacterWrap:
					return TLineBreakMode.CharacterWrap;
				case LineBreakMode.HeadTruncation:
					return TLineBreakMode.HeadTruncation;
				case LineBreakMode.MiddleTruncation:
					return TLineBreakMode.MiddleTruncation;
				case LineBreakMode.NoWrap:
					return TLineBreakMode.NoWrap;
				case LineBreakMode.TailTruncation:
					return TLineBreakMode.TailTruncation;
				case LineBreakMode.WordWrap:
				default:
					return TLineBreakMode.WordWrap;
			}
		}

		public static TTextDecorationse ToPlatform(this TextDecorations td)
		{
			if (td == TextDecorations.Strikethrough)
				return TTextDecorationse.Strikethrough;
			else if (td == TextDecorations.Underline)
				return TTextDecorationse.Underline;
			else
				return TTextDecorationse.None;
		}

	}
}
