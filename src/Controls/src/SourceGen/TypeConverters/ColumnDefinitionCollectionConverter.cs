using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class ColumnDefinitionCollectionConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["ColumnDefinitionCollection", "Microsoft.Maui.Controls.ColumnDefinitionCollection"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		var columnDefinitionType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ColumnDefinition")!;
		if (!string.IsNullOrEmpty(value))
		{
			var lengths = value.Split([',']);
			var gridLengthConverter = new GridLengthConverter();

			var columnDefinitions = new List<string>();
			foreach (var length in lengths)
			{
				var gridLength = gridLengthConverter.Convert(length, node, toType, writer, context);
				columnDefinitions.Add($"new {columnDefinitionType.ToFQDisplayString()}({gridLength})");
			}

			var columnDefinitionCollectionType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ColumnDefinitionCollection")!;
			return $"new {columnDefinitionCollectionType.ToFQDisplayString()}([{string.Join(", ", columnDefinitions)}])";
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.ColumnDefinitionCollectionConversionFailed);
		return "default";
	}
}