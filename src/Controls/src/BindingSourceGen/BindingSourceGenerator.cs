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
		NullableContext nullableContext = context.SemanticModel.GetNullableContext(context.Node.Span.Start);
		var enabledNullable = (nullableContext & NullableContext.Enabled) == NullableContext.Enabled;

		var invocation = (InvocationExpressionSyntax)context.Node;
		var method = (MemberAccessExpressionSyntax)invocation.Expression;

		var sourceCodeLocation = new SourceCodeLocation(
			context.Node.SyntaxTree.FilePath,
			method.Name.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
			method.Name.GetLocation().GetLineSpan().StartLinePosition.Character + 1
		);

		var overloadDiagnostics = VerifyCorrectOverload(invocation, context, t);

		if (overloadDiagnostics.Length > 0)
		{
			return ReportDiagnostics(overloadDiagnostics);
		}

		var (lambdaBody, lambdaSymbol, lambdaDiagnostics) = GetLambda(invocation, context.SemanticModel);

		if (lambdaBody == null || lambdaSymbol == null || lambdaDiagnostics.Length > 0)
		{
			return ReportDiagnostics(lambdaDiagnostics);
		}

		var lambdaTypeInfo = context.SemanticModel.GetTypeInfo(lambdaBody, t);
		if (lambdaTypeInfo.Type == null)
		{
			return ReportDiagnostics([DiagnosticsFactory.UnableToResolvePath(lambdaBody.GetLocation())]); // TODO: New diagnostic
		}

		var pathParser = new PathParser(context);
		var (pathDiagnostics, parts) = pathParser.ParsePath(lambdaBody);
		if (pathDiagnostics.Length > 0)
		{
			return ReportDiagnostics(pathDiagnostics);
		}

		var codeWriterBinding = new CodeWriterBinding(
			Location: sourceCodeLocation,
			SourceType: BindingGenerationUtilities.CreateTypeNameFromITypeSymbol(lambdaSymbol.Parameters[0].Type, enabledNullable),
			PropertyType: BindingGenerationUtilities.CreateTypeNameFromITypeSymbol(lambdaTypeInfo.Type, enabledNullable),
			Path: parts.ToArray(),
			SetterOptions: DeriveSetterOptions(lambdaBody, context.SemanticModel, enabledNullable));
		return new BindingDiagnosticsWrapper(codeWriterBinding, diagnostics.ToArray());
	}

	private static Diagnostic[] VerifyCorrectOverload(InvocationExpressionSyntax invocation, GeneratorSyntaxContext context, CancellationToken t)
	{
		var argumentList = invocation.ArgumentList.Arguments;
		if (argumentList.Count < 2)
		{
			return [DiagnosticsFactory.SuboptimalSetBindingOverload(invocation.GetLocation())];
		}

		var getter = argumentList[1].Expression;
		if (getter is not LambdaExpressionSyntax)
		{
			return [DiagnosticsFactory.SuboptimalSetBindingOverload(getter.GetLocation())];
		}

		return Array.Empty<Diagnostic>();
	}

	private static (ExpressionSyntax? lambdaBodyExpression, IMethodSymbol? lambdaSymbol, Diagnostic[] diagnostics) GetLambda(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
	{
		var argumentList = invocation.ArgumentList.Arguments;
		var lambda = (LambdaExpressionSyntax)argumentList[1].Expression;

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

	private static SetterOptions DeriveSetterOptions(ExpressionSyntax? lambdaBodyExpression, SemanticModel semanticModel, bool enabledNullable)
	{
		if (lambdaBodyExpression is null)
		{
			return new SetterOptions(IsWritable: false, AcceptsNullValue: false);
		}
		else if (lambdaBodyExpression is IdentifierNameSyntax identifier)
		{
			var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
			return new SetterOptions(IsWritable(symbol), AcceptsNullValue(symbol, enabledNullable));
		}

		var nestedExpression = lambdaBodyExpression switch
		{
			MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
			ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.WhenNotNull,
			MemberBindingExpressionSyntax memberBinding => memberBinding.Name,
			BinaryExpressionSyntax binary when binary.Kind() == SyntaxKind.AsExpression => binary.Left,
			ElementAccessExpressionSyntax elementAccess => elementAccess.Expression, // TODO indexers don't work correctlly yet
			ParenthesizedExpressionSyntax parenthesized => parenthesized.Expression,
			_ => null,
		};

		return DeriveSetterOptions(nestedExpression, semanticModel, enabledNullable);

		static bool IsWritable(ISymbol? symbol)
			=> symbol switch
			{
				IPropertySymbol propertySymbol => propertySymbol.SetMethod != null,
				IFieldSymbol fieldSymbol => !fieldSymbol.IsReadOnly,
				_ => false,
			};

		static bool AcceptsNullValue(ISymbol? symbol, bool enabledNullable)
			=> symbol switch
			{
				IPropertySymbol propertySymbol => BindingGenerationUtilities.IsTypeNullable(propertySymbol.Type, enabledNullable),
				IFieldSymbol fieldSymbol => BindingGenerationUtilities.IsTypeNullable(fieldSymbol.Type, enabledNullable),
				_ => false,
			};
	}

	private static BindingDiagnosticsWrapper ReportDiagnostics(Diagnostic[] diagnostics) => new(null, diagnostics);
}

public sealed record BindingDiagnosticsWrapper(
	CodeWriterBinding? Binding,
	Diagnostic[] Diagnostics);

public sealed record CodeWriterBinding(
	SourceCodeLocation Location,
	TypeDescription SourceType,
	TypeDescription PropertyType,
	IPathPart[] Path,
	SetterOptions SetterOptions);

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

public sealed record SetterOptions(bool IsWritable, bool AcceptsNullValue = false);

public sealed record MemberAccess(string MemberName) : IPathPart
{
	public string? PropertyName => MemberName;
}

public sealed record IndexAccess(string DefaultMemberName, object Index) : IPathPart
{
	public string? PropertyName => $"{DefaultMemberName}[{Index}]";
}

public sealed record ConditionalAccess(IPathPart Part) : IPathPart
{
	public string? PropertyName => Part.PropertyName;
}

public sealed record Cast(TypeDescription TargetType) : IPathPart
{
	public string? PropertyName => null;
}

public interface IPathPart
{
	public string? PropertyName { get; }
}
