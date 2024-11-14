using System;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Microsoft.Maui.Controls.Xaml;

using System.CodeDom.Compiler;

namespace Microsoft.Maui.Controls.SourceGen;

static class NodeSGExtensions
{
    public static bool TryGetPropertyName(this INode node, INode parentNode, out XmlName name)
    {
        name = default(XmlName);
        if (parentNode is not IElementNode parentElement)
            return false;
        foreach (var kvp in parentElement.Properties)
        {
            if (kvp.Value != node)
                continue;
            name = kvp.Key;
            return true;
        }
        return false;
    }

    public static bool IsCollectionItem(this INode parentNode, INode node)
    {
        if (parentNode is not IListNode parentList)
            return false;
        return parentList.CollectionItems.Contains(node);
    }

    public static string ConvertTo(this ValueNode valueNode, IFieldSymbol bpFieldSymbol, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter();
        if (typeandconverter == null)
            return string.Empty;
        return valueNode.ConvertTo(typeandconverter.Value.type, typeandconverter.Value.converter, context, iXmlLineInfo);
    }

    // public static string ConvertTo(this ValueNode valueNode, IPropertySymbol bpPropertySymbol) => valueNode.ConvertTo(bpPropertySymbol.Type);
    
    public static string ConvertTo(this ValueNode valueNode, ITypeSymbol toType, ITypeSymbol? typeConverter, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        var valueString = valueNode.Value as string ?? string.Empty;
        if (typeConverter != null)
            return valueNode.ConvertWithConverter(typeConverter, context, iXmlLineInfo);
		return toType.ToString() switch
		{
			"System.SByte" or "sbyte" or "System.Int16" or "short" or "System.Int32" or "int" or "System.Int64" or "long" or "System.Byte" or "byte" or "System.UInt16" or "ushort" or "System.UInt32" or "uint" or "System.UInt64" or "ulong" or "System.Single" or "float" or "System.Double" or "double" or "System.Decimal" => valueString,
			"System.Boolean" or "bool" => valueString.ToLowerInvariant(),
			"System.String" or "string" => $"\"{valueString}\"",
			"System.Object" or "object" => "new()",
			"System.Char" or "char" => $"'{valueString}'",
			"System.TimeSpan" => $"new global::System.TimeSpan({TimeSpan.Parse(valueString).Ticks})",
			"System.Uri" => $"new global::System.Uri(\"{valueString}\", global:System.UriKind.RelativeOrAbsolute)",
			_ => $"\"{valueString}\"",
		};
	}

    public static string ConvertWithConverter(this ValueNode valueNode, ITypeSymbol typeConverter, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        var valueString = valueNode.Value as string ?? string.Empty;
        //TODO check if there's a SourceGen version of the converter
        if (typeConverter.Implements(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.IExtendedTypeConverter")!))
        {
            //TODO
            //check the required services
            return $"((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new global::{typeConverter}()).ConvertFromInvariantString(\"{valueString}\", /*TODO*/ null)";
        }
        return $"new global::{typeConverter}().ConvertFromInvariantString(\"{valueString}\")";
    }
}
