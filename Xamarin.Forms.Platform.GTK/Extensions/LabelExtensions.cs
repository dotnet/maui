using Pango;
using System.Text;
using Xamarin.Forms.Platform.GTK.Helpers;

namespace Xamarin.Forms.Platform.GTK.Extensions
{
    internal static class LabelExtensions
    {
        internal static void SetTextFromFormatted(this Gtk.Label self, FormattedString formatted)
        {
            string markupText = GenerateMarkupText(formatted);

            if (self != null)
            {
                self.Markup = markupText;
            }
        }

        internal static void SetTextFromSpan(this Gtk.Label self, Span span)
        {
            string markupText = GenerateMarkupText(span);

            if (self != null)
            {
                self.Markup = markupText;
            }
        }

        private static string GenerateMarkupText(FormattedString formatted)
        {
            StringBuilder builder = new StringBuilder();

            foreach (Span span in formatted.Spans)
            {
                builder.Append(GenerateMarkupText(span));
            }

            return builder.ToString();
        }

        private static string GenerateMarkupText(Span span)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<span ");

            FontDescription fontDescription = FontDescriptionHelper.CreateFontDescription(
                span.FontSize, span.FontFamily, span.FontAttributes);

            builder.AppendFormat(" font=\"{0}\"", fontDescription.ToString());

            // BackgroundColor => 
            if (!span.BackgroundColor.IsDefault)
            {
                builder.AppendFormat(" bgcolor=\"{0}\"", span.BackgroundColor.ToRgbaColor());
            }

            // ForegroundColor => 
            if (!span.ForegroundColor.IsDefault)
            {
                builder.AppendFormat(" fgcolor=\"{0}\"", span.ForegroundColor.ToRgbaColor());
            }

            builder.Append(">"); // Complete opening span tag

            // Text
            builder.Append(span.Text);
            builder.Append("</span>");

            return builder.ToString();
        }
    }
}