using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class ColorConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "Color", "Microsoft.Maui.Graphics.Color" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		if (Color.TryParse(value, out var color))
		{
			var colorType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color")!;
			return $"new {colorType.ToFQDisplayString()}({FormatInvariant(color.Red)}f, {FormatInvariant(color.Green)}f, {FormatInvariant(color.Blue)}f, {FormatInvariant(color.Alpha)}f) /* {value} */";
		}

		context.ReportConversionFailed(node, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}