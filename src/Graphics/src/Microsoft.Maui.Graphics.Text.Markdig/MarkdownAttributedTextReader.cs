using Microsoft.Maui.Graphics.Text.Renderer;
using Markdig;

namespace Microsoft.Maui.Graphics.Text
{
    public class MarkdownAttributedTextReader
    {
        public static IAttributedText Read(string text)
        {
            var renderer = new AttributedTextRenderer();
            var builder = new MarkdownPipelineBuilder().UseEmphasisExtras();
            var pipeline = builder.Build();
            Markdown.Convert(text, renderer, pipeline);
            return renderer.GetAttributedText();
        }
    }
}