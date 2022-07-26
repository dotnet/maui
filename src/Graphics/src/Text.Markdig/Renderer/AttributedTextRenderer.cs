using System.Collections.Generic;
using Microsoft.Maui.Graphics.Text;
using System.IO;
using Markdig.Renderers;

namespace Microsoft.Maui.Graphics.Text.Renderer
{
	public class AttributedTextRenderer : TextRendererBase<AttributedTextRenderer>
	{
		private List<IAttributedTextRun> _runs = new List<IAttributedTextRun>();

		public AttributedTextRenderer() : base(new CountingWriter(new StringWriter()))
		{
			// Default block renderers
			//ObjectRenderers.Add(new CodeBlockRenderer());
			ObjectRenderers.Add(new ListRenderer());
			//ObjectRenderers.Add(new HeadingRenderer());
			//ObjectRenderers.Add(new HtmlBlockRenderer());
			ObjectRenderers.Add(new ParagraphRenderer());
			//ObjectRenderers.Add(new QuoteBlockRenderer());
			//ObjectRenderers.Add(new ThematicBreakRenderer());

			// Default inline renderers
			//ObjectRenderers.Add(new AutolinkInlineRenderer());
			//ObjectRenderers.Add(new CodeInlineRenderer());
			//ObjectRenderers.Add(new DelimiterInlineRenderer());
			ObjectRenderers.Add(new EmphasisInlineRenderer());
			ObjectRenderers.Add(new LineBreakInlineRenderer());
			ObjectRenderers.Add(new HtmlInlineRenderer());
			//ObjectRenderers.Add(new HtmlEntityInlineRenderer());
			//ObjectRenderers.Add(new LinkInlineRenderer());
			ObjectRenderers.Add(new LiteralInlineRenderer());
		}

		public IAttributedText GetAttributedText()
		{
			Writer.Flush();
			var value = Writer.ToString();
			_runs.Optimize(value.Length);
			return new AttributedText(value, _runs, true);
		}

		internal void AddTextRun(int start, int length, TextAttributes attributes)
		{
			_runs.Add(new AttributedTextRun(start, length, attributes));
		}

		public int Count => ((CountingWriter) Writer).Count;
	}
}
