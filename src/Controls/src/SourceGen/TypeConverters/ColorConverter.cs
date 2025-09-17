using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class ColorConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "Color", "Microsoft.Maui.Graphics.Color" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		if (Maui.Graphics.Color.TryParse(value, out var color))
		{
			var colorType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color")!;
			return $"new {colorType.ToFQDisplayString()}({FormatInvariant(color.Red)}, {FormatInvariant(color.Green)}, {FormatInvariant(color.Blue)}, {FormatInvariant(color.Alpha)}) /* {value} */"; // ensure double literals
		}

		context.ReportConversionFailed(node, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}