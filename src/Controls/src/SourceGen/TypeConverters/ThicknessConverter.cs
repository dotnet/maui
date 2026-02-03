using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class ThicknessConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["Thickness", "Microsoft.Maui.Thickness"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		// IMPORTANT! Update ThicknessTypeDesignConverter.IsValid if making changes here
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			if (value.Contains(","))
			{ //Xaml
				var thickness = value.Split([',']);
				switch (thickness.Length)
				{
					case 2:
						if (TryParseDouble(thickness[0], out double h)
							&& TryParseDouble(thickness[1], out double v))
						{
							var thicknessType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Thickness")!;
							return $"new {thicknessType.ToFQDisplayString()}({FormatDouble(h)}, {FormatDouble(v)})";
						}
						break;
					case 4:
						if (TryParseDouble(thickness[0], out double l)
							&& TryParseDouble(thickness[1], out double t)
							&& TryParseDouble(thickness[2], out double r)
							&& TryParseDouble(thickness[3], out double b))
						{
							var thicknessType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Thickness")!;
							return $"new {thicknessType.ToFQDisplayString()}({FormatDouble(l)}, {FormatDouble(t)}, {FormatDouble(r)}, {FormatDouble(b)})";
						}
						break;
				}
			}
			else
			{ //single uniform thickness
				if (TryParseDouble(value, out double l))
				{
					var thicknessType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Thickness")!;
					return $"new {thicknessType.ToFQDisplayString()}({FormatDouble(l)})";
				}
			}
		}

		context.ReportConversionFailed(xmlLineInfo, value, Descriptors.ThicknessConversionFailed);
		return "default";
	}

	/// <summary>
	/// Tries to parse a double value, including special values like NaN, Infinity, -Infinity.
	/// </summary>
	static bool TryParseDouble(string value, out double result)
	{
		value = value.Trim();

		// Handle special values that NumberStyles.Number doesn't parse
		if (value.Equals("NaN", System.StringComparison.OrdinalIgnoreCase))
		{
			result = double.NaN;
			return true;
		}
		if (value.Equals("Infinity", System.StringComparison.OrdinalIgnoreCase) ||
			value.Equals("+Infinity", System.StringComparison.OrdinalIgnoreCase))
		{
			result = double.PositiveInfinity;
			return true;
		}
		if (value.Equals("-Infinity", System.StringComparison.OrdinalIgnoreCase))
		{
			result = double.NegativeInfinity;
			return true;
		}

		return double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
	}

	/// <summary>
	/// Formats a double value for C# code generation, handling special values.
	/// </summary>
	static string FormatDouble(double value)
	{
		if (double.IsNaN(value))
			return "double.NaN";
		if (double.IsPositiveInfinity(value))
			return "double.PositiveInfinity";
		if (double.IsNegativeInfinity(value))
			return "double.NegativeInfinity";

		return FormatInvariant(value);
	}
}