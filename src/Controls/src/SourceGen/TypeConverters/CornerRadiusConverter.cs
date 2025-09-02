using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class CornerRadiusConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "CornerRadius", "Microsoft.Maui.CornerRadius" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		// IMPORTANT! Update CornerRadiusDesignTypeConverter.IsValid if making changes here
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			if (value.Contains(","))
			{ //Xaml
				var cornerRadius = value.Split([',']);
				if (cornerRadius.Length == 4
					&& double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double tl)
					&& double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double tr)
					&& double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double bl)
					&& double.TryParse(cornerRadius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double br))
					return $"new global::Microsoft.Maui.CornerRadius({FormatInvariant(tl)}, {FormatInvariant(tr)}, {FormatInvariant(bl)}, {FormatInvariant(br)})";

				if (cornerRadius.Length > 1
					&& cornerRadius.Length < 4
					&& double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
					return $"new global::Microsoft.Maui.CornerRadius({FormatInvariant(l)})";
			}
			else
			{ //single uniform CornerRadius
				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
					return $"new global::Microsoft.Maui.CornerRadius({FormatInvariant(l)})";
			}
		}

		context.ReportConversionFailed(xmlLineInfo, value, Descriptors.CornerRadiusConversionFailed);
		return "default";
	}
}