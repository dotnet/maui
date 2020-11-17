using Markdig.Syntax.Inlines;

namespace System.Graphics.Text.Renderer
{
    public class LineBreakInlineRenderer : AttributedTextObjectRenderer<LineBreakInline>
    {
        protected override void Write(AttributedTextRenderer renderer, LineBreakInline obj)
        {
            renderer.WriteLine();
        }
    }
}