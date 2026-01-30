using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class ConstraintConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["Constraint", "Microsoft.Maui.Controls.Compatibility.Constraint"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		// IMPORTANT! Update ConstraintDesignTypeConverter.IsValid if making changes here
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var size))
			{
				var constraintType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Compatibility.Constraint")!;
				return $"{constraintType.ToFQDisplayString()}.Constant({FormatInvariant(size)})";
			}
		}

		context.ReportConversionFailed(xmlLineInfo, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}