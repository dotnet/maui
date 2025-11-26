using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;
 
class PathGeometryConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "PathGeometry", "Microsoft.Maui.Controls.Shapes.PathGeometry" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			// TODO: Implement proper path geometry parsing
			var pathGeometryType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Shapes.PathGeometry")!;
			return $"new {pathGeometryType.ToFQDisplayString()}()";
		}

		context.ReportConversionFailed( xmlLineInfo, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}