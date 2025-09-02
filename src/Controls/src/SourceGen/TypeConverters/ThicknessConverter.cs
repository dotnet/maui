using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class ThicknessConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "Thickness", "Microsoft.Maui.Thickness" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
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
						if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double h)
							&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double v))
							return $"new global::Microsoft.Maui.Thickness({FormatInvariant(h)}, {FormatInvariant(v)})";
						break;
					case 4:
						if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double l)
							&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
							&& double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double r)
							&& double.TryParse(thickness[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
							return $"new global::Microsoft.Maui.Thickness({FormatInvariant(l)}, {FormatInvariant(t)}, {FormatInvariant(r)}, {FormatInvariant(b)})";
						break;
				}
			}
			else
			{ //single uniform thickness
				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
					return $"new global::Microsoft.Maui.Thickness({FormatInvariant(l)})";
			}
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.ThicknessConversionFailed);
		return "default";
	}
}