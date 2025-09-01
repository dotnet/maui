using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class FlowDirectionConverter : BaseTypeConverter
{
	public override IEnumerable<string> SupportedTypes => new[] { "FlowDirection", "Microsoft.Maui.FlowDirection" };

	public override string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrWhiteSpace(value))
		{
			value = value.Trim();

			if (value.Equals("ltr", StringComparison.OrdinalIgnoreCase))
				return "global::Microsoft.Maui.FlowDirection.LeftToRight";

			if (value.Equals("rtl", StringComparison.OrdinalIgnoreCase))
				return "global::Microsoft.Maui.FlowDirection.RightToLeft";

			if (value.Equals("inherit", StringComparison.OrdinalIgnoreCase))
				return "global::Microsoft.Maui.FlowDirection.MatchParent";

			// Fallback to enum conversion
			var enumConverter = new EnumConverter();
			return enumConverter.Convert(value, node, toType, context);
		}

		ReportConversionFailed(context, xmlLineInfo, value, Descriptors.FlowDirectionConversionFailed);
		return "default";
	}
}