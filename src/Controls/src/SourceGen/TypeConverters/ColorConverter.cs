using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

using static Microsoft.Maui.Controls.SourceGen.GeneratorHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class ColorConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "Color", "Microsoft.Maui.Graphics.Color" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		if (ColorUtils.TryParse(value, out float red, out float green, out float blue, out float alpha))
		{
			var colorType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color")!;
			return $"new {colorType.ToFQDisplayString()}({FormatInvariant(red)}f, {FormatInvariant(green)}f, {FormatInvariant(blue)}f, {FormatInvariant(alpha)}f) /* {value} */";
		}

		if (GetNamedColorField(value) is IFieldSymbol colorsField)
		{
			return $"{colorsField.ContainingType.ToFQDisplayString()}.{colorsField.Name}";
		}

		context.ReportConversionFailed((IXmlLineInfo)node, value, toType, Descriptors.ConversionFailed);
		return "default";

		IFieldSymbol? GetNamedColorField(string name)
		{
			return context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Colors")
				?.GetMembers()
				.OfType<IFieldSymbol>()
				.Where(f => f.IsPublic() && f.IsStatic && f.IsReadOnly && f.Type.ToFQDisplayString() == "global::Microsoft.Maui.Graphics.Color")
				.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
		}
	}
}