using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.BindingSourceGen;

[Generator(LanguageNames.CSharp)]
public class BindingSourceGenerator : IIncrementalGenerator
{
	// TODO:
	// Diagnostics
	// Edge cases
	// Optimizations
	// Add diagnostic when lack of usings prevents code from determining the return type of lambda.
	// Do not process Binding(..., string);

	static int _idCounter = 0;
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

	static bool IsNullable(ITypeSymbol type)
	{
		if (type.IsValueType)
		{
			return false; // TODO: Fix
		}
		if (type.NullableAnnotation == NullableAnnotation.Annotated)
		{
			return true;
		}
		return false;
	}

	static BindingDiagnosticsWrapper GetBindingForGeneration(GeneratorSyntaxContext context, CancellationToken t)
	{
		var diagnostics = new List<Diagnostic>();
		var invocation = (InvocationExpressionSyntax)context.Node;

		var method = (MemberAccessExpressionSyntax)invocation.Expression;

		var methodSymbolInfo = context.SemanticModel.GetSymbolInfo(method, cancellationToken: t);

		if (methodSymbolInfo.Symbol is not IMethodSymbol methodSymbol) //TODO: Do we need this check?
		{
			diagnostics.Add(Diagnostic.Create(
				DiagnosticsDescriptors.UnableToResolvePath, method.GetLocation()));
			return new BindingDiagnosticsWrapper(null, diagnostics.ToArray());
		}

		// Check whether we are using correct overload
		if (methodSymbol.Parameters.Length < 2 || methodSymbol.Parameters[1].Type.Name != "Func")
		{
			diagnostics.Add(Diagnostic.Create(
				DiagnosticsDescriptors.SuboptimalSetBindingOverload, method.GetLocation()));
			return new BindingDiagnosticsWrapper(null, diagnostics.ToArray());
		}

		var argumentList = invocation.ArgumentList.Arguments;
		var getter = argumentList[1].Expression;

		//Check if getter is a lambda
		if (getter is not LambdaExpressionSyntax lambda)
		{
			diagnostics.Add(Diagnostic.Create(
				DiagnosticsDescriptors.GetterIsNotLambda, getter.GetLocation()));
			return new BindingDiagnosticsWrapper(null, diagnostics.ToArray());
		}

		//Check if lambda body is an expression
		if (lambda.Body is not ExpressionSyntax)
		{
			diagnostics.Add(Diagnostic.Create(
				DiagnosticsDescriptors.GetterLambdaBodyIsNotExpression, lambda.Body.GetLocation()));
			return new BindingDiagnosticsWrapper(null, diagnostics.ToArray());
		}

		var lambdaSymbol = context.SemanticModel.GetSymbolInfo(lambda, cancellationToken: t).Symbol as IMethodSymbol ?? throw new Exception("Unable to resolve lambda symbol");

		var inputType = lambdaSymbol.Parameters[0].Type;
		var inputTypeGlobalPath = inputType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		var outputType = lambdaSymbol.ReturnType;
		var outputTypeGlobalPath = lambdaSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		var inputTypeIsGenericParameter = inputType.Kind == SymbolKind.TypeParameter;
		var outputTypeIsGenericParameter = outputType.Kind == SymbolKind.TypeParameter;

		var sourceCodeLocation = new SourceCodeLocation(
			context.Node.SyntaxTree.FilePath,
			method.Name.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
			method.Name.GetLocation().GetLineSpan().StartLinePosition.Character + 1
		);

		var parts = new List<PathPart>();
		var correctlyParsed = ParsePath(lambda.Body, context, parts);

		if (!correctlyParsed)
		{
			diagnostics.Add(Diagnostic.Create(
				DiagnosticsDescriptors.UnableToResolvePath, lambda.Body.GetLocation(), lambda.Body.ToString()));
		}

		var codeWriterBinding = new CodeWriterBinding(
			Id: ++_idCounter,
			Location: sourceCodeLocation,
			SourceType: new TypeName(inputTypeGlobalPath, IsNullable(inputType), inputTypeIsGenericParameter),
			PropertyType: new TypeName(outputTypeGlobalPath, IsNullable(outputType), outputTypeIsGenericParameter),
			Path: parts.ToArray(),
			GenerateSetter: true //TODO: Implement
		);
		return new BindingDiagnosticsWrapper(codeWriterBinding, diagnostics.ToArray());
	}

	static bool ParsePath(CSharpSyntaxNode? expressionSyntax, GeneratorSyntaxContext context, List<PathPart> parts)
	{
		if (expressionSyntax is IdentifierNameSyntax identifier)
		{
			var member = identifier.Identifier.Text;
			var typeInfo = context.SemanticModel.GetTypeInfo(identifier).Type;
			if (typeInfo == null)
			{
				return false;
			}; // TODO
			var isNullable = IsNullable(typeInfo);
			parts.Add(new PathPart(member, isNullable));
			return true;
		}
		else if (expressionSyntax is MemberAccessExpressionSyntax memberAccess)
		{
			var member = memberAccess.Name.Identifier.Text;
			var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
			if (typeInfo == null)
			{
				return false;
			};
			if (!ParsePath(memberAccess.Expression, context, parts))
			{
				return false;
			}
			parts.Add(new PathPart(member, false));
			return true;
		}
		else if (expressionSyntax is ElementAccessExpressionSyntax elementAccess)
		{
			var member = elementAccess.Expression.ToString();
			var typeInfo = context.SemanticModel.GetTypeInfo(elementAccess.Expression).Type;
			if (typeInfo == null)
			{
				return false;
			}; // TODO
			parts.Add(new PathPart(member, false, elementAccess.ArgumentList.Arguments[0].Expression)); //TODO: Nullable
			return ParsePath(elementAccess.Expression, context, parts);
		}
		else if (expressionSyntax is ConditionalAccessExpressionSyntax conditionalAccess)
		{
			return ParsePath(conditionalAccess.Expression, context, parts) &&
			ParsePath(conditionalAccess.WhenNotNull, context, parts);
		}
		else if (expressionSyntax is MemberBindingExpressionSyntax memberBinding)
		{
			var member = memberBinding.Name.Identifier.Text;
			parts.Add(new PathPart(member, false)); //TODO: Nullable
			return true;
		}
		else if (expressionSyntax is ParenthesizedExpressionSyntax parenthesized)
		{
			return ParsePath(parenthesized.Expression, context, parts);
		}
		else if (expressionSyntax is InvocationExpressionSyntax)
		{
			return false;
		}
		else
		{
			return false;
		}
	}
}

public sealed record BindingDiagnosticsWrapper(
	CodeWriterBinding? Binding,
	Diagnostic[] Diagnostics);

public sealed record CodeWriterBinding(
	int Id,
	SourceCodeLocation Location,
	TypeName SourceType,
	TypeName PropertyType,
	PathPart[] Path,
	bool GenerateSetter);

public sealed record SourceCodeLocation(string FilePath, int Line, int Column);

public sealed record TypeName(string GlobalName, bool IsNullable, bool IsGenericParameter)
{
	public override string ToString()
		=> IsNullable
			? $"{GlobalName}?"
			: GlobalName;
}

public sealed record PathPart(string Member, bool IsNullable, object? Index = null)
{
	public string MemberName
		=> Index is not null
			? $"{Member}[{Index}]"
			: Member;

	public string PartGetter
		=> Index switch
		{
			string str => $"[\"{str}\"]",
			int num => $"[{num}]",
			null => $".{MemberName}",
			_ => throw new NotSupportedException(),
		};
}
