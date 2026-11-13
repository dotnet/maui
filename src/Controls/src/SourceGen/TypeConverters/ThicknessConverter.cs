using System.CodeDom.Compiler;
using System.Collections.Generic;
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
							return $"new {thicknessType.ToFQDisplayString()}({FormatInvariant(h)}, {FormatInvariant(v)})";
						}
						break;
					case 4:
						if (TryParseDouble(thickness[0], out double l)
							&& TryParseDouble(thickness[1], out double t)
							&& TryParseDouble(thickness[2], out double r)
							&& TryParseDouble(thickness[3], out double b))
						{
							var thicknessType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Thickness")!;
							return $"new {thicknessType.ToFQDisplayString()}({FormatInvariant(l)}, {FormatInvariant(t)}, {FormatInvariant(r)}, {FormatInvariant(b)})";
						}
						break;
				}
			}
			else
			{ //single uniform thickness
				if (TryParseDouble(value, out double l))
				{
					var thicknessType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Thickness")!;
					return $"new {thicknessType.ToFQDisplayString()}({FormatInvariant(l)})";
				}
			}
		}

		context.ReportConversionFailed(xmlLineInfo, value, Descriptors.ThicknessConversionFailed);
		return "default";
	}
}