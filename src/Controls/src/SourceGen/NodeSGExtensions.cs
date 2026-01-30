using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;

namespace Microsoft.Maui.Controls.SourceGen;

using static LocationHelpers;
static class NodeSGExtensions
{
	public delegate string ConverterDelegate(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null);

	public delegate ILocalValue GetNodeValueDelegate(INode node, ITypeSymbol toType);

	public delegate bool ProvideValueDelegate(ElementNode markupNode, IndentedTextWriter writer, SourceGenContext context, GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value);

	// Lazy converter factory function
	static ConverterDelegate CreateLazyConverter<T>() where T : ISGTypeConverter, new() =>
		(value, node, toType, writer, context, parentVar) => 
			lazyConverters.GetOrAdd(typeof(T), _ => new T()).Convert(value, node, toType, writer, context, parentVar);

	static readonly ConcurrentDictionary<Type, ISGTypeConverter> lazyConverters = new();
	static readonly ConcurrentDictionary<string, ISGTypeConverter> lazyRegistryConverters = new();

	// Lazy registry-based converter function (for non-source-gen converters)
	static ConverterDelegate CreateLazyRegistryConverter(string typeName) =>
		(value, node, toType, writer, context, parentVar) => 
		{
			var converter = lazyRegistryConverters.GetOrAdd(typeName, name => TypeConverterRegistry.GetConverter(name)!);
			return converter?.Convert(value, node, toType, writer, context, parentVar) ?? "default";
		};

	// Lazy enum converter
	static readonly Lazy<EnumConverter> _lazyEnumConverter = new(() => new EnumConverter());

	static string ConvertEnum(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null) =>
		_lazyEnumConverter.Value.Convert(value, node, toType, writer, context, parentVar);

	public static Dictionary<ITypeSymbol, (ConverterDelegate converter, ITypeSymbol returnType)> GetKnownSGTypeConverters(SourceGenContext context)
		=> context.knownSGTypeConverters ??= new(SymbolEqualityComparer.Default)
	{
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Converters.RectTypeConverter")!, (CreateLazyConverter<RectConverter>(), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Rect")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Converters.ColorTypeConverter")!, (CreateLazyConverter<ColorConverter>(), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Converters.PointTypeConverter")!, (CreateLazyConverter<PointConverter>(), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Point")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.CornerRadiusTypeConverter")!, (CreateLazyRegistryConverter("Microsoft.Maui.CornerRadius"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.CornerRadius")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.EasingTypeConverter")!, (CreateLazyRegistryConverter("Microsoft.Maui.Easing"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Easing")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexJustifyTypeConverter")!, (ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexJustify")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexDirectionTypeConverter")!, (ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexDirection")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexAlignContentTypeConverter")!, (ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexAlignContent")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexAlignItemsTypeConverter")!, (ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexAlignItems")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexAlignSelfTypeConverter")!, (ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexAlignSelf")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexWrapTypeConverter")!, (ConvertEnum, context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexWrap")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.FlexBasisTypeConverter")!, (CreateLazyRegistryConverter("Microsoft.Maui.Layouts.FlexBasis"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexBasis")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.GridLengthTypeConverter")!, (CreateLazyConverter<GridLengthConverter>(), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.GridLength")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Converters.ThicknessTypeConverter")!, (CreateLazyConverter<ThicknessConverter>(), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Thickness")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ColumnDefinitionCollectionTypeConverter")!, (CreateLazyRegistryConverter("Microsoft.Maui.Controls.ColumnDefinitionCollection"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ColumnDefinitionCollection")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindablePropertyConverter")!, (CreateLazyConverter<BindablePropertyConverter>(), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindableProperty")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.FlowDirectionConverter")!, (CreateLazyRegistryConverter("Microsoft.Maui.FlowDirection"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.FlowDirection")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.FontSizeConverter")!, (CreateLazyRegistryConverter("System.Double"), context.Compilation.GetTypeByMetadataName("System.Double")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.GridLengthTypeConverter")!, (CreateLazyConverter<GridLengthConverter>(), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.GridLength")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ImageSourceConverter")!, (CreateLazyRegistryConverter("Microsoft.Maui.Controls.ImageSource"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ImageSource")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.LayoutOptionsConverter")!, (CreateLazyRegistryConverter("Microsoft.Maui.Controls.LayoutOptions"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.LayoutOptions")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ListStringTypeConverter")!, (CreateLazyRegistryConverter("System.Collections.Generic.IList`1[System.String]"), context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1")!.Construct(context.Compilation.GetSpecialType(SpecialType.System_String))!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary+RDSourceTypeConverter")!, (CreateLazyRegistryConverter("System.Uri"), context.Compilation.GetTypeByMetadataName("System.Uri")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.RowDefinitionCollectionTypeConverter")!, (CreateLazyRegistryConverter("Microsoft.Maui.Controls.RowDefinitionCollection"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.RowDefinitionCollection")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.PointCollectionConverter")!, (CreateLazyRegistryConverter("Microsoft.Maui.Controls.PointCollection"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.PointCollection")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.TypeTypeConverter")!, (CreateLazyConverter<TypeTypeConverter>(), context.Compilation.GetTypeByMetadataName("System.Type")!) },
		{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Compatibility.ConstraintTypeConverter")!, (CreateLazyRegistryConverter("Microsoft.Maui.Controls.Compatibility.Constraint"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Compatibility.Constraint")!) },
        
        // TODO: PathFigureCollectionConverter (used for PathGeometry and StrokeShape) is very complex, skipping for now, apart from that one all other shapes work though
			//{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.PathGeometryConverter")!, (CreateRegistryConverter("Microsoft.Maui.Controls.Shapes.Geometry"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Geometry")!) },
			//{ context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.StrokeShapeTypeConverter")!, (CreateRegistryConverter("Microsoft.Maui.Controls.Shapes.Shape"), context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.Shape")!) },
		};

	public static Dictionary<ITypeSymbol, IKnownMarkupValueProvider> GetKnownValueProviders(SourceGenContext context)
		=> context.knownSGValueProviders ??= new(SymbolEqualityComparer.Default)
	{
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Setter")!, new SetterValueProvider()},
	};



	// These markup extensions can provide values early since the input is just a string literal
	public static Dictionary<ITypeSymbol, ProvideValueDelegate> GetKnownEarlyMarkupExtensions(SourceGenContext context)
		=> context.knownSGEarlyMarkupExtensions ??= new(SymbolEqualityComparer.Default)
	{
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.NullExtension")!, KnownMarkups.ProvideValueForNullExtension},
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.StaticExtension")!, KnownMarkups.ProvideValueForStaticExtension},
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.DynamicResourceExtension")!, KnownMarkups.ProvideValueForDynamicResourceExtension},
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.StyleSheetExtension")!, KnownMarkups.ProvideValueForStyleSheetExtension},
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.TypeExtension")!, KnownMarkups.ProvideValueForTypeExtension},
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.DataTemplateExtension")!, KnownMarkups.ProvideValueForDataTemplateExtension},
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.RelativeSourceExtension")!, KnownMarkups.ProvideValueForRelativeSourceExtension},
	};

	// These markup extensions can only provide values late once the properties have their final values
	public static Dictionary<ITypeSymbol, ProvideValueDelegate> GetKnownLateMarkupExtensions(SourceGenContext context)
		=> context.knownSGLateMarkupExtensions ??= new(SymbolEqualityComparer.Default)
	{
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.BindingExtension")!, KnownMarkups.ProvideValueForBindingExtension},
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.TemplateBindingExtension")!, KnownMarkups.ProvideValueForTemplateBindingExtension},
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.ReferenceExtension")!, KnownMarkups.ProvideValueForReferenceExtension},
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.StaticResourceExtension")!, KnownMarkups.ProvideValueForStaticResourceExtension},
		{context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.AppThemeBindingExtension")!, KnownMarkups.ProvideValueForAppThemeBindingExtension},
	};

	public static bool TryGetPropertyName(this INode node, INode parentNode, out XmlName name)
	{
		name = default;
		if (parentNode is not ElementNode parentElement)
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

	public static bool IsResourceDictionary(this ElementNode node, SourceGenContext context)
		=> context.Variables.TryGetValue(node, out var variable) && variable.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary")!, context);

	public static bool CanConvertTo(this ValueNode valueNode, IFieldSymbol bpFieldSymbol, SourceGenContext context, out ITypeSymbol? converter)
	{
		var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter(context);
		converter = typeandconverter?.converter;
		if (typeandconverter == null)
			return false;
		return valueNode.CanConvertTo(typeandconverter.Value.type, typeandconverter.Value.converter, context);
	}
	public static bool CanConvertTo(this ValueNode valueNode, IFieldSymbol bpFieldSymbol, SourceGenContext context)
		=> CanConvertTo(valueNode, bpFieldSymbol, context, out _);

	public static bool CanConvertTo(this ValueNode valueNode, IPropertySymbol property, SourceGenContext context, out ITypeSymbol? converter)
	{
		List<AttributeData> attributes = [
			.. property.GetAttributes().ToList(),
			.. property.Type.GetAttributes()
		];

		converter = attributes.FirstOrDefault(ad => ad.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")?.ConstructorArguments[0].Value as ITypeSymbol;
		return valueNode.CanConvertTo(property.Type, converter, context);
	}

	public static bool CanConvertTo(this ValueNode valueNode, IPropertySymbol property, SourceGenContext context)
			=> CanConvertTo(valueNode, property, context, out _);

	public static bool CanConvertTo(this ValueNode valueNode, ITypeSymbol toType, ITypeSymbol? converter, SourceGenContext context)
	{
		var stringValue = (string)valueNode.Value;

		//if there's a typeconverter, assume we can convert
		if (converter is not null && stringValue is not null)
			return true;

		if (   toType.NullableAnnotation == NullableAnnotation.Annotated
			&& toType.SpecialType == SpecialType.None
			&& ((INamedTypeSymbol)toType).TypeArguments.Length == 1)
		{
			toType = ((INamedTypeSymbol)toType).TypeArguments[0];
		}

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

	public static string ConvertTo(this ValueNode valueNode, IFieldSymbol bpFieldSymbol, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter(context);
		if (typeandconverter == null)
			return string.Empty;
		return valueNode.ConvertTo(typeandconverter.Value.type, typeandconverter.Value.converter, writer, context, parentVar);
	}

	public static string ConvertTo(this ValueNode valueNode, IPropertySymbol property, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		List<AttributeData> attributes = [
			.. property.GetAttributes().ToList(),
			.. property.Type.GetAttributes()
		];

		var typeConverter = attributes.FirstOrDefault(ad => ad.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")?.ConstructorArguments[0].Value as ITypeSymbol;
		return valueNode.ConvertTo(property.Type, typeConverter, writer, context, parentVar);
	}

	public static string ConvertTo(this ValueNode valueNode, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		List<AttributeData> attributes = [.. toType.GetAttributes()];

		var typeConverter = attributes.FirstOrDefault(ad => ad.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")?.ConstructorArguments[0].Value as ITypeSymbol;

		return valueNode.ConvertTo(toType, typeConverter, writer, context, parentVar);
	}

	public static string ConvertTo(this ValueNode valueNode, ITypeSymbol toType, ITypeSymbol? typeConverter, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var valueString = valueNode.Value as string ?? string.Empty;

		if (typeConverter is not null && GetKnownSGTypeConverters(context).TryGetValue(typeConverter, out var converterAndReturnType))
		{
			var returntype = converterAndReturnType.returnType;
			if (!context.Compilation.HasImplicitConversion(returntype, toType))
				//this could be left to the compiler to figure, but I've yet to find a way to test the compiler...
				//FIXME: better error message
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberResolution, LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)valueNode, valueString), $"Cannot convert {returntype} to {toType}"));
			return converterAndReturnType.converter.Invoke(valueString, valueNode, toType, writer, context, parentVar);
		}

		if (typeConverter is not null)
			return valueNode.ConvertWithConverter(typeConverter, toType, writer, context, parentVar);

		return ValueForLanguagePrimitive(valueString, toType, context, valueNode);
	}

	public static string ValueForLanguagePrimitive(string valueString, ITypeSymbol toType, SourceGenContext context, IXmlLineInfo lineInfo)
	{
#pragma warning disable RS0030 // Do not use banned APIs
		void reportDiagnostic()
			=> context.ReportDiagnostic(Diagnostic.Create(Descriptors.ConversionFailed, LocationHelpers.LocationCreate(context.ProjectItem.RelativePath!, lineInfo, valueString), valueString, toType.ToDisplayString()));
#pragma warning restore RS0030 // Do not use banned APIs

		if (   toType.NullableAnnotation == NullableAnnotation.Annotated
			&& toType.SpecialType == SpecialType.None
			&& ((INamedTypeSymbol)toType).TypeArguments.Length == 1)
		{
			toType = ((INamedTypeSymbol)toType).TypeArguments[0];
		}

		if (toType.SpecialType == SpecialType.System_SByte)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (sbyte.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var sbyteValue))
				return SymbolDisplay.FormatPrimitive(sbyteValue, true, false);
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_Byte)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (byte.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var byteValue))
				return SymbolDisplay.FormatPrimitive(byteValue, true, false);
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_Int16)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (short.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var shortValue))
				return SymbolDisplay.FormatPrimitive(shortValue, true, false);
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_UInt16)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (short.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var ushortValue))
				return SymbolDisplay.FormatPrimitive(ushortValue, true, false);
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_Int32)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (int.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var intValue))
				return SymbolDisplay.FormatPrimitive(intValue, true, false);
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_UInt32)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (uint.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var uintValue))
				return $"{SymbolDisplay.FormatPrimitive(uintValue, true, false)}U";
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_Int64)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (long.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var longValue))
				return $"{SymbolDisplay.FormatPrimitive(longValue, true, false)}L";
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_UInt64)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (ulong.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var ulongValue))
				return $"{SymbolDisplay.FormatPrimitive(ulongValue, true, false)}UL";
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_Single)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (float.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var floatValue))
			{
				// Handle special values that don't need a suffix
				if (float.IsNaN(floatValue))
					return "float.NaN";
				if (float.IsPositiveInfinity(floatValue))
					return "float.PositiveInfinity";
				if (float.IsNegativeInfinity(floatValue))
					return "float.NegativeInfinity";
				// Regular values need the F suffix
				return $"{SymbolDisplay.FormatPrimitive(floatValue, true, false)}F";
			}
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_Double)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (double.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var doubleValue))
			{
				// Handle special values that don't need a suffix
				if (double.IsNaN(doubleValue))
					return "double.NaN";
				if (double.IsPositiveInfinity(doubleValue))
					return "double.PositiveInfinity";
				if (double.IsNegativeInfinity(doubleValue))
					return "double.NegativeInfinity";
				// Regular values need the D suffix
				return $"{SymbolDisplay.FormatPrimitive(doubleValue, true, false)}D";
			}
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_Boolean)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (bool.TryParse(valueString, out var boolValue))
				return SymbolDisplay.FormatPrimitive(boolValue, true, false);
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_Char)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
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
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (DateTime.TryParse(valueString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeValue))
				return $"new global::System.DateTime({dateTimeValue.Ticks})";
			else
				reportDiagnostic();
		}
		if (toType.SpecialType == SpecialType.System_Decimal)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (decimal.TryParse(valueString, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
				return $"{SymbolDisplay.FormatPrimitive(decimalValue, true, false)}M";
			else
				reportDiagnostic();
		}
		if (toType.TypeKind == TypeKind.Enum)
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			return string.Join(" | ", valueString.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(v => $"{toType.ToFQDisplayString()}.{v.Trim()}"));
		}

		if (toType.Equals(context.Compilation.GetTypeByMetadataName("System.TimeSpan")!, SymbolEqualityComparer.Default))
		{
			if (string.IsNullOrEmpty(valueString))
				return "default";
			if (TimeSpan.TryParse(valueString, CultureInfo.InvariantCulture, out var timeSpanValue))
				return $"new global::System.TimeSpan({timeSpanValue.Ticks})";
			else
				reportDiagnostic();
		}
		if (toType.Equals(context.Compilation.GetTypeByMetadataName("System.Uri")!, SymbolEqualityComparer.Default))
			return $"new global::System.Uri(@\"{valueString}\", global::System.UriKind.RelativeOrAbsolute)";

		//default
		return SymbolDisplay.FormatLiteral(valueString, true);
	}

	public static string ConvertWithConverter(this ValueNode valueNode, ITypeSymbol typeConverter, ITypeSymbol targetType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var valueString = valueNode.Value as string ?? string.Empty;
		//TODO check if there's a SourceGen version of the converter
		if (typeConverter.Implements(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.IExtendedTypeConverter")!))
		{

			var (acceptEmptyServiceProvider, requiredServices) = typeConverter.GetServiceProviderAttributes(context);
			if (!acceptEmptyServiceProvider)
			{
				var serviceProvider = valueNode.GetOrCreateServiceProvider(writer, context, requiredServices);
				if (targetType.IsReferenceType || targetType.NullableAnnotation == NullableAnnotation.Annotated)
					return $"((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new {typeConverter.ToFQDisplayString()}()).ConvertFromInvariantString(\"{valueString}\", {serviceProvider.ValueAccessor}) as {targetType.ToFQDisplayString()}";
				else
					return $"({targetType.ToFQDisplayString()})((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new {typeConverter.ToFQDisplayString()}()).ConvertFromInvariantString(\"{valueString}\", {serviceProvider.ValueAccessor})";
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

		return variable.Type.IsValueProvider( context, out returnType, out iface, out acceptEmptyServiceProvider, out requiredServices);
	}


	public static void ProvideValue(this ElementNode node, IndentedTextWriter writer, SourceGenContext context, ITypeSymbol returnType, ITypeSymbol valueProviderFace, bool acceptEmptyServiceProvider, ImmutableArray<ITypeSymbol>? requiredServices)
	{
		var valueProviderVariable = context.Variables[node];
		var variableName = NamingHelpers.CreateUniqueVariableName(context, returnType);

		//if it require a serviceprovider, create one
		if (!acceptEmptyServiceProvider)
		{
			var serviceProviderVar = node.GetOrCreateServiceProvider(writer, context, requiredServices);
			writer.WriteLine($"var {variableName} = ({returnType.ToFQDisplayString()})(({valueProviderFace.ToFQDisplayString()}){valueProviderVariable.ValueAccessor}).ProvideValue({serviceProviderVar.ValueAccessor});");
			context.Variables[node] = new LocalVariable(returnType, variableName);
			node.RegisterSourceInfo(context, context.Writer, update: false);
		}
		else
		{
			writer.WriteLine($"var {variableName} = ({returnType.ToFQDisplayString()})(({valueProviderFace.ToFQDisplayString()}){valueProviderVariable.ValueAccessor}).ProvideValue(null);");
			context.Variables[node] = new LocalVariable(returnType, variableName);
			node.RegisterSourceInfo(context, context.Writer, update: false);
		}
	}

	public static bool TryProvideValue(this ElementNode node, IndentedTextWriter writer, SourceGenContext context)
		=> TryProvideValue(node, writer, context, null);

	public static bool TryProvideValue(this ElementNode node, IndentedTextWriter writer, SourceGenContext context, GetNodeValueDelegate? getNodeValue)
	{
		if (!context.Variables.TryGetValue(node, out var variable))
			return false;

		if (variable.Type is null)
			return false;

		if (GetKnownLateMarkupExtensions(context).TryGetValue(variable.Type, out var provideValue)
			&& provideValue.Invoke(node, writer, context, getNodeValue, out var returnType0, out var value))
		{
			var variableName = NamingHelpers.CreateUniqueVariableName(context, returnType0 ?? context.Compilation.ObjectType);
			context.Writer.WriteLine($"var {variableName} = {value};");
			context.Variables[node] = new LocalVariable(returnType0 ?? context.Compilation.ObjectType, variableName);
			node.RegisterSourceInfo(context, context.Writer, update: false);

			return true;
		}

		if (GetKnownValueProviders(context).TryGetValue(variable.Type, out var valueProvider)
			&& valueProvider.TryProvideValue(node, writer, context, getNodeValue, out returnType0, out value))
		{
			var variableName = NamingHelpers.CreateUniqueVariableName(context, returnType0 ?? context.Compilation.ObjectType);
			context.Writer.WriteLine($"var {variableName} = {value};");
			context.Variables[node] = new LocalVariable(returnType0 ?? context.Compilation.ObjectType, variableName);
			node.RegisterSourceInfo(context, context.Writer, update: false);

			return true;
		}

		if (!node.IsValueProvider(context, out var returnType, out var iface, out var acceptEmptyServiceProvider, out var requiredServices))
			return false;

		node.ProvideValue(context.Writer, context, returnType, iface!, acceptEmptyServiceProvider, requiredServices);
		return true;
	}

	public static void RegisterSourceInfo(this INode node, SourceGenContext context, IndentedTextWriter writer, bool update = true)
	{
		if (!context.ProjectItem.EnableDiagnostics)
			return;

		if (!context.Variables.TryGetValue(node, out var variable))
			return;

		var assembly = context.Compilation.Assembly.Name;
		var filePath = context.ProjectItem.RelativePath;
		var lineInfo = node as IXmlLineInfo;

		if (!update)
		{
			writer.WriteLine($"if (global::Microsoft.Maui.VisualDiagnostics.GetSourceInfo({variable.ValueAccessor}!) == null)");
			writer.Indent++;
		}
		// on other inflators, we do not replace path separator, so keep the bug for compat
		// filePath = new UriBuilder() { Path = filePath }.Path; // ensure the file use the right separator
		writer.WriteLine($"global::Microsoft.Maui.VisualDiagnostics.RegisterSourceInfo({variable.ValueAccessor}!, new global::System.Uri(@\"{filePath};assembly={assembly}\", global::System.UriKind.Relative), {lineInfo?.LineNumber ?? -1}, {lineInfo?.LinePosition ?? -1});");
		if (!update)
			writer.Indent--;
	}

	public static (IFieldSymbol?, IPropertySymbol?) GetFieldOrBP(ITypeSymbol owner, XmlName name, SourceGenContext context)
	{
		var localName = name.LocalName;
		var bpFieldSymbol = !string.IsNullOrEmpty(localName) ? owner.GetBindableProperty(name.NamespaceURI, ref localName, out _, context, null) : null;
		if (bpFieldSymbol != null && !context.Compilation.IsSymbolAccessibleWithin(bpFieldSymbol, context.RootType))
			bpFieldSymbol = null;
		var property = owner.GetAllProperties(name.LocalName, context).FirstOrDefault();
		if (property != null && !context.Compilation.IsSymbolAccessibleWithin(property, context.RootType))
			property = null;
		return (bpFieldSymbol, property);
	}
	
	public static IFieldSymbol GetBindableProperty(this ValueNode node, SourceGenContext context)
	{
		static ITypeSymbol? GetTargetTypeSymbol(INode node, SourceGenContext context)
		{
			var ttnode = (node as ElementNode)?.Properties[new XmlName("", "TargetType")];
			//it's either a value
			if (ttnode is ValueNode { Value: string tt })
				return XmlTypeExtensions.GetTypeSymbol(tt, context, node);
			//or a x:Type that we parsed earlier
			if (context.Types.TryGetValue(ttnode!, out var typeSymbol))
				return typeSymbol;
			//FIXME: report diagnostic on missing TargetType
			return null;
		}

		var parts = ((string)node.Value).Split('.');
		if (parts.Length == 1)
		{
			ITypeSymbol? typeSymbol = null;
			var parent = node.Parent?.Parent as ElementNode ?? (node.Parent?.Parent as IListNode)?.Parent as ElementNode;
			if ((node.Parent as ElementNode)!.XmlType!.IsOfAnyType("Setter", "PropertyCondition"))
			{
				if (parent!.XmlType.IsOfAnyType("Trigger", "DataTrigger", "MultiTrigger", "Style"))
					typeSymbol = GetTargetTypeSymbol(parent, context);
				else if (parent.XmlType.IsOfAnyType("VisualState"))
					typeSymbol = FindTypeSymbolForVisualState(parent, context, node);
			}
			else if ((node.Parent as ElementNode)!.XmlType!.IsOfAnyType("Trigger"))
				typeSymbol = GetTargetTypeSymbol(node.Parent!, context);

			var propertyName = parts[0];
			return typeSymbol!.GetBindableProperty("", ref propertyName, out _, context, node)!;
		}
		else if (parts.Length == 2)
		{
			var typeSymbol = XmlTypeExtensions.GetTypeSymbol(parts[0], context, node);
			string propertyName = parts[1];
			return typeSymbol!.GetBindableProperty("", ref propertyName, out _, context, node)!;
		}
		else
		{
			throw new Exception();
			// FIXME context.ReportDiagnostic
		}
	}

	static ITypeSymbol? FindTypeSymbolForVisualState(ElementNode parent, SourceGenContext context, IXmlLineInfo lineInfo)
	{
		//1. parent is VisualState, don't check that

		//2. check that the VS is in a VSG
		if (!(parent.Parent is ElementNode target) || !target.XmlType.IsOfAnyType("VisualStateGroup"))
			throw new Exception($"Expected VisualStateGroup but found {parent.Parent}");

		//3. if the VSG is in a VSGL, skip that as it could be implicit
		if (   target.Parent is ListNode
			|| (target.Parent as ElementNode)!.XmlType!.IsOfAnyType( "VisualStateGroupList"))
			target = (ElementNode)target.Parent.Parent;
		else
			target = (ElementNode)target.Parent;

		XmlType? typeName = null;
		//4. target is now a Setter in a Style, or a VE
		if (target.XmlType.IsOfAnyType("Setter"))
		{
			var targetType = ((target?.Parent as ElementNode)?.Properties[new XmlName("", "TargetType")] as ValueNode)?.Value as string;
			typeName = TypeArgumentsParser.ParseSingle(targetType, parent.NamespaceResolver, lineInfo);
		}
		else
			typeName = target.XmlType;

		return typeName!.GetTypeSymbol(context);
	}

	public static bool RepresentsType(this INode node, string namespaceUri, string name)
		=> node is ElementNode elementNode && elementNode.XmlType.RepresentsType(namespaceUri, name);
}