using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class TypeTypeConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["Type", "System.Type"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		if (string.IsNullOrEmpty(value))
			goto error;

		var split = value.Split(':');
		if (split.Length > 2)
			goto error;

		XmlType xmlType;
		if (split.Length == 2)
			xmlType = new XmlType(node.NamespaceResolver.LookupNamespace(split[0]), split[1], null);
		else
			xmlType = new XmlType(node.NamespaceResolver.LookupNamespace(""), split[0], null);
		var typeRef = xmlType.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache);
		if (typeRef != null)
			return $"typeof({typeRef.ToFQDisplayString()})";
		error:
		context.ReportConversionFailed((IXmlLineInfo)node, value, Descriptors.TypeResolution);
		return "default";
	}
}