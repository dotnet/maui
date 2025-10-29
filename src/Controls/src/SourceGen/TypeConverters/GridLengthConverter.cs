using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class GridLengthConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "GridLength", "Microsoft.Maui.GridLength" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			if (value.Equals("*", StringComparison.OrdinalIgnoreCase))
			{
				var gridLengthType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.GridLength")!;
				return $"{gridLengthType.ToFQDisplayString()}.Star";
			}
			else if (value.EndsWith("*", StringComparison.OrdinalIgnoreCase))
			{
				if (double.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out double val))
				{
					var gridLengthType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.GridLength")!;
					var gridUnitType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.GridUnitType")!;
					return $"new {gridLengthType.ToFQDisplayString()}({FormatInvariant(val)}, {gridUnitType.ToFQDisplayString()}.Star)";
				}
			}
			else if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
			{
				var gridLengthType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.GridLength")!;
				return $"{gridLengthType.ToFQDisplayString()}.Auto";
			}
			else if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double val))
			{
				var gridLengthType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.GridLength")!;
				var gridUnitType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.GridUnitType")!;
				return $"new {gridLengthType.ToFQDisplayString()}({FormatInvariant(val)}, {gridUnitType.ToFQDisplayString()}.Absolute)";
			}
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.GridLengthConversionFailed);
		return "default";
	}
}