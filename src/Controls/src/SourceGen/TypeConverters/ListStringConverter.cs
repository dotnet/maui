using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class ListStringConverter : BaseTypeConverter
{
	public override IEnumerable<string> SupportedTypes => new[] { "List<string>", "System.Collections.Generic.List<string>", "System.Collections.Generic.IList`1[System.String]", "IList<string>" };

	public override string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			return $"new global::System.Collections.Generic.List<string> {{ {string.Join(", ", value.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(v => $"\"{v.Trim()}\""))} }}";
		}

		ReportConversionFailed(context, xmlLineInfo, value, Descriptors.ListStringConversionFailed);
		return "default";
	}
}