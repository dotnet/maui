using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class ColumnDefinitionCollectionConverter : BaseTypeConverter
{
	public override IEnumerable<string> SupportedTypes => new[] { "ColumnDefinitionCollection", "Microsoft.Maui.Controls.ColumnDefinitionCollection" };

	public override string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
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

		ReportConversionFailed(context, xmlLineInfo, value, Descriptors.ColumnDefinitionCollectionConversionFailed);
		return "default";
	}
}