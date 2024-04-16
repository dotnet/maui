using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.BindingSourceGen;

public sealed record DiagnosticInfo
{
    public DiagnosticInfo(DiagnosticDescriptor descriptor, Location? location)
    {
        Descriptor = descriptor;
        Location = location is not null ? SourceCodeLocation.CreateFrom(location) : null;
    }

    public DiagnosticDescriptor Descriptor { get; }
    public SourceCodeLocation? Location { get; }
}

internal static class DiagnosticsFactory
{
    public static DiagnosticInfo UnableToResolvePath(Location location)
        => new(
            new DiagnosticDescriptor(
                id: "BSG0001",
                title: "Unable to resolve path",
                messageFormat: "TODO: unable to resolve path",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true),
            location);

    public static DiagnosticInfo GetterIsNotLambda(Location location)
        => new(
            new DiagnosticDescriptor(
                id: "BSG0002",
                title: "Getter must be a lambda",
                messageFormat: "TODO: getter must be a lambda",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true),
            location);

    public static DiagnosticInfo GetterLambdaBodyIsNotExpression(Location location)
        => new(
            new DiagnosticDescriptor(
                id: "BSG0003",
                title: "Getter lambda's body must be an expression",
                messageFormat: "TODO: getter lambda's body must be an expression",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true),
            location);

    public static DiagnosticInfo SuboptimalSetBindingOverload(Location location)
        => new(
            new DiagnosticDescriptor(
                id: "BSG0004",
                title: "SetBinding with string path",
                messageFormat: "TODO: consider using SetBinding overload with a lambda getter",
                category: "Usage",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true),
            location);
}