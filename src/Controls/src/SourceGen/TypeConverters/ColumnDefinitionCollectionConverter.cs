using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class ColumnDefinitionCollectionConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "ColumnDefinitionCollection", "Microsoft.Maui.Controls.ColumnDefinitionCollection" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			var lengths = value.Split([',']);
			var gridLengthConverter = new GridLengthConverter();

			var columnDefinitions = new List<string>();
			foreach (var length in lengths)
			{
				var gridLength = gridLengthConverter.Convert(length, node, toType, context);
				columnDefinitions.Add($"new ColumnDefinition({gridLength})");
			}

			return $"new global::Microsoft.Maui.Controls.ColumnDefinitionCollection([{string.Join(", ", columnDefinitions)}])";
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.ColumnDefinitionCollectionConversionFailed);
		return "default";
	}
}