using Microsoft.Maui.Graphics.Text;
using Markdig.Syntax;

namespace Microsoft.Maui.Graphics.Text.Renderer
{
	public class ListRenderer : AttributedTextObjectRenderer<ListBlock>
	{
		protected override void Write(
			AttributedTextRenderer renderer,
			ListBlock listBlock)
		{
			var start = renderer.Count;

			renderer.EnsureLine();

			foreach (var item in listBlock)
			{
				var listItem = (ListItemBlock) item;

				renderer.EnsureLine();
				renderer.Write("• ");
				renderer.WriteChildren(listItem);
			}

			var length = renderer.Count - start;
			if (length > 0)
			{
				var attributes = new TextAttributes();
				if (!listBlock.IsOrdered) attributes[TextAttribute.UnorderedList] = "True";
				renderer.AddTextRun(start, length, attributes);
			}
		}
	}
}
