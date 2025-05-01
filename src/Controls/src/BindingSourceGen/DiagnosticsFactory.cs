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
		=> new DiagnosticInfo(
			new DiagnosticDescriptor(
				id: "BSG0001",
				title: "Invalid getter method",
				messageFormat: "The getter expression is not valid. The expression can only consist of property access, index access, and type casts.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Error,
				isEnabledByDefault: true),
			location);

	public static DiagnosticInfo GetterIsNotLambda(Location location)
		=> new DiagnosticInfo(
			new DiagnosticDescriptor(
				id: "BSG0002",
				title: "Getter method is not a lambda",
				messageFormat: "The getter must be a lambda expression.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Error,
				isEnabledByDefault: true),
			location);

	public static DiagnosticInfo GetterLambdaBodyIsNotExpression(Location location)
		=> new DiagnosticInfo(
			new DiagnosticDescriptor(
				id: "BSG0003",
				title: "Getter method body is not an expression",
				messageFormat: "The getter lambda's body must be an expression.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Error,
				isEnabledByDefault: true),
			location);

	public static DiagnosticInfo SuboptimalSetBindingOverload(Location location)
		=> new DiagnosticInfo(
			new DiagnosticDescriptor(
				id: "BSG0004",
				title: "Using SetBinding with a string path",
				messageFormat: "Consider using SetBinding<TSource, TProperty> with a lambda expression for improved performance.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Hidden,
				isEnabledByDefault: false),
			location);

	public static DiagnosticInfo LambdaParameterCannotBeResolved(Location location)
		=> new DiagnosticInfo(
			new DiagnosticDescriptor(
				id: "BSG0005",
				title: "Lambda parameter cannot be resolved",
				messageFormat: "The lambda parameter cannot be resolved. Make sure that it is not source generated.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Error,
				isEnabledByDefault: true),
			location);

	public static DiagnosticInfo LambdaResultCannotBeResolved(Location location)
		=> new DiagnosticInfo(
			new DiagnosticDescriptor(
				id: "BSG0006",
				title: "Lambda result type cannot be resolved",
				messageFormat: "The lambda result type cannot be resolved. Make sure that source generated fields / properties are not used in the path.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Error,
				isEnabledByDefault: true),
			location);

	public static DiagnosticInfo UnaccessibleTypeUsedAsLambdaParameter(Location location)
		=> new DiagnosticInfo(
			new DiagnosticDescriptor(
				id: "BSG0007",
				title: "Unaccessible type used as lambda parameter",
				messageFormat: "The lambda parameter type has to be declared as public, internal or protected internal.",
				category: "Usage",
				defaultSeverity: DiagnosticSeverity.Error,
				isEnabledByDefault: true),
			location);

	public static DiagnosticInfo UnaccessibleFieldInPath(Location location)
	=> new DiagnosticInfo(
		new DiagnosticDescriptor(
			id: "BSG0008",
			title: "Unaccessible field in path",
			messageFormat: "The path can contain only public, internal and protected internal fields.",
			category: "Usage",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true),
		location);

	public static DiagnosticInfo UnaccessiblePropertyInPath(Location location)
	=> new DiagnosticInfo(
		new DiagnosticDescriptor(
			id: "BSG0009",
			title: "Unaccessible property in path",
			messageFormat: "The path can contain only public, internal and protected internal properties.",
			category: "Usage",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true),
		location);

	public static DiagnosticInfo LambdaIsNotStatic(Location location)
	=> new DiagnosticInfo(
		new DiagnosticDescriptor(
			id: "BSG0010",
			title: "Lambda is not static",
			messageFormat: "The lambda must be static.",
			category: "Usage",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true),
		location);
}
