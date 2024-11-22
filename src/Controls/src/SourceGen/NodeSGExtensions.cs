using System;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Microsoft.Maui.Controls.Xaml;

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

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

    public static string ConvertTo(this ValueNode valueNode, IPropertySymbol property, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        List<AttributeData> attributes = [
            .. property.GetAttributes().ToList(),
            .. property.Type.GetAttributes()
        ];
        
        var typeConverter = attributes.FirstOrDefault(ad => ad.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")?.ConstructorArguments[0].Value as ITypeSymbol;     
        return valueNode.ConvertTo(property.Type, typeConverter, context, iXmlLineInfo);
    }
    
    public static string ConvertTo(this ValueNode valueNode, ITypeSymbol toType, ITypeSymbol? typeConverter, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        var valueString = valueNode.Value as string ?? string.Empty;
        if (typeConverter != null)
            return valueNode.ConvertWithConverter(typeConverter, context, iXmlLineInfo);
        if (toType.NullableAnnotation == NullableAnnotation.Annotated)
            toType = ((INamedTypeSymbol)toType).TypeArguments[0];
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

            var (acceptEmptyServiceProvider, requiredServices) = typeConverter.GetServiceProviderAttributes(context);
            if (!acceptEmptyServiceProvider)
            {
                var serviceProvider = valueNode.GetOrCreateServiceProvider(context.Writer, context, requiredServices);
                return $"((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new {typeConverter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()).ConvertFromInvariantString(\"{valueString}\", {serviceProvider.Name})";
            }
            else //should never happen. there's no point to implement IExtendedTypeConverter AND accept empty service provider
                return $"((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new {typeConverter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()).ConvertFromInvariantString(\"{valueString}\", null)";
        }
        
        return $"new {typeConverter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}().ConvertFromInvariantString(\"{valueString}\")";
    }

    public static bool IsValueProvider(this INode node, SourceGenContext context, 
            out ITypeSymbol returnType, 
            out ITypeSymbol? iface, 
            out bool acceptEmptyServiceProvider, 
            out ImmutableArray<ITypeSymbol>? requiredServices)
    {
        returnType = context.Compilation.ObjectType;
        iface = null;
        acceptEmptyServiceProvider = false;
        requiredServices = null;

        if (!context.Variables.TryGetValue(node, out var variable))
            return false;
        
        if (variable.Type.Implements(iface = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IValueProvider")!))
        {
        }
        else if (variable.Type.ImplementsGeneric(iface = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IMarkupExtension`1")!, out var typeArg))
        {
            iface = ((INamedTypeSymbol)iface).Construct(typeArg[0]);
            returnType = typeArg[0];
        }
        else if (variable.Type.Implements(iface = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.IMarkupExtension")!))
        {
        }
        else
            return false;

        var requireServiceAttribute = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.RequireServiceAttribute")!;
        requiredServices = variable.Type.GetAttributes(requireServiceAttribute).FirstOrDefault()?.ConstructorArguments[0].Values.Where(ca => ca.Value is ITypeSymbol).Select(ca => (ca.Value as ITypeSymbol)!).ToImmutableArray() ?? null;
        
        var acceptEmptyServiceProviderAttribute = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.AcceptEmptyServiceProviderAttribute")!;
        acceptEmptyServiceProvider = variable.Type.GetAttributes(acceptEmptyServiceProviderAttribute).Any();

        return true;
    }
}
