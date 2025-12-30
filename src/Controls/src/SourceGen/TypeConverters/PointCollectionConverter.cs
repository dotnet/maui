using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class PointCollectionConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["PointCollection", "Microsoft.Maui.Controls.PointCollection"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			string[] points = value.Split([' ', ',']);
			var pointCollection = new List<string>();
			var pointConverter = new PointConverter();
			double x = 0;
			bool hasX = false;

			foreach (string point in points)
			{
				if (string.IsNullOrWhiteSpace(point))
					continue;

				if (double.TryParse(point, NumberStyles.Number, CultureInfo.InvariantCulture, out double number))
				{
					if (!hasX)
					{
						x = number;
						hasX = true;
					}
					else
					{
						pointCollection.Add(pointConverter.Convert($"{FormatInvariant(x)},{FormatInvariant(number)}", node, toType, writer, context));
						hasX = false;
					}
				}
				else
				{
					context.ReportConversionFailed( xmlLineInfo, value, toType, Descriptors.ConversionFailed);
					return "default";
				}
			}

			if (hasX)
			{
				context.ReportConversionFailed( xmlLineInfo, value, toType, Descriptors.ConversionFailed);
				return "default";
			}

			var pointCollectionType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.PointCollection")!;
			
			return $"new {pointCollectionType.ToFQDisplayString()}(new[] {{ {string.Join(", ", pointCollection)} }})";
		}

		context.ReportConversionFailed( xmlLineInfo, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}