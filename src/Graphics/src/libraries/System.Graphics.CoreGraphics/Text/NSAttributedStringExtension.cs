#if __MACOS__
using AppKit;
#else
using NSStringAttributeKey = UIKit.UIStringAttributeKey;
using NSFont = UIKit.UIFont;
using NSColor = UIKit.UIColor;
#endif
using System.Collections.Generic;
using System.Graphics.Text;
using System.Graphics.Text.Immutable;
using System.IO;
using Foundation;

namespace System.Graphics.CoreGraphics
{
    public static class NSAttributedStringExtension
    {
        public static IAttributedText AsAttributedText(this NSAttributedString target)
        {
            if (target != null)
            {
                using (var textWriter = new StringWriter())
                {
                    var runs = CreateRuns(target, textWriter);
                    return new AttributedText(textWriter.ToString(), runs);
                }
            }

            return null;
        }

        private static List<IAttributedTextRun> CreateRuns(
            NSAttributedString target,
            TextWriter writer)
        {
            var runs = new List<IAttributedTextRun>();

            target.EnumerateAttributes(new NSRange(0, target.Length), NSAttributedStringEnumeration.None,
                (NSDictionary attrs, NSRange range, ref bool stop) => { stop = HandleAttributes(runs, writer, target, attrs, range); });

            return runs;
        }

        private static bool HandleAttributes(
            IList<IAttributedTextRun> runs,
            TextWriter writer,
            NSAttributedString target,
            NSDictionary attrs,
            NSRange range)
        {
            var text = target.Substring(range.Location, range.Length).Value;

            var formatAttributes = new TextAttributes();
            var run = new AttributedTextRun((int) range.Location, (int) range.Length, formatAttributes);

            NSObject font;
            if (attrs.TryGetValue(NSStringAttributeKey.Font, out font))
            {
                var actualFont = (NSFont) font;
#if __MACOS__
				var fontName = actualFont.FontName;
#else
                var fontName = actualFont.Name;
#endif

                formatAttributes.SetFontSize((float) actualFont.PointSize);
                if (!fontName.StartsWith(".", System.StringComparison.Ordinal))
                    formatAttributes.SetFontName(fontName);
                else
                {
                    if (fontName.Contains("Italic"))
                        formatAttributes.SetItalic(true);

                    if (fontName.Contains("Bold"))
                        formatAttributes.SetBold(true);
                }
            }

            NSObject underline;
            if (attrs.TryGetValue(NSStringAttributeKey.UnderlineStyle, out underline))
            {
                var number = underline as NSNumber;
                if (number != null && number.Int32Value > 0)
                    formatAttributes.SetUnderline(true);
            }

            NSObject strikethrough;
            if (attrs.TryGetValue(NSStringAttributeKey.StrikethroughStyle, out strikethrough))
            {
                var number = strikethrough as NSNumber;
                if (number != null && number.Int32Value > 0)
                    formatAttributes.SetStrikethrough(true);
            }

#if MONOMAC
			NSObject superscript;
			if (attrs.TryGetValue (NSStringAttributeKey.Superscript, out superscript))
			{
				var number = superscript as NSNumber;
				if (number != null && number.Int32Value == -1)
					formatAttributes.SetSubscript(true);
				else if (number != null && number.Int32Value == 1)
					formatAttributes.SetSuperscript(true);
			}
#endif

            NSObject color;
            if (attrs.TryGetValue(NSStringAttributeKey.ForegroundColor, out color))
            {
                var colorObject = color as NSColor;
                if (colorObject != null)
                    formatAttributes.SetForegroundColor(colorObject.ToHex());
            }

            NSObject backgroundColor;
            if (attrs.TryGetValue(NSStringAttributeKey.BackgroundColor, out backgroundColor))
            {
                var colorObject = backgroundColor as NSColor;
                if (colorObject != null)
                    formatAttributes.SetBackgroundColor(colorObject.ToHex());
            }

#if MONOMAC
			NSObject paragraphStyleAsObject;
			if (attrs.TryGetValue (NSStringAttributeKey.ParagraphStyle, out paragraphStyleAsObject))
			{
				var paragraphStyle = (NSParagraphStyle)paragraphStyleAsObject;
				if (paragraphStyle.TextLists != null && paragraphStyle.TextLists.Length > 0)
				{
					for (int i = 0; i<paragraphStyle.TextLists.Length; i++)
					{
						var textList = paragraphStyle.TextLists [i];
						var markerFormat = textList.MarkerFormat;

						if ("{hyphen}".Equals (textList.MarkerFormat))
						{
							formatAttributes.SetUnorderedList(true);
							formatAttributes.SetMarker(MarkerType.Hyphen);
						}
						else
						{
							formatAttributes.SetUnorderedList(true);
							formatAttributes.SetMarker(MarkerType.ClosedCircle);
						}
					}
				}
			}
#endif

            if (run.Attributes.Count > 0)
                runs.Add(run);

            writer.Write(text);
            return false;
        }
    }
}