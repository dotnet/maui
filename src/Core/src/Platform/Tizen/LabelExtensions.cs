using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using TLineBreakMode = Tizen.UIExtensions.Common.LineBreakMode;
using TTextDecorationse = Tizen.UIExtensions.Common.TextDecorations;

namespace Microsoft.Maui
{
	public static class LabelExtensions
	{
		public static void UpdateText(this Label nativeLabel, ILabel label)
		{
			nativeLabel.Text = label.Text ?? "";
		}

		public static void UpdateTextColor(this Label nativeLabel, ILabel label)
		{
			nativeLabel.TextColor = label.TextColor.ToNative();
		}

		public static void UpdateFont(this Label nativeLabel, ILabel label, IFontManager fontManager)
		{
			nativeLabel.BatchBegin();
			nativeLabel.FontSize = label.Font.Size > 0 ? label.Font.Size : 25.ToDPFont();
			nativeLabel.FontAttributes = label.Font.GetFontAttributes();
			nativeLabel.FontFamily = fontManager.GetFontFamily(label.Font.Family)??"";
			nativeLabel.BatchCommit();
		}

		public static void UpdateHorizontalTextAlignment(this Label nativeLabel, ILabel label)
		{
			nativeLabel.HorizontalTextAlignment = label.HorizontalTextAlignment.ToNative();
		}

		public static void UpdateVerticalTextAlignment(this Label nativeLabel, ILabel label)
		{
			nativeLabel.VerticalTextAlignment = label.VerticalTextAlignment.ToNative();
		}

		public static void UpdateLineBreakMode(this Label nativeLabel, ILabel label)
		{
			nativeLabel.LineBreakMode = label.LineBreakMode.ToNative();
		}

		public static void UpdateTextDecorations(this Label nativeLabel, ILabel label)
		{
			nativeLabel.TextDecorations = label.TextDecorations.ToNative();
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

		public static TLineBreakMode ToNative(this LineBreakMode mode)
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

		public static TTextDecorationse ToNative(this TextDecorations td)
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
