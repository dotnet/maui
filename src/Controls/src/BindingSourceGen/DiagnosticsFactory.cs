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
				title: "Invalid getter method",
				messageFormat: "The getter expression is not valid. The expression can only consist of property access, index access, and type casts.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Error,
				isEnabledByDefault: true),
			location);

	public static DiagnosticInfo GetterIsNotLambda(Location location)
		=> new(
			new DiagnosticDescriptor(
				id: "BSG0002",
				title: "Getter method is not a lambda",
				messageFormat: "The getter must be a lambda expression.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Error,
				isEnabledByDefault: true),
			location);

	public static DiagnosticInfo GetterLambdaBodyIsNotExpression(Location location)
		=> new(
			new DiagnosticDescriptor(
				id: "BSG0003",
				title: "Getter method body is not an expression",
				messageFormat: "The getter lambda's body must be an expression.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Error,
				isEnabledByDefault: true),
			location);

	public static DiagnosticInfo SuboptimalSetBindingOverload(Location location)
		=> new(
			new DiagnosticDescriptor(
				id: "BSG0004",
				title: "Using SetBinding with a string path",
				messageFormat: "Consider using SetBinding<TSource, TProperty> with a lambda expression for improved performance.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Hidden,
				isEnabledByDefault: false),
			location);
}
