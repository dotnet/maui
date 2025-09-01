using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class PathGeometryConverter : BaseTypeConverter
{
	public override IEnumerable<string> SupportedTypes => new[] { "PathGeometry", "Microsoft.Maui.Controls.Shapes.PathGeometry" };

	public override string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			// TODO: Implement proper path geometry parsing
			return "new global::Microsoft.Maui.Controls.Shapes.PathGeometry()";
		}

		ReportConversionFailed(context, xmlLineInfo, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}