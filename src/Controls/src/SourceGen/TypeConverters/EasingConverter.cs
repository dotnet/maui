using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class EasingConverter : ISGTypeConverter
{
	static readonly HashSet<string> KnownEasingNames = new(StringComparer.OrdinalIgnoreCase)
	{
		"Linear", "SinOut", "SinIn", "SinInOut", "CubicIn", "CubicOut", "CubicInOut",
		"BounceOut", "BounceIn", "SpringIn", "SpringOut"
	};

	public IEnumerable<string> SupportedTypes => ["Easing", "Microsoft.Maui.Easing"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		var easingName = value;

		if (!string.IsNullOrWhiteSpace(easingName))
		{
			var parts = easingName.Split(['.']);
			if (parts.Length == 2 && parts[0].Equals("Easing", StringComparison.OrdinalIgnoreCase))
			{
				easingName = parts[1];
			}

			if (KnownEasingNames.Contains(easingName))
			{
				var easingType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Easing")!;
				return $"{easingType.ToFQDisplayString()}.{easingName}";
			}
		}

		context.ReportConversionFailed(xmlLineInfo, value, Descriptors.EasingConversionFailed);
		return "default";
	}
}