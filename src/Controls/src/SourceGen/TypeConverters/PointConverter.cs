using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class PointConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["Point", "Microsoft.Maui.Graphics.Point"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		// IMPORTANT! Update RectTypeDesignConverter.IsValid if making changes here
		if (!string.IsNullOrEmpty(value))
		{
			string[] xy = value.Split([',']);
			if (xy.Length == 2 && double.TryParse(xy[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var x)
				&& double.TryParse(xy[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var y))
			{
				var pointType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Point")!;
				return $"new {pointType.ToFQDisplayString()}({FormatInvariant(x)}, {FormatInvariant(y)})";
			}
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.PointConversionFailed);
		return "default";
	}
}