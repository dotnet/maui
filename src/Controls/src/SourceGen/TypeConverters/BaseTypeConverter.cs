using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.LocationHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

/// <summary>
/// Base class for type converters providing common functionality.
/// </summary>
internal abstract class BaseTypeConverter : ITypeConverter
{
	public abstract string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null);
	public abstract IEnumerable<string> SupportedTypes { get; }

	protected static void ReportConversionFailed(SourceGenContext context, IXmlLineInfo xmlLineInfo, string value, DiagnosticDescriptor descriptor)
	{
		context.ReportDiagnostic(Diagnostic.Create(descriptor, LocationCreate(context.FilePath!, xmlLineInfo, value), value));
	}

	protected static void ReportConversionFailed(SourceGenContext context, IXmlLineInfo xmlLineInfo, string value, ITypeSymbol? toType, DiagnosticDescriptor descriptor)
	{
#pragma warning disable RS0030 // Do not use banned APIs
		context.ReportDiagnostic(Diagnostic.Create(descriptor, LocationCreate(context.FilePath!, xmlLineInfo, value), value, toType?.ToDisplayString()));
#pragma warning restore RS0030 // Do not use banned APIs
	}

	protected static void ReportConversionFailed(SourceGenContext context, IXmlLineInfo xmlLineInfo, string value, string additionalInfo, ITypeSymbol? toType, DiagnosticDescriptor descriptor)
	{
#pragma warning disable RS0030 // Do not use banned APIs
		context.ReportDiagnostic(Diagnostic.Create(descriptor, LocationCreate(context.FilePath!, xmlLineInfo, value), value, additionalInfo, toType?.ToDisplayString()));
#pragma warning restore RS0030 // Do not use banned APIs
	}
}