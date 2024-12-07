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
using System.Reflection.Metadata;
using System.Net.Mime;
using System.Globalization;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace Microsoft.Maui.Controls.SourceGen;

static class NodeSGExtensions
{
	static Dictionary<ITypeSymbol, Func<string, Action<Diagnostic>, IXmlLineInfo, string, string>>? KnownSGTypeConverters;

    static Dictionary<ITypeSymbol, Func<string, Action<Diagnostic>, IXmlLineInfo, string, string>> GetKnownSGTypeConverters(SourceGenContext context)
        => KnownSGTypeConverters ??= new (SymbolEqualityComparer.Default)
	{
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Converters.RectTypeConverter")!, ConvertRect },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Converters.ColorTypeConverter")!, ConvertColor },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Converters.PointTypeConverter")!, ConvertPoint },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.ThicknessTypeConverter")!, ConvertThickness },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.CornerRadiusTypeConverter")!, ConvertCornerRadius },
		// { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.EasingTypeConverter")!, typeof(EasingTypeConverter) },
		// { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexJustifyTypeConverter")!, typeof(EnumTypeConverter<Layouts.FlexJustify>) },
		// { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexDirectionTypeConverter")!, typeof(EnumTypeConverter<Layouts.FlexDirection>) },
		// { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexAlignContentTypeConverter")!, typeof(EnumTypeConverter<Layouts.FlexAlignContent>) },
		// { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexAlignItemsTypeConverter")!, typeof(EnumTypeConverter<Layouts.FlexAlignItems>) },
		// { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexAlignSelfTypeConverter")!, typeof(EnumTypeConverter<Layouts.FlexAlignSelf>) },
		// { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexWrapTypeConverter")!, typeof(EnumTypeConverter<Layouts.FlexWrap>) },
		// { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexBasisTypeConverter")!, typeof(FlexBasisTypeConverter) },
	};

    static string ConvertRect(string value, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        // IMPORTANT! Update RectTypeDesignConverter.IsValid if making changes here
        var values = value.Split([','], StringSplitOptions.RemoveEmptyEntries)
                          .Select(v => v.Trim());

        if (!string.IsNullOrEmpty(value))
        {
            string[] xywh = value.Split(',');
            if (xywh.Length == 4
                && double.TryParse(xywh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double x)
                && double.TryParse(xywh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double y)
                && double.TryParse(xywh[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double w)
                && double.TryParse(xywh[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
            {
                return $"new global::Microsoft.Maui.Graphics.Rect({x}, {y}, {w}, {h})";
            }
        }

        // TODO use correct position
        reportDiagnostic(Diagnostic.Create(Descriptors.RectConversionFailed, null, value));

        return "default";
    }

    static string ConvertColor(string value, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        // Q: I don't think we can do any more validation?
        if (!string.IsNullOrEmpty(value))
        {
            return $"global::Microsoft.Maui.Graphics.Color.Parse(\"{value}\")";
        }

        // TODO use correct position
        reportDiagnostic(Diagnostic.Create(Descriptors.ColorConversionFailed, null, value));

        return "default";
    }

    static string ConvertPoint(string value, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        // IMPORTANT! Update RectTypeDesignConverter.IsValid if making changes here
        if (!string.IsNullOrEmpty(value))
        {
            string[] xy = value.Split(',');
            if (xy.Length == 2 && double.TryParse(xy[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var x)
                && double.TryParse(xy[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var y))
            {
                return $"new global::Microsoft.Maui.Graphics.Point({x}, {y})";
            }
        }

        // TODO use correct position
        reportDiagnostic(Diagnostic.Create(Descriptors.PointConversionFailed, null, value));

        return "default";
    }

    // Q: Do we want to support CSS notation?
    static string ConvertThickness(string value, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        // IMPORTANT! Update ThicknessTypeDesignConverter.IsValid if making changes here
        if (!string.IsNullOrEmpty(value))
        {
            value = value.Trim();

            if (value.Contains(','))
            { //Xaml
                var thickness = value.Split(',');
                switch (thickness.Length)
                {
                    case 2:
                        if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double h)
                            && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double v))
                            return $"new global::Microsoft.Maui.Thickness({h}, {v})";
                        break;
                    case 4:
                        if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double l)
                            && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
                            && double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double r)
                            && double.TryParse(thickness[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
                            return $"new global::Microsoft.Maui.Thickness({l}, {t}, {r}, {b})";
                        break;
                }
            }
            else if (value.Contains(' '))
            { //CSS
                var thickness = value.Split(' ');
                switch (thickness.Length)
                {
                    case 2:
                        if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double v)
                            && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
                            return $"new global::Microsoft.Maui.Thickness({h}, {v})";
                        break;
                    case 3:
                        if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
                            && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out h)
                            && double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
                            return $"new global::Microsoft.Maui.Thickness({h}, {t}, {h}, {b})";
                        break;
                    case 4:
                        if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out t)
                            && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double r)
                            && double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out b)
                            && double.TryParse(thickness[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
                            return $"new global::Microsoft.Maui.Thickness({l}, {t}, {r}, {b})";
                        break;
                }
            }
            else
            { //single uniform thickness
                if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
                    return $"new global::Microsoft.Maui.Thickness({l})";
            }
        }

        // TODO use correct position
        reportDiagnostic(Diagnostic.Create(Descriptors.ThicknessConversionFailed, null, value));

        return "default";
    }

    // Q: Do we want to support CSS notation?
    static string ConvertCornerRadius(string value, Action<Diagnostic> reportDiagnostic, IXmlLineInfo xmlLineInfo, string filePath)
    {
        // IMPORTANT! Update CornerRadiusDesignTypeConverter.IsValid if making changes here

        if (!string.IsNullOrEmpty(value))
        {
            value = value.Trim();
            if (value.Contains(','))
            { //Xaml
                var cornerRadius = value.Split(',');
                if (cornerRadius.Length == 4
                    && double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double tl)
                    && double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double tr)
                    && double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double bl)
                    && double.TryParse(cornerRadius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double br))
                    return $"new global::Microsoft.Maui.CornerRadius({tl}, {tr}, {bl}, {br})";

                if (cornerRadius.Length > 1
                    && cornerRadius.Length < 4
                    && double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
                    return $"new global::Microsoft.Maui.CornerRadius({l})";
            }
            else if (value.Contains(' '))
            { //CSS
                var cornerRadius = value.Split(' ');
                if (cornerRadius.Length == 2
                    && double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
                    && double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
                    return $"new global::Microsoft.Maui.CornerRadius({t}, {b}, {b}, {t})";
                if (cornerRadius.Length == 3
                    && double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double tl)
                    && double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double trbl)
                    && double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double br))
                    return $"new global::Microsoft.Maui.CornerRadius({tl}, {trbl}, {trbl}, {br})";
                if (cornerRadius.Length == 4
                    && double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out tl)
                    && double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double tr)
                    && double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double bl)
                    && double.TryParse(cornerRadius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out br))
                    return $"new global::Microsoft.Maui.CornerRadius({tl}, {tr}, {bl}, {br})";

            }
            else
            { //single uniform CornerRadius
                if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
                    return $"new global::Microsoft.Maui.CornerRadius({l})";
            }
        }

        // TODO use correct position
        reportDiagnostic(Diagnostic.Create(Descriptors.CornerRadiusConversionFailed, null, value));

        return "default";
    }

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

	public static bool IsResourceDictionary(this IElementNode node, SourceGenContext context)
        => context.Variables.TryGetValue(node, out var variable) && variable.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary")!);

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

    public static string ConvertTo(this ValueNode valueNode, ITypeSymbol toType, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        List<AttributeData> attributes = [.. toType.GetAttributes()];
        
        var typeConverter = attributes.FirstOrDefault(ad => ad.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")?.ConstructorArguments[0].Value as ITypeSymbol;     

        return valueNode.ConvertTo(toType, typeConverter, context, iXmlLineInfo);
    }
    
    public static string ConvertTo(this ValueNode valueNode, ITypeSymbol toType, ITypeSymbol? typeConverter, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        var valueString = valueNode.Value as string ?? string.Empty;

        if (typeConverter is not null && GetKnownSGTypeConverters(context).TryGetValue(typeConverter, out var converter))
            return converter.Invoke(valueString, context.ReportDiagnostic, iXmlLineInfo, context.FilePath!);

        if (typeConverter is not null)
            return valueNode.ConvertWithConverter(typeConverter, toType, context, iXmlLineInfo);

        return ValueForLanguagePrimitive(valueString, toType, context);
    }

    public static string ValueForLanguagePrimitive(string valueString, ITypeSymbol toType, SourceGenContext context)
    {
        if (toType.NullableAnnotation == NullableAnnotation.Annotated)
            toType = ((INamedTypeSymbol)toType).TypeArguments[0];

        if (toType.SpecialType == SpecialType.System_SByte && sbyte.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var sbyteValue))
            return SymbolDisplay.FormatPrimitive(sbyteValue, true, false);
        if (toType.SpecialType == SpecialType.System_Byte && byte.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var byteValue))
            return SymbolDisplay.FormatPrimitive(byteValue, true, false);
        if (toType.SpecialType == SpecialType.System_Int16 && short.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var shortValue))
            return SymbolDisplay.FormatPrimitive(shortValue, true, false);
        if (toType.SpecialType == SpecialType.System_UInt16 && ushort.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var ushortValue))
            return SymbolDisplay.FormatPrimitive(ushortValue, true, false);
        if (toType.SpecialType == SpecialType.System_Int32 && int.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var intValue))
            return SymbolDisplay.FormatPrimitive(intValue, true, false);
        if (toType.SpecialType == SpecialType.System_UInt32 && uint.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var uintValue))
            return SymbolDisplay.FormatPrimitive(uintValue, true, false);
        if (toType.SpecialType == SpecialType.System_Int64 && long.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var longValue))
            return SymbolDisplay.FormatPrimitive(longValue, true, false);
        if (toType.SpecialType == SpecialType.System_UInt64 && ulong.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var ulongValue))
            return SymbolDisplay.FormatPrimitive(ulongValue, true, false);
        if (toType.SpecialType == SpecialType.System_Single && float.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var floatValue))
            return SymbolDisplay.FormatPrimitive(floatValue, true, false);
        if (toType.SpecialType == SpecialType.System_Double && double.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var doubleValue))
            return SymbolDisplay.FormatPrimitive(doubleValue, true, false);
        if (toType.SpecialType == SpecialType.System_Boolean && bool.TryParse(valueString, out var boolValue))
            return SymbolDisplay.FormatPrimitive(boolValue, true, false);
        if (toType.SpecialType == SpecialType.System_Char && char.TryParse(valueString, out var charValue))
            return SymbolDisplay.FormatPrimitive(charValue, true, false);
        if (toType.SpecialType == SpecialType.System_String)
            return SymbolDisplay.FormatLiteral(valueString, true);    
        if (toType.SpecialType == SpecialType.System_DateTime && DateTime.TryParse(valueString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeValue))
            return $"new global::System.DateTime({dateTimeValue.Ticks})";
        if (toType.SpecialType == SpecialType.System_Decimal && decimal.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
            return $"new global::System.Decimal({decimalValue})";
        if (toType.TypeKind == TypeKind.Enum)
            return string.Join(" | ", valueString.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(v => $"{toType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{v.Trim()}"));
        if (toType.Equals(context.Compilation.GetTypeByMetadataName("System.TimeSpan")!, SymbolEqualityComparer.Default) && TimeSpan.TryParse(valueString, CultureInfo.InvariantCulture, out var timeSpanValue))
            return $"new global::System.TimeSpan({timeSpanValue.Ticks})";
        if (toType.Equals(context.Compilation.GetTypeByMetadataName("System.Uri")!, SymbolEqualityComparer.Default))
            return $"new global::System.Uri(\"{valueString}\", global::System.UriKind.RelativeOrAbsolute)";

        //default
        return SymbolDisplay.FormatLiteral(valueString, true);    
	}

    static string ConvertValue(ValueNode valueNode)
    {
        var valueString = valueNode.Value as string ?? string.Empty;
        var values = valueString.Split([','], StringSplitOptions.RemoveEmptyEntries)
                                .Select(v => v.Trim());
        return $"new global::Microsoft.Maui.Graphics.Rectangle({string.Join(", ", values)})";
    }
    public static string ConvertWithConverter(this ValueNode valueNode, ITypeSymbol typeConverter, ITypeSymbol targetType, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        var valueString = valueNode.Value as string ?? string.Empty;
        //TODO check if there's a SourceGen version of the converter
        if (typeConverter.Implements(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.IExtendedTypeConverter")!))
        {

            var (acceptEmptyServiceProvider, requiredServices) = typeConverter.GetServiceProviderAttributes(context);
            if (!acceptEmptyServiceProvider)
            {
                var serviceProvider = valueNode.GetOrCreateServiceProvider(context.Writer, context, requiredServices);
                return $"((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new {typeConverter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()).ConvertFromInvariantString(\"{valueString}\", {serviceProvider.Name}) as {targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}";
            }
            else //should never happen. there's no point to implement IExtendedTypeConverter AND accept empty service provider
                return $"((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new {typeConverter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()).ConvertFromInvariantString(\"{valueString}\", null) as {targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}";
        }
        //TypeConverter returns an object, so we need to cast it to the target type
        if (targetType.IsReferenceType || targetType.NullableAnnotation == NullableAnnotation.Annotated)
            return $"((global::System.ComponentModel.TypeConverter)new {typeConverter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()).ConvertFromInvariantString(\"{valueString}\") as {targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}";
        else
            return $"({targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})new {typeConverter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}().ConvertFromInvariantString(\"{valueString}\")!";
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
            //HACK waiting for the ValueProvider to be compiled
            if (variable.Type.Equals(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Setter")!, SymbolEqualityComparer.Default))
                returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Setter")!;
            if (variable.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.TriggerBase")!))
                returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.TriggerBase")!;
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

    [Conditional("_SOURCEGEN_SOURCEINFO_ENABLE")]
    public static void RegisterSourceInfo(this INode node, SourceGenContext context, IndentedTextWriter writer)
    {
        if (!context.Variables.TryGetValue(node, out var variable))
            return;

        var assembly = context.Compilation.Assembly.Name;
        var filePath = context.FilePath;
        var lineInfo = node as IXmlLineInfo;
        using (PrePost.NewConditional(writer, "_MAUIXAML_SG_SOURCEINFO"))
        {
            writer.WriteLine($"global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo({variable.Name}!, new global::System.Uri(\"{filePath};assembly={assembly}\", global::System.UriKind.Relative), {lineInfo?.LineNumber ?? -1}, {lineInfo?.LinePosition ?? -1});");
        }
    }
}
