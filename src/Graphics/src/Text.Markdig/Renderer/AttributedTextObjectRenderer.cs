using Markdig.Renderers;
using Markdig.Syntax;

namespace Microsoft.Maui.Graphics.Text.Renderer
{
	public abstract class AttributedTextObjectRenderer<T>
		: MarkdownObjectRenderer<AttributedTextRenderer, T> where T : MarkdownObject
	{
	}
}
