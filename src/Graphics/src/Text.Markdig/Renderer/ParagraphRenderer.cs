using Markdig.Syntax;

namespace Microsoft.Maui.Graphics.Text.Renderer
{
	/// <summary>
	/// A HTML renderer for a <see cref="ParagraphBlock"/>.
	/// </summary>
	/// <seealso cref="Markdig.Renderers.Html.HtmlObjectRenderer{ParagraphBlock}"/>
	public class ParagraphRenderer : AttributedTextObjectRenderer<ParagraphBlock>
	{
		protected override void Write(AttributedTextRenderer renderer, ParagraphBlock obj)
		{
			/*if (!renderer.ImplicitParagraph)
			{
			   if (!renderer.IsFirstInContainer)
			   {
				  renderer.EnsureLine();
			   }
			   renderer.Write("<p").WriteAttributes(obj).Write(">");
			}*/
			renderer.WriteLeafInline(obj);
			renderer.WriteLine();
			/*if (!renderer.ImplicitParagraph)
			{
			   renderer.WriteLine("</p>");
			}*/
		}
	}
}
