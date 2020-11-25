using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

// ReSharper disable UnusedMember.Global

namespace System.Graphics.Text
{
    public static class AttributedTextExtensions
    {
        public static FlowDocument AsFlowDocument(
            this IAttributedText target)
        {
            if (target == null)
                return null;

            var text = target.Text ?? "";
            var paragraph = new Paragraph(new Run(text));

            if (!string.IsNullOrWhiteSpace(text) && target.Runs != null)
            {
                foreach (var textRun in target.Runs)
                {
                    var pointer1 = paragraph.ContentStart.GetPositionAtOffset(textRun.Start + 1);
                    var pointer2 = paragraph.ContentStart.GetPositionAtOffset(textRun.GetEnd() + 1);

                    if (pointer1 != null && pointer2 != null)
                    {
                        var span = new Span(pointer1, pointer2);
                        ApplyFormattingToSpan(span, textRun);
                    }
                }
            }

            return new FlowDocument(paragraph);
        }

        private static void ApplyFormattingToSpan(Span span, IAttributedTextRun textRun)
        {
            var attributes = textRun.Attributes;

            var fontName = attributes.GetFontName();
            if (fontName != null)
                span.FontFamily = new Windows.Media.FontFamily(fontName);

            if (attributes.GetBold())
                span.FontWeight = FontWeights.Bold;

            if (attributes.GetItalic())
                span.FontStyle = FontStyles.Italic;

            if (attributes.GetUnderline())
                span.TextDecorations.Add(TextDecorations.Underline);

            var foregroundColor = attributes.GetForegroundColor()?.ParseAsInts()?.ToColor();
            if (foregroundColor != null)
            {
                var brush = new SolidColorBrush((Windows.Media.Color) foregroundColor);
                span.Foreground = brush;
            }

            var backgroundColor = attributes.GetBackgroundColor()?.ParseAsInts()?.ToColor();
            if (backgroundColor != null)
            {
                var brush = new SolidColorBrush((Windows.Media.Color) backgroundColor);
                span.Background = brush;
            }

            if (attributes.GetSubscript())
                span.BaselineAlignment = BaselineAlignment.Subscript;

            if (attributes.GetSuperscript())
                span.BaselineAlignment = BaselineAlignment.Superscript;

            if (attributes.GetStrikethrough())
                span.TextDecorations.Add(TextDecorations.Strikethrough);

            /*if (attributes.GetUnorderedList())
            {
                var bulletSpan = new BulletSpan();
                spannableString.SetSpan(bulletSpan, start, end, SpanTypes.ExclusiveExclusive);
            }*/
        }
    }
}