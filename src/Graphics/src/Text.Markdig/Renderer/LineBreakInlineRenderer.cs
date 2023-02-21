using Markdig.Syntax.Inlines;

namespace Microsoft.Maui.Graphics.Text.Renderer
{
	public class LineBreakInlineRenderer : AttributedTextObjectRenderer<LineBreakInline>
	{
		protected override void Write(AttributedTextRenderer renderer, LineBreakInline obj)
		{
			renderer.WriteLine();
		}
	}
}
