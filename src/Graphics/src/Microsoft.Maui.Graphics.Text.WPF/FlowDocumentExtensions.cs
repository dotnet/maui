using System.Collections.Generic;
using Microsoft.Maui.Graphics.Text.Immutable;
using System.IO;
using System.Windows.Documents;

namespace Microsoft.Maui.Graphics.Text
{
    public static class FlowDocumentExtensions
    {
        public static AttributedText AsAttributedText(
            this FlowDocument flowDocument)
        {
            if (flowDocument == null)
                return null;

            using (var textWriter = new StringWriter())
            {
                var runs = CreateRuns(flowDocument, textWriter);
                return new AttributedText(textWriter.ToString(), runs);
            }
        }

        private static List<AttributedTextRun> CreateRuns(
            FlowDocument target,
            TextWriter writer)
        {
            var runs = new List<AttributedTextRun>();

            foreach (var block in target.Blocks)
            {
                if (block is Paragraph paragraph)
                {
                    foreach (var inline in paragraph.Inlines)
                    {
                        if (inline is Run run)
                        {
                            writer.Write(run.Text);
                        }
                        else
                        {
                            Logger.Warn($"Inline of type {inline.GetType().Name} not currently supported: {inline}");
                        }
                    }

                    writer.WriteLine("\n");
                }
                else
                {
                    Logger.Warn($"Block of type {block.GetType().Name} not currently supported: {block}");
                }
            }

            return runs;
        }
    }
}
