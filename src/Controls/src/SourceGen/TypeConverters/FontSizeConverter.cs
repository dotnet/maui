using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;
using static Microsoft.Maui.Controls.SourceGen.LocationHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class FontSizeConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["FontSize", "double", "System.Double"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
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
				if (xmlLineInfo.LineNumber > 0)
				{
					var location = LocationCreate(context.ProjectItem?.RelativePath ?? string.Empty, xmlLineInfo, value);
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.ObsoleteFontSize, location, $"NamedSize.{detectedEnumValue.Name}"));
				}

				var type = parentVar?.Type ?? context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Label")!;
				var deviceType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Device")!;
				var namedSizeType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.NamedSize")!;
				var getNamedSizeExpr = $"{deviceType.ToFQDisplayString()}.GetNamedSize(({namedSizeType.ToFQDisplayString()}){detectedEnumValue.ConstantValue}, typeof({type.ToFQDisplayString()}))";
				return $"\n#pragma warning disable CS0612 // Type or member is obsolete\n{getNamedSizeExpr}\n#pragma warning restore CS0612 // Type or member is obsolete\n";
			}
		}

		context.ReportConversionFailed(xmlLineInfo, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}