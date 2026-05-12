using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.HybridWebViewSourceGen;

internal static class Diagnostics
{
	public static readonly DiagnosticDescriptor ClassMustBePartial = new(
		id: "HWV0001",
		title: "HybridWebView provider class must be partial",
		messageFormat: "Class '{0}' with [HybridWebViewDotNetMethodProvider] must be declared as partial",
		category: "HybridWebView",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor NoCallableMethods = new(
		id: "HWV0002",
		title: "No callable methods found",
		messageFormat: "Class '{0}' has no callable methods. In Explicit mode, add [HybridWebViewCallable] to methods. In AllPublic mode, add public instance methods.",
		category: "HybridWebView",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor UnsupportedParameter = new(
		id: "HWV0003",
		title: "Unsupported parameter modifier",
		messageFormat: "{0}",
		category: "HybridWebView",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor MissingJsonTypeInfo = new(
		id: "HWV0004",
		title: "Missing JsonTypeInfo for type",
		messageFormat: "Type '{0}' has no matching JsonTypeInfo property in the specified JsonSerializerContext '{1}'",
		category: "HybridWebView",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor DuplicateJsName = new(
		id: "HWV0005",
		title: "Duplicate JS-facing method name",
		messageFormat: "JS-facing name '{0}' is used by both '{1}' and '{2}'",
		category: "HybridWebView",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);
}
