using System;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.SourceGen;

public static class LocationHelpers
{
    public static Location LocationCreate(string filePath, IXmlLineInfo lineInfo, string text)
        => Location.Create(filePath!, new TextSpan(lineInfo.LinePosition, text.Length), new LinePositionSpan(new LinePosition(lineInfo.LineNumber-1, lineInfo.LinePosition), new LinePosition(lineInfo.LineNumber-1, lineInfo.LinePosition + text.Length)));

}
