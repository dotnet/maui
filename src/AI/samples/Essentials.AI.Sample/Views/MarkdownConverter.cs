using System.Globalization;
using System.Text.RegularExpressions;

namespace Maui.Controls.Sample.Views;

/// <summary>
/// Converts a markdown string to a FormattedString with styled Spans.
/// Supports **bold**, *italic*, ***bold italic***, and `code`.
/// </summary>
public partial class MarkdownConverter : IValueConverter
{
	[GeneratedRegex(@"\*\*\*(.+?)\*\*\*|\*\*(.+?)\*\*|\*(?!\s)(.+?)(?<!\s)\*|`(.+?)`", RegexOptions.Singleline)]
	private static partial Regex MarkdownPattern();

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is not string text || string.IsNullOrEmpty(text))
			return new FormattedString();

		var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
		var textColor = isDark ? Color.FromArgb("#E1E1E1") : Color.FromArgb("#1F1F1F");
		var codeBackground = isDark ? Color.FromArgb("#3D3D3D") : Color.FromArgb("#E8E8E8");

		return Parse(text, textColor, codeBackground);
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException();

	static FormattedString Parse(string text, Color textColor, Color codeBackground)
	{
		var formatted = new FormattedString();
		var lastIndex = 0;

		foreach (Match match in MarkdownPattern().Matches(text))
		{
			if (match.Index > lastIndex)
				AddSpan(formatted, text[lastIndex..match.Index], textColor);

			if (match.Groups[1].Success) // ***bold italic***
				AddSpan(formatted, match.Groups[1].Value, textColor, FontAttributes.Bold | FontAttributes.Italic);
			else if (match.Groups[2].Success) // **bold**
				AddSpan(formatted, match.Groups[2].Value, textColor, FontAttributes.Bold);
			else if (match.Groups[3].Success) // *italic*
				AddSpan(formatted, match.Groups[3].Value, textColor, FontAttributes.Italic);
			else if (match.Groups[4].Success) // `code`
				AddSpan(formatted, match.Groups[4].Value, textColor, fontFamily: "Courier New", backgroundColor: codeBackground);

			lastIndex = match.Index + match.Length;
		}

		if (lastIndex < text.Length)
			AddSpan(formatted, text[lastIndex..], textColor);

		if (formatted.Spans.Count == 0)
			AddSpan(formatted, text, textColor);

		return formatted;
	}

	static void AddSpan(FormattedString fs, string text, Color textColor,
		FontAttributes fontAttributes = FontAttributes.None,
		string? fontFamily = null, Color? backgroundColor = null)
	{
		var span = new Span
		{
			Text = text,
			TextColor = textColor,
			FontAttributes = fontAttributes,
			FontSize = 14,
		};

		if (fontFamily is not null)
			span.FontFamily = fontFamily;
		if (backgroundColor is not null)
			span.BackgroundColor = backgroundColor;

		fs.Spans.Add(span);
	}
}
