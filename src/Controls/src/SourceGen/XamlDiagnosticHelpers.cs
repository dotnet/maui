using System;
using System.Text.RegularExpressions;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

static class XamlDiagnosticHelpers
{
	static readonly SourceInfo EmptySourceInfo = new(null, null, 0, 0, null);

	public static Diagnostic CreateXamlParserDiagnostic(ProjectItem? projectItem, SourceText? sourceText, Exception exception)
	{
		var (lineInfo, errorMessage) = GetExceptionInfo(exception);
		var sourceInfo = GetSourceInfo(projectItem, sourceText, lineInfo);
		var location = sourceInfo.Location;
		var message = FormatMessage(errorMessage, sourceInfo);

		return Diagnostic.Create(Descriptors.XamlParserError, location, message);
	}

	static (IXmlLineInfo lineInfo, string errorMessage) GetExceptionInfo(Exception exception)
	{
		if (exception is XamlParseException xpe)
			return (xpe.XmlInfo, xpe.UnformattedMessage);

		if (exception is XmlException xmlEx)
			return (new XmlLineInfo(xmlEx.LineNumber, xmlEx.LinePosition), StripLineInfoFromXmlExceptionMessage(xmlEx.Message));

		if (exception.InnerException is XmlException innerXmlEx)
			return (new XmlLineInfo(innerXmlEx.LineNumber, innerXmlEx.LinePosition), StripLineInfoFromXmlExceptionMessage(innerXmlEx.Message));

		return (ExtractLineInfoFromMessage(exception.Message), StripLineInfoFromXmlExceptionMessage(exception.Message));
	}

	static SourceInfo GetSourceInfo(ProjectItem? projectItem, SourceText? sourceText, IXmlLineInfo lineInfo)
	{
		var filePath = projectItem?.AdditionalText.Path ?? projectItem?.RelativePath;
		if (filePath is null)
			return EmptySourceInfo;

		if (sourceText is null)
			return new SourceInfo(Location.Create(filePath, new TextSpan(), new LinePositionSpan()), filePath, lineInfo.LineNumber, lineInfo.LinePosition, null);

		if (TryGetLineInfoLocation(filePath, sourceText, lineInfo, out var location, out var lineNumber, out var columnNumber, out var excerpt))
			return new SourceInfo(location, filePath, lineNumber, columnNumber, excerpt);

		if (TryGetMalformedMarkupExtensionLocation(filePath, sourceText, out location, out lineNumber, out columnNumber, out excerpt))
			return new SourceInfo(location, filePath, lineNumber, columnNumber, excerpt);

		return new SourceInfo(Location.Create(filePath, new TextSpan(), new LinePositionSpan()), filePath, 0, 0, null);
	}

	static bool TryGetLineInfoLocation(string filePath, SourceText sourceText, IXmlLineInfo lineInfo, out Location location, out int lineNumber, out int columnNumber, out string? excerpt)
	{
		location = Location.None;
		lineNumber = lineInfo.LineNumber;
		columnNumber = lineInfo.LinePosition;
		excerpt = null;

		if ((uint)(lineNumber - 1) >= (uint)sourceText.Lines.Count)
			return false;

		var lineIndex = lineNumber - 1;
		var line = sourceText.Lines[lineIndex];
		var lineText = sourceText.ToString(line.Span);
		var columnIndex = Math.Max(0, Math.Min(columnNumber - 1, lineText.Length));
		var excerptSpan = GetBestLineExcerpt(lineText, columnIndex);
		excerpt = excerptSpan.length > 0 ? lineText.Substring(excerptSpan.start, excerptSpan.length).Trim() : lineText.Trim();
		if (string.IsNullOrWhiteSpace(excerpt))
			excerpt = null;

		var startColumn = Math.Max(0, Math.Min(excerptSpan.start, lineText.Length));
		var length = Math.Max(1, Math.Min(Math.Max(excerptSpan.length, 1), Math.Max(1, lineText.Length - startColumn)));
		location = CreateLocation(filePath, sourceText, lineIndex, startColumn, length);
		columnNumber = startColumn + 1;
		return true;
	}

	static bool TryGetMalformedMarkupExtensionLocation(string filePath, SourceText sourceText, out Location location, out int lineNumber, out int columnNumber, out string? excerpt)
	{
		location = Location.None;
		lineNumber = 0;
		columnNumber = 0;
		excerpt = null;

		for (var i = 0; i < sourceText.Lines.Count; i++)
		{
			var line = sourceText.Lines[i];
			var lineText = sourceText.ToString(line.Span);
			var match = Regex.Match(lineText, @"[\w:.-]+\s*=\s*[""']\{x:Static\}[""']");
			if (!match.Success)
				continue;

			excerpt = match.Value.Trim();
			lineNumber = i + 1;
			columnNumber = match.Index + 1;
			location = CreateLocation(filePath, sourceText, i, match.Index, Math.Max(1, match.Length));
			return true;
		}

		return false;
	}

	static (int start, int length) GetBestLineExcerpt(string lineText, int columnIndex)
	{
		foreach (Match match in Regex.Matches(lineText, @"[\w:.-]+\s*=\s*(?:""[^""]*""|'[^']*')"))
		{
			if (columnIndex >= match.Index && columnIndex <= match.Index + match.Length)
				return (match.Index, match.Length);
		}

		foreach (Match match in Regex.Matches(lineText, @"[\w:.-]+\s*=\s*[""'][^""']*\{x:Static\}[^""']*[""']"))
		{
			return (match.Index, match.Length);
		}

		var trimmed = lineText.Trim();
		if (trimmed.Length == 0)
			return (Math.Min(columnIndex, lineText.Length), 0);

		var start = lineText.IndexOf(trimmed, StringComparison.Ordinal);
		if (trimmed.Length > 160)
		{
			var relativeColumn = Math.Max(0, columnIndex - start);
			start += Math.Max(0, Math.Min(relativeColumn, trimmed.Length - 160));
			return (start, 160);
		}

		return (start, trimmed.Length);
	}

	static Location CreateLocation(string filePath, SourceText sourceText, int lineIndex, int startColumn, int length)
	{
		var line = sourceText.Lines[lineIndex];
		var lineLength = line.Span.Length;
		startColumn = Math.Max(0, Math.Min(startColumn, lineLength));
		length = Math.Max(1, Math.Min(length, Math.Max(1, lineLength - startColumn)));
		var span = new TextSpan(line.Start + startColumn, length);
		var endColumn = Math.Min(lineLength, startColumn + length);
		return Location.Create(filePath, span, new LinePositionSpan(new LinePosition(lineIndex, startColumn), new LinePosition(lineIndex, endColumn)));
	}

	static string FormatMessage(string errorMessage, SourceInfo sourceInfo)
	{
		var message = errorMessage;
		if (!string.IsNullOrWhiteSpace(sourceInfo.FilePath))
			message += $" File: {sourceInfo.FilePath}";
		if (sourceInfo.LineNumber > 0)
			message += $"; Line: {sourceInfo.LineNumber}";
		if (sourceInfo.ColumnNumber > 0)
			message += $"; Column: {sourceInfo.ColumnNumber}";
		if (!string.IsNullOrWhiteSpace(sourceInfo.Excerpt))
			message += $"; Source: {sourceInfo.Excerpt}";
		return message;
	}

	static string StripLineInfoFromXmlExceptionMessage(string message)
	{
		var lineIndex = message.LastIndexOf(" Line ", StringComparison.Ordinal);
		if (lineIndex > 0)
			return message.Substring(0, lineIndex).TrimEnd('.', ' ');
		return message;
	}

	static IXmlLineInfo ExtractLineInfoFromMessage(string message)
	{
		var lineIndex = message.LastIndexOf(" Line ", StringComparison.Ordinal);
		if (lineIndex > 0)
		{
			var lineInfoPart = message.Substring(lineIndex + 6);
			var parts = lineInfoPart.Split(new[] { ", position " }, StringSplitOptions.None);
			if (parts.Length == 2)
			{
				var lineStr = parts[0].Trim();
				var posStr = parts[1].TrimEnd('.', ' ');
				if (int.TryParse(lineStr, out var lineNumber) && int.TryParse(posStr, out var linePosition))
					return new XmlLineInfo(lineNumber, linePosition);
			}
		}
		return new XmlLineInfo();
	}

	readonly record struct SourceInfo(Location? Location, string? FilePath, int LineNumber, int ColumnNumber, string? Excerpt);
}
