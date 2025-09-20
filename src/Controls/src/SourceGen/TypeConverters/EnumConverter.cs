using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class EnumConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "Enum" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrWhiteSpace(value) && toType is not null && toType.TypeKind == TypeKind.Enum)
		{
			var detectedEnumValue = toType.GetFields().FirstOrDefault(
				f => string.Equals(f.Name, value, StringComparison.OrdinalIgnoreCase));

			if (detectedEnumValue is not null)
			{
				return $"{toType.ToFQDisplayString()}.{detectedEnumValue}";
			}
		}

		context.ReportConversionFailed(xmlLineInfo, value, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}