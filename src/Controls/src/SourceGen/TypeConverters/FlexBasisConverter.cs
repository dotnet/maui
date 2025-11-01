using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class FlexBasisConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["FlexBasis", "Microsoft.Maui.Layouts.FlexBasis"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
			{
				var flexBasisType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexBasis")!;
				return $"{flexBasisType.ToFQDisplayString()}.Auto";
			}

			if (value.EndsWith("%", StringComparison.OrdinalIgnoreCase)
				&& float.TryParse(value.Substring(0, value.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out float relflex))
			{
				var flexBasisRelType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexBasis")!;
				return $"new {flexBasisRelType.ToFQDisplayString()}({FormatInvariant(relflex / 100)}, true)";
			}

			if (float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out float flex))
			{
				var flexBasisAbsType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Layouts.FlexBasis")!;
				return $"new {flexBasisAbsType.ToFQDisplayString()}({FormatInvariant(flex)}, false)";
			}
		}

		context.ReportConversionFailed(xmlLineInfo, value, Descriptors.FlexBasisConversionFailed);
		return "default";
	}
}