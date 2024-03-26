using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal static class DiagnosticsDescriptors
{
    public static DiagnosticDescriptor UnableToResolvePath { get; } = new(
        id: "BSG0001",
        title: "Unable to resolve path",
        messageFormat: "TODO: unable to resolve path",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor GetterIsNotLambda { get; } = new(
        id: "BSG0002",
        title: "Getter must be a lambda",
        messageFormat: "TODO: getter must be a lambda",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor GetterLambdaBodyIsNotExpression { get; } = new(
        id: "BSG0003",
        title: "Getter lambda's body must be an expression",
        messageFormat: "TODO: getter lambda's body must be an expression",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor SuboptimalSetBindingOverload { get; } = new(
        id: "BSG0004",
        title: "SetBinding with string path",
        messageFormat: "TODO: consider using SetBinding overload with a lambda getter",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}