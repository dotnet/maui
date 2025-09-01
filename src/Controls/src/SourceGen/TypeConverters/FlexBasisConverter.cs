using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class FlexBasisConverter : BaseTypeConverter
{
	public override IEnumerable<string> SupportedTypes => new[] { "FlexBasis", "Microsoft.Maui.Layouts.FlexBasis" };

	public override string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
				return $"global::Microsoft.Maui.Layouts.FlexBasis.Auto";

			if (value.EndsWith("%", StringComparison.OrdinalIgnoreCase)
				&& float.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out float relflex))
				return $"new global::Microsoft.Maui.Layouts.FlexBasis({FormatInvariant(relflex / 100)}, true)";

			if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out float flex))
				return $"new global::Microsoft.Maui.Layouts.FlexBasis({FormatInvariant(flex)}, false)";
		}

		ReportConversionFailed(context, xmlLineInfo, value, Descriptors.FlexBasisConversionFailed);
		return "default";
	}
}