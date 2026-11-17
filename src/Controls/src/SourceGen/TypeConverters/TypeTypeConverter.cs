using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class TypeTypeConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["Type", "System.Type"];

	public static INamedTypeSymbol? ResolveType(string value, BaseNode node, SourceGenContext context)
	{
		var xmlType = TypeArgumentsParser.ParseSingle(value, node.NamespaceResolver, node);
		return xmlType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, context.TypeCache, out var type)
			? type
			: null;
	}

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var typeRef = ResolveType(value, node, context);
		if (typeRef != null)
			return $"typeof({typeRef.ToFQDisplayString()})";
		context.ReportConversionFailed((IXmlLineInfo)node, value, Descriptors.TypeResolution);
		return "default";
	}
}