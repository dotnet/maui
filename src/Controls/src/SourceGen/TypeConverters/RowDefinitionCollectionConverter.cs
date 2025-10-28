using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class RowDefinitionCollectionConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "RowDefinitionCollection", "Microsoft.Maui.Controls.RowDefinitionCollection" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			var lengths = value.Split([',']);
			var gridLengthConverter = new GridLengthConverter();

			var rowDefinitions = new List<string>();
			foreach (var length in lengths)
			{
				var gridLength = gridLengthConverter.Convert(length, node, toType, context);
				rowDefinitions.Add($"new RowDefinition({gridLength})");
			}

			var rowDefinitionCollectionType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.RowDefinitionCollection")!;
			return $"new {rowDefinitionCollectionType.ToFQDisplayString()}([{string.Join(", ", rowDefinitions)}])";
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.RowDefinitionCollectionConversionFailed);
		return "default";
	}
}