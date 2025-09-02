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
				return $"global::Microsoft.Maui.GridLength.Star";
			}
			else if (value.EndsWith("*", StringComparison.OrdinalIgnoreCase))
			{
				if (double.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out double val))
				{
					return $"new global::Microsoft.Maui.GridLength({FormatInvariant(val)}, global::Microsoft.Maui.GridUnitType.Star)";
				}
			}
			else if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
			{
				return $"global::Microsoft.Maui.GridLength.Auto";
			}
			else if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double val))
			{
				return $"new global::Microsoft.Maui.GridLength({FormatInvariant(val)}, global::Microsoft.Maui.GridUnitType.Absolute)";
			}
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.GridLengthConversionFailed);
		return "default";
	}
}