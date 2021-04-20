using Markdig.Syntax;

namespace Microsoft.Maui.Graphics.Text.Renderer
{
	public class HtmlBlockRenderer : AttributedTextObjectRenderer<HtmlBlock>
	{
		protected override void Write(AttributedTextRenderer renderer, HtmlBlock leafBlock)
		{
			bool writeEndOfLines = true;

			if (leafBlock.Lines.Lines != null)
			{
				var lines = leafBlock.Lines;
				var slices = lines.Lines;
				for (int i = 0; i < lines.Count; i++)
				{
					if (!writeEndOfLines && i > 0)
					{
						renderer.WriteLine();
					}

					renderer.Write(ref slices[i].Slice);

					if (writeEndOfLines)
					{
						renderer.WriteLine();
					}
				}
			}
		}
	}
}
