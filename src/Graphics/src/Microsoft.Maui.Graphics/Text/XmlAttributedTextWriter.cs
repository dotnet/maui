using System.Globalization;
using System.IO;
using System.Text;
using XmlNames = Microsoft.Maui.Graphics.Text.XmlAttributedTextNames;

namespace Microsoft.Maui.Graphics.Text
{
    public class XmlAttributedTextWriter
    {
        public string Write(IAttributedText text)
        {
            using (var writer = new StringWriter())
            {
                Write(text, writer);
                return writer.ToString();
            }
        }

        public void Write(
            IAttributedText attributedText,
            TextWriter writer)
        {
            if (attributedText != null && !string.IsNullOrEmpty(attributedText.Text))
            {
                bool encode = attributedText.Text.Contains("]]");

                writer.Write($"<{XmlNames.AttributedText}>");
                if (encode)
                {
                    writer.Write($"<{XmlNames.Content} {XmlNames.Encoded}=\"True\"><![CDATA[");
                    byte[] bytes = Encoding.UTF8.GetBytes(attributedText.Text);
                    writer.Write(Convert.ToBase64String(bytes));
                    writer.Write($"]]></{XmlNames.Content}>");
                }
                else
                {
                    writer.Write($"<{XmlNames.Content}><![CDATA[");
                    writer.Write(attributedText.Text);
                    writer.Write($"]]></{XmlNames.Content}>");
                }

                if (attributedText.Runs != null && attributedText.Runs.Count > 0)
                {
                    foreach (var run in attributedText.Runs)
                        WriteRun(run, writer);
                }

                writer.Write($"</{XmlNames.AttributedText}>");
            }
        }

        private void WriteRun(
            IAttributedTextRun run,
            TextWriter writer)
        {
            if (run.Attributes != null)
            {
                var attributes = run.Attributes;

                writer.Write($"<{XmlNames.Run}");

                writer.Write($" {XmlNames.Start}=\"");
                writer.Write(run.Start.ToString(CultureInfo.InvariantCulture));
                writer.Write("\"");

                writer.Write($" {XmlNames.Length}=\"");
                writer.Write(run.Length.ToString(CultureInfo.InvariantCulture));
                writer.Write("\"");

                foreach (var entry in run.Attributes)
                    Write(attributes, entry.Key, null, writer);

                writer.Write(" />");
            }
        }

        private void Write(
            ITextAttributes currentAttributes,
            TextAttribute key,
            string defaultValue,
            TextWriter writer)
        {
            currentAttributes.TryGetValue(key, out var value);

            if (!string.Equals(value, defaultValue))
                WriteAttribute(writer, key.ToString(), value);
        }

        private void WriteAttribute(TextWriter writer, string attribute, string value)
        {
            if (value != null)
            {
                writer.Write(" ");
                writer.Write(attribute);
                writer.Write("=\"");
                writer.Write(value);
                writer.Write("\"");
            }
        }
    }
}