using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class ConstraintConverter : BaseTypeConverter
{
	public override IEnumerable<string> SupportedTypes => new[] { "Constraint", "Microsoft.Maui.Controls.Compatibility.Constraint" };

	public override string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		// IMPORTANT! Update ConstraintDesignTypeConverter.IsValid if making changes here
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var size))
				return $"global::Microsoft.Maui.Controls.Compatibility.Constraint.Constant({FormatInvariant(size)})";
		}

		ReportConversionFailed(context, xmlLineInfo, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}