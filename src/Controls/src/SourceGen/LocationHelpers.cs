using System;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.SourceGen;

public static class LocationHelpers
{
	public static Location LocationCreate(string filePath, IXmlLineInfo lineInfo, string text)
	{
		var lineNumber = lineInfo.LineNumber <= 0 ? 1 : lineInfo.LineNumber;
		var linePosition = lineInfo.LinePosition <= 0 ? 1 : lineInfo.LinePosition;
		return Location.Create(filePath!, new TextSpan(linePosition, text.Length), new LinePositionSpan(new LinePosition(lineNumber - 1, linePosition), new LinePosition(lineNumber - 1, linePosition + text.Length)));
	}

	public static Location LocationCreate(string filePath, int startLine, int startColumn, int endLine, int endColumn)
	{
		var lineNumber = startLine <= 0 ? 1 : startLine;
		var linePosition = startColumn <= 0 ? 1 : startColumn;
		return Location.Create(filePath!, new TextSpan(linePosition, 1), new LinePositionSpan(new LinePosition(lineNumber - 1, linePosition - 1), new LinePosition(endLine - 1, endColumn)));
	}
}