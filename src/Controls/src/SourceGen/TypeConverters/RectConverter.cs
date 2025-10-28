using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class RectConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "Rect", "Microsoft.Maui.Graphics.Rect" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		// IMPORTANT! Update RectTypeDesignConverter.IsValid if making changes here
		if (!string.IsNullOrEmpty(value))
		{
			string[] xywh = value.Split([',']);
			if (xywh.Length == 4
				&& double.TryParse(xywh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double x)
				&& double.TryParse(xywh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double y)
				&& double.TryParse(xywh[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double w)
				&& double.TryParse(xywh[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
			{
				var rectType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Rect")!;
				return $"new {rectType.ToFQDisplayString()}({FormatInvariant(x)}, {FormatInvariant(y)}, {FormatInvariant(w)}, {FormatInvariant(h)})";
			}
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.RectConversionFailed);
		return "default";
	}
}