using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class BindablePropertyConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "BindableProperty", "Microsoft.Maui.Controls.BindableProperty" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var parts = value.Split(['.']);

		if (parts.Length != 2)
		{
			// reportDiagnostic(Diagnostic.Create(Descriptors.BindablePropertyConversionFailed, LocationCreate(filePath, xmlLineInfo, value), value));
			return "default";
		}

		if (parts.Length == 2)
		{
			var typesymbol = parts[0]!.GetTypeSymbol(context.ReportDiagnostic, context.Compilation, context.XmlnsCache, null!)!;

			var name = parts[1];
			return typesymbol.GetBindableProperty("", ref name, out _, context, node)!.ToFQDisplayString();
		}
		return "null";
	}
}