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
    public static bool TryGetPropertyName(this INode parentNode, INode node, out XmlName name)
    {
        name = default(XmlName);
        if (!(parentNode is IElementNode parentElement))
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
        if (!(parentNode is IListNode parentList))
            return false;
        return parentList.CollectionItems.Contains(node);
    }

    public static string ConvertTo(this ValueNode valueNode, IFieldSymbol bpFieldSymbol, IXmlLineInfo iXmlLineInfo)
    {
        var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter();
        if (typeandconverter == null)
            return string.Empty;
        return valueNode.ConvertTo(typeandconverter.Value.type, typeandconverter.Value.converter, iXmlLineInfo);
    }

    // public static string ConvertTo(this ValueNode valueNode, IPropertySymbol bpPropertySymbol) => valueNode.ConvertTo(bpPropertySymbol.Type);
    
    public static string ConvertTo(this ValueNode valueNode, ITypeSymbol toType, ITypeSymbol? typeConverter, IXmlLineInfo iXmlLineInfo)
    {
        var valueString = valueNode.Value as string ?? string.Empty;
        if (typeConverter != null)
            return $"new global:{typeConverter}().ConvertFromInvariantString(\"{valueString}\")";
        switch (toType.ToString()) {
        case "System.SByte":
        case "System.Int16":
        case "System.Int32":
        case "System.Int64":
        case "System.Byte":
        case "System.UInt16":
        case "System.UInt32":
        case "System.UInt64":
        case "System.Single":
        case "System.Double":
        case "System.Decimal": return valueString;
        case "System.Boolean": return valueString.ToLowerInvariant();
        case "System.String": return $"\"{valueString}\"";
        case "System.Object": return "new()";
        case "System.Char": return $"'{valueString}'";
        case "System.TimeSpan":
            var span = TimeSpan.Parse(valueString);
            return $"new global::System.TimeSpan({span.Ticks})";
        case "System.Uri": return $"new global::System.Uri(\"{valueString}\", global:System.UriKind.RelativeOrAbsolute)";
        }

        return $"\"{valueString}\"";        
    }
}
