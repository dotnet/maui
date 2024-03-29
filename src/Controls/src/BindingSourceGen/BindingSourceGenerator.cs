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

		var sourceCodeLocation = new SourceCodeLocation(
			context.Node.SyntaxTree.FilePath,
			method.Name.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
			method.Name.GetLocation().GetLineSpan().StartLinePosition.Character + 1
		);

		NullableContext nullableContext = context.SemanticModel.GetNullableContext(context.Node.Span.Start);
		var enabledNullable = (nullableContext & NullableContext.Enabled) == NullableContext.Enabled;

		var parts = new List<PathPart>();
		var correctlyParsed = ParsePath(lambda.Body, enabledNullable, context, parts);

		if (!correctlyParsed)
		{
			diagnostics.Add(Diagnostic.Create(
				DiagnosticsDescriptors.UnableToResolvePath, lambda.Body.GetLocation(), lambda.Body.ToString()));
		}

		var codeWriterBinding = new CodeWriterBinding(
			Id: ++_idCounter,
			Location: sourceCodeLocation,
			SourceType: CreateTypeNameFromITypeSymbol(lambdaSymbol.Parameters[0].Type, enabledNullable),
			PropertyType: CreateTypeNameFromITypeSymbol(lambdaSymbol.ReturnType, enabledNullable),
			Path: parts.ToArray(),
			GenerateSetter: true //TODO: Implement
		);
		return new BindingDiagnosticsWrapper(codeWriterBinding, diagnostics.ToArray());
	}
	static bool ParsePath(CSharpSyntaxNode? expressionSyntax, bool enabledNullable, GeneratorSyntaxContext context, List<PathPart> parts, bool isNodeNullable = false, object? index = null)
	{
		if (expressionSyntax is IdentifierNameSyntax identifier)
		{
			return true;
		}
		else if (expressionSyntax is MemberAccessExpressionSyntax memberAccess)
		{
			var member = memberAccess.Name.Identifier.Text;
			var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Name).Type;
			if (typeInfo == null)
			{
				return false;
			};
			if (!ParsePath(memberAccess.Expression, enabledNullable, context, parts))
			{
				return false;
			}
			parts.Add(new PathPart(member, isNodeNullable || IsTypeNullable(typeInfo, enabledNullable), index));
			return true;
		}
		else if (expressionSyntax is ElementAccessExpressionSyntax elementAccess)
		{
			var typeInfo = context.SemanticModel.GetTypeInfo(elementAccess.Expression).Type;
			if (typeInfo == null)
			{
				return false;
			}; // TODO
			var argumentList = elementAccess.ArgumentList.Arguments;
			if (argumentList.Count != 1)
			{
				return false;
			}
			var indexExpression = argumentList[0].Expression;
			var indexValue = context.SemanticModel.GetConstantValue(indexExpression).Value;

			return ParsePath(elementAccess.Expression, enabledNullable, context, parts, index: indexValue);
		}
		else if (expressionSyntax is ConditionalAccessExpressionSyntax conditionalAccess)
		{
			return ParsePath(conditionalAccess.Expression, enabledNullable, context, parts, isNodeNullable: true) &&
			ParsePath(conditionalAccess.WhenNotNull, enabledNullable, context, parts);
		}
		else if (expressionSyntax is MemberBindingExpressionSyntax memberBinding)
		{
			var member = memberBinding.Name.Identifier.Text;
			parts.Add(new PathPart(member, isNodeNullable, index));
			return true;
		}
		else if (expressionSyntax is ParenthesizedExpressionSyntax parenthesized)
		{
			return ParsePath(parenthesized.Expression, enabledNullable, context, parts);
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

	internal static bool IsTypeNullable(ITypeSymbol typeInfo, bool enabledNullable)
	{
		if (!enabledNullable && typeInfo.IsReferenceType)
		{
			return true;
		}

		return typeInfo is INamedTypeSymbol namedTypeSymbol
			&& namedTypeSymbol.IsGenericType
			&& namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T;
	}

	internal static TypeName CreateTypeNameFromITypeSymbol(ITypeSymbol typeSymbol, bool enabledNullable)
	{
		var (isNullable, name) = GetNullabilityAndName(typeSymbol, enabledNullable);
		return new TypeName(
			GlobalName: name,
			IsNullable: isNullable,
			IsGenericParameter: typeSymbol.Kind == SymbolKind.TypeParameter
		);
	}

	static (bool, string) GetNullabilityAndName(ITypeSymbol typeSymbol, bool enabledNullable)
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
