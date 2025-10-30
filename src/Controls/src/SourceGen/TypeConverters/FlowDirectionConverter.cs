using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class FlowDirectionConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "FlowDirection", "Microsoft.Maui.FlowDirection" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrWhiteSpace(value))
		{
			value = value.Trim();

			if (value.Equals("ltr", StringComparison.OrdinalIgnoreCase))
			{
				var flowDirectionType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.FlowDirection")!;
				return $"{flowDirectionType.ToFQDisplayString()}.LeftToRight";
			}

			if (value.Equals("rtl", StringComparison.OrdinalIgnoreCase))
			{
				var flowDirectionRtlType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.FlowDirection")!;
				return $"{flowDirectionRtlType.ToFQDisplayString()}.RightToLeft";
			}
			if (value.Equals("inherit", StringComparison.OrdinalIgnoreCase))
			{
				var flowDirectionMatchType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.FlowDirection")!;
				return $"{flowDirectionMatchType.ToFQDisplayString()}.MatchParent";
			}

			// Fallback to enum conversion
			var enumConverter = new EnumConverter();
			return enumConverter.Convert(value, node, toType, context);
		}

		context.ReportConversionFailed(xmlLineInfo, value, Descriptors.FlowDirectionConversionFailed);
		return "default";
	}
}