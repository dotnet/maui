using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class ListStringConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["List<string>", "System.Collections.Generic.List<string>", "System.Collections.Generic.IList`1[System.String]", "IList<string>"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			var listStringType = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!;
			var stringType = context.Compilation.GetSpecialType(SpecialType.System_String);
			var constructedListType = listStringType.Construct(stringType);
			return $"new {constructedListType.ToFQDisplayString()} {{ {string.Join(", ", value.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(v => $"\"{v.Trim()}\""))} }}";
		}

		context.ReportConversionFailed(xmlLineInfo, value, Descriptors.ListStringConversionFailed);
		return "default";
	}
}