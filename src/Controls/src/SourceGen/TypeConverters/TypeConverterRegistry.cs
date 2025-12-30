using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

/// <summary>
/// Registry for type converters used in source generation.
/// </summary>
internal static class TypeConverterRegistry
{
	private static readonly Dictionary<string, ISGTypeConverter> _converters = new(StringComparer.OrdinalIgnoreCase);
	private static readonly Lazy<bool> _initialized = new(RegisterDefaults);

	static TypeConverterRegistry()
	{
		// Ensure initialization happens
		_ = _initialized.Value;
	}

	/// <summary>
	/// Registers a converter for its supported types.
	/// </summary>
	public static void Register(ISGTypeConverter converter)
	{
		foreach (var type in converter.SupportedTypes)
		{
			_converters[type] = converter;
		}
	}

	/// <summary>
	/// Gets a converter for the specified type name.
	/// </summary>
	public static ISGTypeConverter? GetConverter(string typeName) =>
		_converters.TryGetValue(typeName, out var converter) ? converter : null;

	/// <summary>
	/// Converts a value using the appropriate registered converter.
	/// </summary>
	public static string Convert(string typeName, string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var converter = GetConverter(typeName);
		return converter?.Convert(value, node, toType, writer, context, parentVar) ?? "default";
	}

	/// <summary>
	/// Registers all default converters.
	/// </summary>
	private static bool RegisterDefaults()
	{
		Register(new ColorConverter());
		Register(new RectConverter());
		Register(new PointConverter());
		Register(new ThicknessConverter());
		Register(new CornerRadiusConverter());
		Register(new EasingConverter());
		Register(new EnumConverter());
		Register(new FlexBasisConverter());
		Register(new FontSizeConverter());
		Register(new FlowDirectionConverter());
		Register(new GridLengthConverter());
		Register(new ColumnDefinitionCollectionConverter());
		Register(new RowDefinitionCollectionConverter());
		Register(new ImageSourceConverter());
		Register(new ListStringConverter());
		Register(new PointCollectionConverter());
		Register(new PathGeometryConverter());
		Register(new StrokeShapeConverter());
		Register(new LayoutOptionsConverter());
		Register(new ConstraintConverter());
		Register(new BindablePropertyConverter());
		Register(new RDSourceConverter());
		Register(new TypeTypeConverter());

		return true;
	}
}