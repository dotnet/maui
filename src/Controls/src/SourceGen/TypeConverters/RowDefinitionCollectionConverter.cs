using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class RowDefinitionCollectionConverter : BaseTypeConverter
{
	public override IEnumerable<string> SupportedTypes => new[] { "RowDefinitionCollection", "Microsoft.Maui.Controls.RowDefinitionCollection" };

	public override string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
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

			return $"new global::Microsoft.Maui.Controls.RowDefinitionCollection([{string.Join(", ", rowDefinitions)}])";
		}

		ReportConversionFailed(context, xmlLineInfo, value, Descriptors.RowDefinitionCollectionConversionFailed);
		return "default";
	}
}