using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class FontSizeConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "FontSize", "double", "System.Double" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();
			if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double size))
				return $"{FormatInvariant(size, quoted: true)}D";

			var namedSizeSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.NamedSize")!;

			var detectedEnumValue = namedSizeSymbol.GetFields().FirstOrDefault(
				f => string.Equals(f.Name, value, StringComparison.OrdinalIgnoreCase));
			if (detectedEnumValue is not null)
			{
				var type = parentVar?.Type ?? context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Label")!;
				var deviceType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Device")!;
				var namedSizeType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.NamedSize")!;
				return $"{deviceType.ToFQDisplayString()}.GetNamedSize(({namedSizeType.ToFQDisplayString()}){detectedEnumValue.ConstantValue}, typeof({type.ToFQDisplayString()}))";
			}
		}

		context.ReportConversionFailed( xmlLineInfo, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}