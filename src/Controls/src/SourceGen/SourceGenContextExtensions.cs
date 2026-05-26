using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.LocationHelpers;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

/// <summary>
/// Extension methods for SourceGenContext to provide diagnostic reporting functionality for type converters.
/// </summary>
internal static class SourceGenContextExtensions
{
	/// <summary>
	/// Reports a conversion failure diagnostic.
	/// </summary>
	/// <param name="context">The source generation context</param>
	/// <param name="xmlLineInfo">XML line information for location</param>
	/// <param name="value">The value that failed to convert</param>
	/// <param name="descriptor">The diagnostic descriptor</param>
	public static void ReportConversionFailed(this SourceGenContext context, IXmlLineInfo xmlLineInfo, string value, DiagnosticDescriptor descriptor)
	{
		context.ReportDiagnostic(Diagnostic.Create(descriptor, LocationCreate(context.ProjectItem.RelativePath!, xmlLineInfo, value), value));
	}

	/// <summary>
	/// Reports a conversion failure diagnostic with target type information.
	/// </summary>
	/// <param name="context">The source generation context</param>
	/// <param name="xmlLineInfo">XML line information for location</param>
	/// <param name="value">The value that failed to convert</param>
	/// <param name="toType">The target type for conversion</param>
	/// <param name="descriptor">The diagnostic descriptor</param>
	public static void ReportConversionFailed(this SourceGenContext context, IXmlLineInfo xmlLineInfo, string value, ITypeSymbol? toType, DiagnosticDescriptor descriptor)
	{
#pragma warning disable RS0030 // Do not use banned APIs
		context.ReportDiagnostic(Diagnostic.Create(descriptor, LocationCreate(context.ProjectItem.RelativePath!, xmlLineInfo, value), value, toType?.ToDisplayString()));
#pragma warning restore RS0030 // Do not use banned APIs
	}

	/// <summary>
	/// Reports a conversion failure diagnostic with additional information and target type.
	/// </summary>
	/// <param name="context">The source generation context</param>
	/// <param name="xmlLineInfo">XML line information for location</param>
	/// <param name="value">The value that failed to convert</param>
	/// <param name="additionalInfo">Additional context information</param>
	/// <param name="toType">The target type for conversion</param>
	/// <param name="descriptor">The diagnostic descriptor</param>
	public static void ReportConversionFailed(this SourceGenContext context, IXmlLineInfo xmlLineInfo, string value, string additionalInfo, ITypeSymbol? toType, DiagnosticDescriptor descriptor)
	{
#pragma warning disable RS0030 // Do not use banned APIs
		context.ReportDiagnostic(Diagnostic.Create(descriptor, LocationCreate(context.ProjectItem.RelativePath!, xmlLineInfo, value), value, additionalInfo, toType?.ToDisplayString()));
#pragma warning restore RS0030 // Do not use banned APIs
	}
}