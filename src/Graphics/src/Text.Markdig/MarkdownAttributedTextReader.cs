using Markdig;
using Microsoft.Maui.Graphics.Text.Renderer;

namespace Microsoft.Maui.Graphics.Text
{
	/// <summary>
	/// Provides functionality to convert markdown text into attributed text.
	/// </summary>
	public class MarkdownAttributedTextReader
	{
		/// <summary>
		/// Converts the specified markdown text to attributed text.
		/// </summary>
		/// <param name="text">The markdown text to convert.</param>
		/// <returns>An <see cref="IAttributedText"/> representation of the markdown.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
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
