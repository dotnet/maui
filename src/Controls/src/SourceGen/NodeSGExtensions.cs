using System;
using System.Xml;

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using System.Globalization;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

using static LocationHelpers;
static class NodeSGExtensions
{
    public delegate string ConverterDelegate(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null);

    public delegate string ProvideValueDelegate(IElementNode markupNode, SourceGenContext context, out ITypeSymbol? returnType);

	static Dictionary<ITypeSymbol, (ConverterDelegate, ITypeSymbol)>? KnownSGTypeConverters;

    static Dictionary<ITypeSymbol, (ConverterDelegate, ITypeSymbol)> GetKnownSGTypeConverters(SourceGenContext context)
        => KnownSGTypeConverters ??= new (SymbolEqualityComparer.Default)
	{
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Converters.RectTypeConverter")!, (KnownTypeConverters.ConvertRect, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Rect")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Converters.ColorTypeConverter")!, (KnownTypeConverters.ConvertColor, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Converters.PointTypeConverter")!, (KnownTypeConverters.ConvertPoint, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Point")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.ThicknessTypeConverter")!, (KnownTypeConverters.ConvertThickness, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Thickness")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.CornerRadiusTypeConverter")!, (KnownTypeConverters.ConvertCornerRadius, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.CornerRadius")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.EasingTypeConverter")!, (KnownTypeConverters.ConvertEasing, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Easing")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexJustifyTypeConverter")!, (KnownTypeConverters.ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexJustify")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexDirectionTypeConverter")!, (KnownTypeConverters.ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexDirection")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexAlignContentTypeConverter")!, (KnownTypeConverters.ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexAlignContent")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexAlignItemsTypeConverter")!, (KnownTypeConverters.ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexAlignItems")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexAlignSelfTypeConverter")!, (KnownTypeConverters.ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexAlignSelf")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexWrapTypeConverter")!, (KnownTypeConverters.ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexWrap")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexBasisTypeConverter")!, (KnownTypeConverters.ConvertFlexBasis, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexBasis")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.FlowDirectionConverter")!, (KnownTypeConverters.ConvertFlowDirection, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.FlowDirection")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.GridLengthTypeConverter")!, (KnownTypeConverters.ConvertGridLength, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.GridLength")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ColumnDefinitionCollectionTypeConverter")!, (KnownTypeConverters.ConvertColumnDefinitionCollection, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ColumnDefinitionCollection")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.RowDefinitionCollectionTypeConverter")!, (KnownTypeConverters.ConvertRowDefinitionCollection, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.RowDefinitionCollection")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ImageSourceConverter")!, (KnownTypeConverters.ConvertImageSource, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ImageSource")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ListStringTypeConverter")!, (KnownTypeConverters.ConvertListString, context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1")!.Construct(context.Compilation.GetSpecialType(SpecialType.System_String))!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.PointCollectionConverter")!, (KnownTypeConverters.ConvertPointCollection, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.PointCollection")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.LayoutOptionsConverter")!, (KnownTypeConverters.ConvertLayoutOptions, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.LayoutOptions")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Compatibility.ConstraintTypeConverter")!, (KnownTypeConverters.ConvertConstraint, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Compatibility.Constraint")!) },
        { context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary+RDSourceTypeConverter")!, (KnownTypeConverters.ConvertRDSource, context.Compilation.GetTypeByMetadataName("System.Uri")!) },
        
        // TODO: PathFigureCollectionConverter (used for PathGeometry and StrokeShape) is very complex, skipping for now, apart from that one all other shapes work though
        //{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.PathGeometryConverter")!, (KnownTypeConverters.ConvertPathGeometry, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Geometry")!) },
        //{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.StrokeShapeTypeConverter")!, (KnownTypeConverters.ConvertStrokeShape, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Shape")!) },
	};

    static Dictionary<ITypeSymbol, ProvideValueDelegate>? KnownSGMarkups;
    public static Dictionary<ITypeSymbol, ProvideValueDelegate> GetKnownValueProviders(SourceGenContext context)
        => KnownSGMarkups ??= new (SymbolEqualityComparer.Default)
    {
        {context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.StaticExtension")!, KnownMarkups.ProvideValueForStaticExtension},
        {context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Setter")!, KnownMarkups.ProvideValueForSetter},
        {context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.DynamicResourceExtension")!, KnownMarkups.ProvideValueForDynamicResourceExtension},
        {context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.StyleSheetExtension")!, KnownMarkups.ProvideValueForStyleSheetExtension},
    };

    public static bool TryGetPropertyName(this INode node, INode parentNode, out XmlName name)
    {
        name = default;
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

    public static bool CanConvertTo(this ValueNode valueNode, IFieldSymbol bpFieldSymbol, SourceGenContext context)
    {
        var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter(context);
        if (typeandconverter == null)
            return false;
        return valueNode.CanConvertTo(typeandconverter.Value.type, typeandconverter.Value.converter, context);
    }

    public static bool CanConvertTo(this ValueNode valueNode, IPropertySymbol property, SourceGenContext context)
    {
        List<AttributeData> attributes = [
            .. property.GetAttributes().ToList(),
            .. property.Type.GetAttributes()
        ];
        
        var typeConverter = attributes.FirstOrDefault(ad => ad.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")?.ConstructorArguments[0].Value as ITypeSymbol;     
        return valueNode.CanConvertTo(property.Type, typeConverter, context);
    }

    public static bool CanConvertTo(this ValueNode valueNode, ITypeSymbol toType, ITypeSymbol? converter, SourceGenContext context)
    {
        var stringValue = (string)valueNode.Value;

        //if there's a typeconverter, assume we can convert
        if (converter is not null && stringValue is not null)
            return true;
        
        if (toType.NullableAnnotation == NullableAnnotation.Annotated)
            toType = ((INamedTypeSymbol)toType).TypeArguments[0];

        if (toType.TypeKind == TypeKind.Enum)
            return true;

        //assignable from a string
        if (toType.SpecialType == SpecialType.System_Char)
            return true;
        if (toType.SpecialType == SpecialType.System_SByte)
            return true;
        if (toType.SpecialType == SpecialType.System_Int16)
            return true;            
        if (toType.SpecialType == SpecialType.System_Int32)
            return true;            
        if (toType.SpecialType == SpecialType.System_Int64)
            return true;            
        if (toType.SpecialType == SpecialType.System_Byte)
            return true;
        if (toType.SpecialType == SpecialType.System_UInt16)
            return true;
        if (toType.SpecialType == SpecialType.System_UInt32)
            return true;
        if (toType.SpecialType == SpecialType.System_UInt64)
            return true;
        if (toType.SpecialType == SpecialType.System_Single)
            return true;
        if (toType.SpecialType == SpecialType.System_Double)
            return true;
        if (toType.SpecialType == SpecialType.System_Boolean)
            return true;
        if (toType.SpecialType == SpecialType.System_DateTime)
            return true;
        if (toType.SpecialType == SpecialType.System_Object)
            return true;
        if (toType.SpecialType == SpecialType.System_String)
            return true;
        if (toType.SpecialType == SpecialType.System_Decimal)
            return true;
        if (context.Compilation.HasImplicitConversion(context.Compilation.GetTypeByMetadataName("System.String")!, toType))
            return true;
        if (toType.Equals(context.Compilation.GetTypeByMetadataName("System.TimeSpan")!, SymbolEqualityComparer.Default))
            return true;
        return false;
    }

	public static string ConvertTo(this ValueNode valueNode, IFieldSymbol bpFieldSymbol, SourceGenContext context, LocalVariable? parentVar = null)
    {
        var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter(context);
        if (typeandconverter == null)
            return string.Empty;
        return valueNode.ConvertTo(typeandconverter.Value.type, typeandconverter.Value.converter, context, parentVar);
    }

    public static string ConvertTo(this ValueNode valueNode, IPropertySymbol property, SourceGenContext context, LocalVariable? parentVar = null)
    {
        List<AttributeData> attributes = [
            .. property.GetAttributes().ToList(),
            .. property.Type.GetAttributes()
        ];
        
        var typeConverter = attributes.FirstOrDefault(ad => ad.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")?.ConstructorArguments[0].Value as ITypeSymbol;     
        return valueNode.ConvertTo(property.Type, typeConverter, context, parentVar);
    }

    public static string ConvertTo(this ValueNode valueNode, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
    {
        List<AttributeData> attributes = [.. toType.GetAttributes()];
        
        var typeConverter = attributes.FirstOrDefault(ad => ad.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")?.ConstructorArguments[0].Value as ITypeSymbol;     

        return valueNode.ConvertTo(toType, typeConverter, context, parentVar);
    }
    
    public static string ConvertTo(this ValueNode valueNode, ITypeSymbol toType, ITypeSymbol? typeConverter, SourceGenContext context, LocalVariable? parentVar = null)
    {
        var valueString = valueNode.Value as string ?? string.Empty;

        if (typeConverter is not null && GetKnownSGTypeConverters(context).TryGetValue(typeConverter, out var converterAndReturnType))
        {
            var returntype = converterAndReturnType.Item2;
            if (!context.Compilation.HasImplicitConversion(returntype, toType))
                //this could be left to the compiler to figure, but I've yet to find a way to test the compiler...
                //FIXME: better error message
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberResolution, LocationCreate(context.FilePath!, (IXmlLineInfo)valueNode, valueString), $"Cannot convert {returntype} to {toType}"));
            return converterAndReturnType.Item1.Invoke(valueString, valueNode, toType, context, parentVar);
        }

        if (typeConverter is not null)
            return valueNode.ConvertWithConverter(typeConverter, toType, context, parentVar);

        return ValueForLanguagePrimitive(valueString, toType, context, valueNode);
    }

    public static string ValueForLanguagePrimitive(string valueString, ITypeSymbol toType, SourceGenContext context, IXmlLineInfo lineInfo)
    {
#pragma warning disable RS0030 // Do not use banned APIs
		void reportDiagnostic() 
            => context.ReportDiagnostic(Diagnostic.Create(Descriptors.InvalidFormat, LocationHelpers.LocationCreate(context.FilePath!, lineInfo, valueString), valueString, toType.ToDisplayString()));
#pragma warning restore RS0030 // Do not use banned APIs

		if (toType.NullableAnnotation == NullableAnnotation.Annotated)
            toType = ((INamedTypeSymbol)toType).TypeArguments[0];

        if (toType.SpecialType == SpecialType.System_SByte)
        {
            if(sbyte.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var sbyteValue))
                return SymbolDisplay.FormatPrimitive(sbyteValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_Byte)
        {
            if (byte.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var byteValue))
                return SymbolDisplay.FormatPrimitive(byteValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_Int16)
        {
            if (short.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var shortValue))
                return SymbolDisplay.FormatPrimitive(shortValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_UInt16)
        {
            if (short.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var ushortValue))
                return SymbolDisplay.FormatPrimitive(ushortValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_Int32)
        {
            if (int.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var intValue))
                return SymbolDisplay.FormatPrimitive(intValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_UInt32)
        {
            if (uint.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var uintValue))
                return SymbolDisplay.FormatPrimitive(uintValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_Int64)
        {
            if (long.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var longValue))
                return SymbolDisplay.FormatPrimitive(longValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_UInt64)
        {
            if (ulong.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var ulongValue))
                return SymbolDisplay.FormatPrimitive(ulongValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_Single)
        {
            if (float.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var floatValue))
                return SymbolDisplay.FormatPrimitive(floatValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_Double)
        {
            if (double.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var doubleValue))
                return SymbolDisplay.FormatPrimitive(doubleValue, true, false);
            else
                reportDiagnostic();
        }    
        if (toType.SpecialType == SpecialType.System_Boolean)
        {
            if (bool.TryParse(valueString, out var boolValue))
                return SymbolDisplay.FormatPrimitive(boolValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_Char)
        {
            if (char.TryParse(valueString, out var charValue))
                return SymbolDisplay.FormatPrimitive(charValue, true, false);
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_String && valueString.StartsWith("{}", StringComparison.Ordinal))
            return SymbolDisplay.FormatLiteral(valueString.Substring(2), true);
        if (toType.SpecialType == SpecialType.System_String)
            return SymbolDisplay.FormatLiteral(valueString, true);    
        if (toType.SpecialType == SpecialType.System_DateTime)
        {
            if (DateTime.TryParse(valueString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeValue))
                return $"new global::System.DateTime({dateTimeValue.Ticks})";
            else
                reportDiagnostic();
        }
        if (toType.SpecialType == SpecialType.System_Decimal)
        {
            if (decimal.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
                return $"new global::System.Decimal({decimalValue})";
            else
                reportDiagnostic();
        }
        if (toType.TypeKind == TypeKind.Enum)
            return string.Join(" | ", valueString.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(v => $"{toType.ToFQDisplayString()}.{v.Trim()}"));
        if (toType.Equals(context.Compilation.GetTypeByMetadataName("System.TimeSpan")!, SymbolEqualityComparer.Default))
        {
            if (TimeSpan.TryParse(valueString, CultureInfo.InvariantCulture, out var timeSpanValue))
                return $"new global::System.TimeSpan({timeSpanValue.Ticks})";
            else
                reportDiagnostic();
        }
        if (toType.Equals(context.Compilation.GetTypeByMetadataName("System.Uri")!, SymbolEqualityComparer.Default))
            return $"new global::System.Uri(\"{valueString}\", global::System.UriKind.RelativeOrAbsolute)";

        //default
        return SymbolDisplay.FormatLiteral(valueString, true);    
	}

    public static string ConvertWithConverter(this ValueNode valueNode, ITypeSymbol typeConverter, ITypeSymbol targetType, SourceGenContext context, LocalVariable? parentVar = null)
    {
        var valueString = valueNode.Value as string ?? string.Empty;
        //TODO check if there's a SourceGen version of the converter
        if (typeConverter.Implements(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.IExtendedTypeConverter")!))
        {

            var (acceptEmptyServiceProvider, requiredServices) = typeConverter.GetServiceProviderAttributes(context);
            if (!acceptEmptyServiceProvider)
            {
                var serviceProvider = valueNode.GetOrCreateServiceProvider(context.Writer, context, requiredServices);
                if (targetType.IsReferenceType || targetType.NullableAnnotation == NullableAnnotation.Annotated)
                    return $"((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new {typeConverter.ToFQDisplayString()}()).ConvertFromInvariantString(\"{valueString}\", {serviceProvider.Name}) as {targetType.ToFQDisplayString()}";
                else
                    return $"({targetType.ToFQDisplayString()})((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new {typeConverter.ToFQDisplayString()}()).ConvertFromInvariantString(\"{valueString}\", {serviceProvider.Name})";
            }
            else //should never happen. there's no point to implement IExtendedTypeConverter AND accept empty service provider
                return $"((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new {typeConverter.ToFQDisplayString()}()).ConvertFromInvariantString(\"{valueString}\", null) as {targetType.ToFQDisplayString()}";
        }
        //TypeConverter returns an object, so we need to cast it to the target type
        if (targetType.IsReferenceType || targetType.NullableAnnotation == NullableAnnotation.Annotated)
            return $"((global::System.ComponentModel.TypeConverter)new {typeConverter.ToFQDisplayString()}()).ConvertFromInvariantString(\"{valueString}\") as {targetType.ToFQDisplayString()}";
        else
            return $"({targetType.ToFQDisplayString()})new {typeConverter.ToFQDisplayString()}().ConvertFromInvariantString(\"{valueString}\")!";
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
            //the following 2 should go away when https://github.com/dotnet/maui/pull/26671 is merged    
            if (variable.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.PropertyCondition")!))
                returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.PropertyCondition")!;
            if (variable.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindingCondition")!))
                returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindingCondition")!;
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

    public static void ProvideValue(this ElementNode node, IndentedTextWriter writer, SourceGenContext context, ITypeSymbol? returnType, ITypeSymbol? valueProviderFace, bool acceptEmptyServiceProvider, ImmutableArray<ITypeSymbol>? requiredServices)
    {
        var valueProviderVariable = context.Variables[node];
        var variableName = NamingHelpers.CreateUniqueVariableName(context, returnType!.Name!.Split('.').Last());

        //if it require a serviceprovider, create one
        if (!acceptEmptyServiceProvider)
        {
            var serviceProviderVar = node.GetOrCreateServiceProvider(writer, context, requiredServices);                
            writer.WriteLine($"var {variableName} = ({returnType.ToFQDisplayString()})(({valueProviderFace!.ToFQDisplayString()}){valueProviderVariable.Name}).ProvideValue({serviceProviderVar.Name});");
            context.Variables[node] = new LocalVariable(returnType, variableName);
        }
        else {
            writer.WriteLine($"var {variableName} = ({returnType.ToFQDisplayString()})(({valueProviderFace!.ToFQDisplayString()}){valueProviderVariable.Name}).ProvideValue(null);");
            context.Variables[node] = new LocalVariable(returnType, variableName);
        }
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
            writer.WriteLine($"global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo({variable.Name}!, new global::System.Uri(\"{filePath};assembly={assembly}\", global::System.UriKind.Relative), {lineInfo?.LineNumber ?? -1}, {lineInfo?.LinePosition ?? -1});");
    }

    public static IFieldSymbol GetBindableProperty(this ValueNode node, SourceGenContext context)
    {
        //FIXME: report diagnostic on missing TargetType
        static string? GetTargetTypeName(INode node)
			=> (((ElementNode)node).Properties[new XmlName("", "TargetType")] as ValueNode)?.Value as string;


        var parts = ((string)node.Value).Split('.');
        if (parts.Length == 1)
        {
            string? typeName = null;
            var parent = node.Parent?.Parent as IElementNode ?? (node.Parent?.Parent as IListNode)?.Parent as IElementNode;
            if (   node.Parent is ElementNode { XmlType.NamespaceUri: XamlParser.MauiUri }
                && (   node.Parent is ElementNode { XmlType.Name: "Setter" }
                    || node.Parent is ElementNode { XmlType.Name: "PropertyCondition" }))
            {
                if (parent!.XmlType.NamespaceUri == XamlParser.MauiUri &&
                    (parent.XmlType.Name == "Trigger"
                        || parent.XmlType.Name == "DataTrigger"
                        || parent.XmlType.Name == "MultiTrigger"
                        || parent.XmlType.Name == "Style"))
                {
                    typeName = GetTargetTypeName(parent);
                }
                else if (parent.XmlType.NamespaceUri == XamlParser.MauiUri && parent.XmlType.Name == "VisualState")
                {
                    typeName = FindTypeNameForVisualState(parent, context, node);
                }
            }
            else if ((node.Parent as ElementNode)?.XmlType.NamespaceUri == XamlParser.MauiUri && (node.Parent as ElementNode)?.XmlType.Name == "Trigger")
            {
                typeName = GetTargetTypeName(node.Parent!);
            }
            var typeSymbol = XmlTypeExtensions.GetTypeSymbol(typeName!, context.ReportDiagnostic, context.Compilation, context.XmlnsCache, node);
            var propertyName = parts[0];
            return typeSymbol!.GetBindableProperty("", ref propertyName, out _, context, node)!;
        }
        else if (parts.Length == 2)
        {
            var typeSymbol = XmlTypeExtensions.GetTypeSymbol(parts[0], context.ReportDiagnostic, context.Compilation, context.XmlnsCache, node);
            string propertyName = parts[1];
            return typeSymbol!.GetBindableProperty("", ref propertyName, out _, context, node)!;
        }
        else
        {
            throw new Exception();
            // FIXME context.ReportDiagnostic
        }
    }

    static string? FindTypeNameForVisualState(IElementNode parent, SourceGenContext context, IXmlLineInfo lineInfo)
    {
        //1. parent is VisualState, don't check that

        //2. check that the VS is in a VSG
        if (!(parent.Parent is IElementNode target) || target.XmlType.NamespaceUri != XamlParser.MauiUri || target.XmlType.Name != "VisualStateGroup")
            //FIXME context.RreportDiagnostic
            throw new Exception($"Expected VisualStateGroup but found {parent.Parent}");

        //3. if the VSG is in a VSGL, skip that as it could be implicit
        if (target.Parent is ListNode
            || ((target.Parent as IElementNode)?.XmlType.NamespaceUri == XamlParser.MauiUri
                && (target.Parent as IElementNode)?.XmlType.Name == "VisualStateGroupList"))
            target = (IElementNode)target.Parent.Parent;
        else
            target = (IElementNode)target.Parent;

        //4. target is now a Setter in a Style, or a VE
        if (target.XmlType.NamespaceUri == XamlParser.MauiUri && target.XmlType.Name == "Setter")
            return ((target?.Parent as IElementNode)?.Properties[new XmlName("", "TargetType")] as ValueNode)?.Value as string;
        else
            return target.XmlType.Name;
    }
}