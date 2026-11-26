using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class EasingConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["Easing", "Microsoft.Maui.Easing"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;

		// The same grammar as the runtime converter (the file is linked), validated here so a typo such as
		// "Sprung(0.7)" fails the build instead of the page load.
		var parsed = EasingSyntax.Parse(value);
		var easingType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Easing")!.ToFQDisplayString();

		switch (parsed.Kind)
		{
			case EasingSyntax.Kind.Named:
				return $"{easingType}.{parsed.Name}";
			case EasingSyntax.Kind.CubicBezier:
				return $"{easingType}.CubicBezier({Literal(parsed.A)}, {Literal(parsed.B)}, {Literal(parsed.C)}, {Literal(parsed.D)})";
			case EasingSyntax.Kind.Spring:
				return $"{easingType}.Spring({Literal(parsed.A)})";
			case EasingSyntax.Kind.Steps:
				return $"{easingType}.Steps({((int)parsed.A).ToString(CultureInfo.InvariantCulture)}, {(parsed.Flag ? "true" : "false")})";
		}

		context.ReportConversionFailed(xmlLineInfo, value, Descriptors.EasingConversionFailed);
		return "default";
	}

	static string Literal(double value) => value.ToString("R", CultureInfo.InvariantCulture) + "d";
}
