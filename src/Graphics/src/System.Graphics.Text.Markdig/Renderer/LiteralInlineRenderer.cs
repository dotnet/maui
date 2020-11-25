using Markdig.Syntax.Inlines;

namespace System.Graphics.Text.Renderer
{
    public class LiteralInlineRenderer : AttributedTextObjectRenderer<LiteralInline>
    {
        protected override void Write(AttributedTextRenderer renderer, LiteralInline obj)
        {
            renderer.Write(ref obj.Content);
        }
    }
}