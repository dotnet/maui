using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.BindingSourceGen;

[Generator(LanguageNames.CSharp)]
public class BindingSourceGenerator : IIncrementalGenerator
{
	// TODO:
	// Edge cases
	// Optimizations

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var bindingsWithDiagnostics = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (node, _) => IsSetBindingMethod(node),
			transform: static (ctx, t) => GetBindingForGeneration(ctx, t)
		)
		.WithTrackingName("BindingsWithDiagnostics");


		context.RegisterSourceOutput(bindingsWithDiagnostics, (spc, bindingWithDiagnostic) =>
		{
			foreach (var diagnostic in bindingWithDiagnostic.Diagnostics)
			{
				spc.ReportDiagnostic(diagnostic);
			}
		});

		var bindings = bindingsWithDiagnostics
			.Where(static binding => binding.Diagnostics.Length == 0 && binding.Binding != null)
			.Select(static (binding, t) => binding.Binding!)
			.WithTrackingName("Bindings")
			.Collect();


		context.RegisterSourceOutput(bindings, (spc, bindings) =>
		{
			var codeWriter = new BindingCodeWriter();

			foreach (var binding in bindings)
			{
				codeWriter.AddBinding(binding);
			}

			spc.AddSource("GeneratedBindableObjectExtensions.g.cs", codeWriter.GenerateCode());
		});
	}

	static bool IsSetBindingMethod(SyntaxNode node)
	{
		return node is InvocationExpressionSyntax invocation
			&& invocation.Expression is MemberAccessExpressionSyntax method
			&& method.Name.Identifier.Text == "SetBinding";
	}

	static BindingDiagnosticsWrapper GetBindingForGeneration(GeneratorSyntaxContext context, CancellationToken t)
	{
		var diagnostics = new List<Diagnostic>();
		var invocation = (InvocationExpressionSyntax)context.Node;

		var method = (MemberAccessExpressionSyntax)invocation.Expression;

		var sourceCodeLocation = new SourceCodeLocation(
			context.Node.SyntaxTree.FilePath,
			method.Name.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
			method.Name.GetLocation().GetLineSpan().StartLinePosition.Character + 1
		);

		var overloadDiagnostics = VerifyCorrectOverload(method, context, t);

		if (overloadDiagnostics.Length > 0)
		{
			return ReportDiagnostics(overloadDiagnostics);
		}

		var (lambdaBody, lambdaSymbol, lambdaDiagnostics) = GetLambda(invocation, context.SemanticModel);
		
		if (lambdaBody == null || lambdaSymbol == null || lambdaDiagnostics.Length > 0)
		{
			return ReportDiagnostics(lambdaDiagnostics);
		}

		NullableContext nullableContext = context.SemanticModel.GetNullableContext(context.Node.Span.Start);
		var enabledNullable = (nullableContext & NullableContext.Enabled) == NullableContext.Enabled;
		var parts = new List<IPathPart>();
		var correctlyParsed = PathParser.ParsePath(lambdaBody, enabledNullable, context, parts);

		if (!correctlyParsed)
		{
			return ReportDiagnostics([DiagnosticsFactory.UnableToResolvePath(lambdaBody.GetLocation())]);
		}

		// Sometimes analysing just the return type of the lambda is not enough. TODO: Refactor
		var propertyType = BindingGenerationUtilities.CreateTypeNameFromITypeSymbol(lambdaSymbol.ReturnType, enabledNullable);
		var lastMember = parts.Last() is Cast cast ? cast.Part : parts.Last();
		propertyType = propertyType with { IsNullable = lastMember is ConditionalAccess || propertyType.IsNullable };

		var codeWriterBinding = new CodeWriterBinding(
			Location: sourceCodeLocation,
			SourceType: BindingGenerationUtilities.CreateTypeNameFromITypeSymbol(lambdaSymbol.Parameters[0].Type, enabledNullable),
			PropertyType: propertyType,
			Path: parts.ToArray(),
			GenerateSetter: true //TODO: Implement
		);
		return new BindingDiagnosticsWrapper(codeWriterBinding, diagnostics.ToArray());
	}

	private static Diagnostic[] VerifyCorrectOverload(SyntaxNode method, GeneratorSyntaxContext context, CancellationToken t)
	{
		var methodSymbolInfo = context.SemanticModel.GetSymbolInfo(method, cancellationToken: t);

		if (methodSymbolInfo.Symbol is not IMethodSymbol methodSymbol) //TODO: Do we need this check?
		{
			return [DiagnosticsFactory.UnableToResolvePath(method.GetLocation())];
		}

		if (methodSymbol.Parameters.Length < 2 || methodSymbol.Parameters[1].Type.Name != "Func")
		{
			return [DiagnosticsFactory.SuboptimalSetBindingOverload(method.GetLocation())];
		}

		return Array.Empty<Diagnostic>();
	}

	private static (ExpressionSyntax? lambdaBodyExpression, IMethodSymbol? lambdaSymbol, Diagnostic[] diagnostics) GetLambda(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
	{
		var argumentList = invocation.ArgumentList.Arguments;
		var getter = argumentList[1].Expression;

		if (getter is not LambdaExpressionSyntax lambda)
		{
			return (null, null, [DiagnosticsFactory.GetterIsNotLambda(getter.GetLocation())]);
		}

		if (lambda.Body is not ExpressionSyntax lambdaBody)
		{
			return (null, null, [DiagnosticsFactory.GetterLambdaBodyIsNotExpression(lambda.Body.GetLocation())]);
		}

		if (semanticModel.GetSymbolInfo(lambda).Symbol is not IMethodSymbol lambdaSymbol)
		{
			return (null, null, [DiagnosticsFactory.GetterIsNotLambda(lambda.GetLocation())]);
		}

		return (lambdaBody, lambdaSymbol, Array.Empty<Diagnostic>());
	}

	private static BindingDiagnosticsWrapper ReportDiagnostics(Diagnostic[] diagnostics) => new(null, diagnostics);
}

internal static class BindingGenerationUtilities
{
	internal static bool IsTypeNullable(ITypeSymbol typeInfo, bool enabledNullable)
	{
		if (!enabledNullable && typeInfo.IsReferenceType)
		{
			return true;
		}

		if (typeInfo.NullableAnnotation == NullableAnnotation.Annotated)
		{
			return true;
		}

		return typeInfo is INamedTypeSymbol namedTypeSymbol
			&& namedTypeSymbol.IsGenericType
			&& namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;
	}

	internal static TypeDescription CreateTypeNameFromITypeSymbol(ITypeSymbol typeSymbol, bool enabledNullable)
	{
		var (isNullable, name) = GetNullabilityAndName(typeSymbol, enabledNullable);
		return new TypeDescription(
			GlobalName: name,
			IsNullable: isNullable,
			IsGenericParameter: typeSymbol.Kind == SymbolKind.TypeParameter,
			IsValueType: typeSymbol.IsValueType);
	}

	internal static (bool, string) GetNullabilityAndName(ITypeSymbol typeSymbol, bool enabledNullable)
	{
		if (typeSymbol.IsReferenceType && (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated || !enabledNullable))
		{
			return (true, typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
		}

		if (IsTypeNullable(typeSymbol, enabledNullable))
		{
			var type = ((INamedTypeSymbol)typeSymbol).TypeArguments[0];
			return (true, type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
		}

		return (false, typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
	}
}

public sealed record BindingDiagnosticsWrapper(
	CodeWriterBinding? Binding,
	Diagnostic[] Diagnostics);

public sealed record CodeWriterBinding(
	SourceCodeLocation Location,
	TypeDescription SourceType,
	TypeDescription PropertyType,
	IPathPart[] Path,
	bool GenerateSetter);

public sealed record SourceCodeLocation(string FilePath, int Line, int Column);

public sealed record TypeDescription(
	string GlobalName,
	bool IsValueType = false,
	bool IsNullable = false,
	bool IsGenericParameter = false)
{
	public override string ToString()
		=> IsNullable
			? $"{GlobalName}?"
			: GlobalName;
}

public sealed record MemberAccess(string MemberName) : IPathPart
{
	public string PropertyName => MemberName;
	public bool IsConditional => false;
}

public sealed record IndexAccess(string DefaultMemberName, IIndex Index) : IPathPart
{
	public string PropertyName => $"{DefaultMemberName}[{Index.RawIndex}]";
	public bool IsConditional => false;
}

public sealed record NumericIndex(int Constant) : IIndex
{
	public string RawIndex => Constant.ToString();
	public string FormattedIndex => Constant.ToString();
}

public sealed record StringIndex(string StringLiteral) : IIndex
{
	public string RawIndex => StringLiteral;
	public string FormattedIndex => $"\"{StringLiteral}\"";
}

public interface IIndex
{
	public string RawIndex { get; }
	public string FormattedIndex { get; }
}

public sealed record ConditionalAccess(IPathPart Part) : IPathPart
{
	public string PropertyName => Part.PropertyName;
	public bool IsConditional => true;
}

public sealed record Cast(IPathPart Part, TypeDescription TargetType) : IPathPart
{
	public string PropertyName => Part.PropertyName;
	public bool IsConditional => Part.IsConditional;
}

public interface IPathPart
{
	public string PropertyName { get; }
	public bool IsConditional { get; }
}
