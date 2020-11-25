using System.Graphics.Text.Renderer;
using Markdig;

namespace System.Graphics.Text
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