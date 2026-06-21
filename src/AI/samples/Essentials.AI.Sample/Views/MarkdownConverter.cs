using System.Globalization;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Maui.Controls.Sample.Views;

/// <summary>
/// Converts a markdown string to a FormattedString using Markdig's AST.
/// Supports bold, italic, code, and renders everything else as plain text.
/// </summary>
public class MarkdownConverter : IValueConverter
{
	static readonly MarkdownPipeline s_pipeline = new MarkdownPipelineBuilder()
		.UseEmphasisExtras()
		.Build();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not string text || string.IsNullOrEmpty(text))
			return new FormattedString();

		var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
		var textColor = isDark ? Color.FromArgb("#E1E1E1") : Color.FromArgb("#1F1F1F");
		var codeBackground = isDark ? Color.FromArgb("#3D3D3D") : Color.FromArgb("#E8E8E8");

		var formatted = new FormattedString();
		var doc = Markdown.Parse(text, s_pipeline);

		foreach (var block in doc)
		{
			if (formatted.Spans.Count > 0)
				formatted.Spans.Add(new Span { Text = "\n", TextColor = textColor, FontSize = 14 });

			if (block is ParagraphBlock paragraph && paragraph.Inline is not null)
			{
				WalkInlines(paragraph.Inline, formatted, textColor, codeBackground, FontAttributes.None);
			}
			else if (block is HeadingBlock heading && heading.Inline is not null)
			{
				WalkInlines(heading.Inline, formatted, textColor, codeBackground, FontAttributes.Bold);
			}
			else if (block is ListBlock list)
			{
				int index = 1;
				foreach (var item in list)
				{
					if (formatted.Spans.Count > 0)
						formatted.Spans.Add(new Span { Text = "\n", TextColor = textColor, FontSize = 14 });

					var bullet = list.IsOrdered ? $"{index++}. " : "• ";
					formatted.Spans.Add(new Span { Text = bullet, TextColor = textColor, FontSize = 14 });

					if (item is ListItemBlock listItem)
					{
						foreach (var subBlock in listItem)
						{
							if (subBlock is ParagraphBlock p && p.Inline is not null)
								WalkInlines(p.Inline, formatted, textColor, codeBackground, FontAttributes.None);
						}
					}
				}
			}
			else
			{
				// Fallback: render block as plain text
				var start = Math.Min(block.Span.Start, text.Length);
				var length = Math.Min(block.Span.Length, text.Length - start);
				var plainText = block.GetType().Name == "FencedCodeBlock" || block.GetType().Name == "CodeBlock"
					? GetCodeBlockText(block)
					: text.Substring(start, length);
				formatted.Spans.Add(new Span
				{
					Text = plainText,
					TextColor = textColor,
					FontFamily = "Courier New",
					BackgroundColor = codeBackground,
					FontSize = 13,
				});
			}
		}

		if (formatted.Spans.Count == 0)
			formatted.Spans.Add(new Span { Text = text, TextColor = textColor, FontSize = 14 });

		return formatted;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();

	static void WalkInlines(ContainerInline container, FormattedString fs, Color textColor, Color codeBackground, FontAttributes inherited)
	{
		foreach (var inline in container)
		{
			switch (inline)
			{
				case EmphasisInline emphasis:
					var attrs = inherited;
					if (emphasis.DelimiterCount == 2 || (emphasis.DelimiterChar == '*' && emphasis.DelimiterCount >= 2))
						attrs |= FontAttributes.Bold;
					else
						attrs |= FontAttributes.Italic;
					WalkInlines(emphasis, fs, textColor, codeBackground, attrs);
					break;

				case CodeInline code:
					fs.Spans.Add(new Span
					{
						Text = code.Content,
						TextColor = textColor,
						FontFamily = "Courier New",
						BackgroundColor = codeBackground,
						FontSize = 13,
					});
					break;

				case LinkInline link:
					// Render link text with underline
					var linkText = link.FirstChild is LiteralInline lit ? lit.Content.ToString() : link.Url ?? "";
					fs.Spans.Add(new Span
					{
						Text = linkText,
						TextColor = Color.FromArgb("#512BD4"),
						TextDecorations = TextDecorations.Underline,
						FontAttributes = inherited,
						FontSize = 14,
					});
					break;

				case LineBreakInline:
					fs.Spans.Add(new Span { Text = "\n", TextColor = textColor, FontSize = 14 });
					break;

				case LiteralInline literal:
					fs.Spans.Add(new Span
					{
						Text = literal.Content.ToString(),
						TextColor = textColor,
						FontAttributes = inherited,
						FontSize = 14,
					});
					break;

				default:
					// Any other inline — render as plain text
					if (inline is ContainerInline ci)
						WalkInlines(ci, fs, textColor, codeBackground, inherited);
					break;
			}
		}
	}

	static string GetCodeBlockText(Block block)
	{
		if (block is FencedCodeBlock fenced)
			return string.Join("\n", fenced.Lines);
		if (block is CodeBlock code)
			return string.Join("\n", code.Lines);
		return block.Span.ToString() ?? "";
	}
}
