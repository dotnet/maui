using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.BindingSourceGen;

[Generator(LanguageNames.CSharp)]
public class BindingSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var bindingsWithDiagnostics = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (node, _) => IsSetBindingMethod(node),
			transform: static (ctx, t) => GetBindingForGeneration(ctx, t)
		)
		.WithTrackingName(TrackingNames.BindingsWithDiagnostics);


		context.RegisterSourceOutput(bindingsWithDiagnostics, (spc, bindingWithDiagnostic) =>
		{
			foreach (var diagnostic in bindingWithDiagnostic.Diagnostics)
			{
				spc.ReportDiagnostic(Diagnostic.Create(diagnostic.Descriptor, diagnostic.Location?.ToLocation()));
			}
		});

		var bindings = bindingsWithDiagnostics
			.Where(static binding => !binding.HasDiagnostics)
			.Select(static (binding, t) => binding.Value)
			.WithTrackingName(TrackingNames.Bindings)
			.Collect();


		context.RegisterSourceOutput(bindings, (spc, bindings) =>
		{
			spc.AddSource("GeneratedBindableObjectExtensionsCommon.g.cs", BindingCodeWriter.GenerateCommonCode());

			for (int i = 0; i < bindings.Length; i++)
			{
				spc.AddSource($"GeneratedBindableObjectExtensions{i+1}.g.cs", BindingCodeWriter.GenerateBinding(bindings[i], i + 1));
			}
		});
	}

	private static bool IsSetBindingMethod(SyntaxNode node)
	{
		return node is InvocationExpressionSyntax invocation
			&& invocation.Expression is MemberAccessExpressionSyntax method
			&& method.Name.Identifier.Text == "SetBinding"
			&& invocation.ArgumentList.Arguments.Count >= 2
			&& invocation.ArgumentList.Arguments[1].Expression is not LiteralExpressionSyntax
			&& invocation.ArgumentList.Arguments[1].Expression is not ObjectCreationExpressionSyntax;
	}

	private static Result<SetBindingInvocationDescription> GetBindingForGeneration(GeneratorSyntaxContext context, CancellationToken t)
	{
		var diagnostics = new List<DiagnosticInfo>();
		var enabledNullable = IsNullableContextEnabled(context);

		var invocation = (InvocationExpressionSyntax)context.Node;
		var method = (MemberAccessExpressionSyntax)invocation.Expression;

		var sourceCodeLocation = SourceCodeLocation.CreateFrom(method.Name.GetLocation());
		if (sourceCodeLocation == null)
		{
			return Result<SetBindingInvocationDescription>.Failure(DiagnosticsFactory.UnableToResolvePath(invocation.GetLocation()));
		}

		var overloadDiagnostics = new EquatableArray<DiagnosticInfo>(VerifyCorrectOverload(invocation, context, t));
		if (overloadDiagnostics.Length > 0)
		{
			return Result<SetBindingInvocationDescription>.Failure(overloadDiagnostics);
		}

		var lambdaResult = ExtractLambda(invocation);
		if (lambdaResult.HasDiagnostics)
		{
			return Result<SetBindingInvocationDescription>.Failure(lambdaResult.Diagnostics);
		}

		var lambdaBodyResult = ExtractLambdaBody(lambdaResult.Value);
		if (lambdaBodyResult.HasDiagnostics)
		{
			return Result<SetBindingInvocationDescription>.Failure(lambdaBodyResult.Diagnostics);
		}

		var lambdaSymbolResult = GetLambdaSymbol(lambdaResult.Value, context.SemanticModel);
		if (lambdaSymbolResult.HasDiagnostics)
		{
			return Result<SetBindingInvocationDescription>.Failure(lambdaSymbolResult.Diagnostics);
		}

		var lambdaTypeInfo = context.SemanticModel.GetTypeInfo(lambdaBodyResult.Value, t);
		if (lambdaTypeInfo.Type == null)
		{
			return Result<SetBindingInvocationDescription>.Failure(DiagnosticsFactory.UnableToResolvePath(lambdaBodyResult.Value.GetLocation()));
		}

		var pathParser = new PathParser(context, enabledNullable);
		var pathParseResult = pathParser.ParsePath(lambdaBodyResult.Value);
		if (pathParseResult.HasDiagnostics)
		{
			return Result<SetBindingInvocationDescription>.Failure(pathParseResult.Diagnostics);
		}

		var binding = new SetBindingInvocationDescription(
			Location: sourceCodeLocation.ToInterceptorLocation(),
			SourceType: BindingGenerationUtilities.CreateTypeDescription(lambdaSymbolResult.Value.Parameters[0].Type, enabledNullable),
			PropertyType: BindingGenerationUtilities.CreateTypeDescription(lambdaTypeInfo.Type, enabledNullable),
			Path: new EquatableArray<IPathPart>([.. pathParseResult.Value]),
			SetterOptions: DeriveSetterOptions(lambdaBodyResult.Value, context.SemanticModel, enabledNullable),
			NullableContextEnabled: enabledNullable);
		return Result<SetBindingInvocationDescription>.Success(binding);
	}

	private static bool IsNullableContextEnabled(GeneratorSyntaxContext context)
	{
		NullableContext nullableContext = context.SemanticModel.GetNullableContext(context.Node.Span.Start);
		return (nullableContext & NullableContext.Enabled) == NullableContext.Enabled;
	}

	private static DiagnosticInfo[] VerifyCorrectOverload(InvocationExpressionSyntax invocation, GeneratorSyntaxContext context, CancellationToken t)
	{
		var argumentList = invocation.ArgumentList.Arguments;
		if (argumentList.Count < 2)
		{
			throw new ArgumentOutOfRangeException(nameof(invocation));
		}

		var secondArgument = argumentList[1].Expression;
		if (secondArgument is LambdaExpressionSyntax)
		{
			return [];
		}

		var secondArgumentType = context.SemanticModel.GetTypeInfo(secondArgument, cancellationToken: t).Type;
		return secondArgumentType switch
		{
			{ Name: "Func", ContainingNamespace.Name: "System" } => [DiagnosticsFactory.GetterIsNotLambda(secondArgument.GetLocation())],
			_ => [DiagnosticsFactory.SuboptimalSetBindingOverload(secondArgument.GetLocation())],
		};
	}

	private static Result<LambdaExpressionSyntax> ExtractLambda(InvocationExpressionSyntax invocation)
	{
		var argumentList = invocation.ArgumentList.Arguments;
		var lambda = argumentList[1].Expression;

		if (lambda is not LambdaExpressionSyntax lambdaExpression)
		{
			return Result<LambdaExpressionSyntax>.Failure(DiagnosticsFactory.GetterIsNotLambda(lambda.GetLocation()));
		}
		return Result<LambdaExpressionSyntax>.Success(lambdaExpression);

	}

	private static Result<ExpressionSyntax> ExtractLambdaBody(LambdaExpressionSyntax lambdaExpression)
	{
		if (lambdaExpression.Body is not ExpressionSyntax lambdaBody)
		{
			return Result<ExpressionSyntax>.Failure(DiagnosticsFactory.GetterLambdaBodyIsNotExpression(lambdaExpression.Body.GetLocation()));

		}
		return Result<ExpressionSyntax>.Success(lambdaBody);
	}

	private static Result<IMethodSymbol> GetLambdaSymbol(LambdaExpressionSyntax lambda, SemanticModel semanticModel)
	{
		if (semanticModel.GetSymbolInfo(lambda).Symbol is not IMethodSymbol lambdaSymbol)
		{
			return Result<IMethodSymbol>.Failure(DiagnosticsFactory.GetterIsNotLambda(lambda.GetLocation()));
		}

		return Result<IMethodSymbol>.Success(lambdaSymbol);
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
		else if (lambdaBodyExpression is ElementAccessExpressionSyntax elementAccess)
		{
			var symbol = semanticModel.GetSymbolInfo(elementAccess).Symbol;
			return new SetterOptions(IsWritable(symbol), AcceptsNullValue(symbol, enabledNullable));
		}
		else if (lambdaBodyExpression is ElementBindingExpressionSyntax elementBinding)
		{
			var symbol = semanticModel.GetSymbolInfo(elementBinding).Symbol;
			return new SetterOptions(IsWritable(symbol), AcceptsNullValue(symbol, enabledNullable));
		}

		var nestedExpression = lambdaBodyExpression switch
		{
			MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
			ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.WhenNotNull,
			MemberBindingExpressionSyntax memberBinding => memberBinding.Name,
			BinaryExpressionSyntax binary when binary.Kind() == SyntaxKind.AsExpression => binary.Left,
			CastExpressionSyntax cast => cast.Expression,
			ParenthesizedExpressionSyntax parenthesized => parenthesized.Expression,
			_ => null,
		};

		return DeriveSetterOptions(nestedExpression, semanticModel, enabledNullable);

		static bool IsWritable(ISymbol? symbol)
			=> symbol switch
			{
				IPropertySymbol propertySymbol => propertySymbol.SetMethod != null,
				IFieldSymbol fieldSymbol => !fieldSymbol.IsReadOnly,
				_ => true,
			};

		static bool AcceptsNullValue(ISymbol? symbol, bool enabledNullable)
			=> symbol switch
			{
				IPropertySymbol propertySymbol => BindingGenerationUtilities.IsTypeNullable(propertySymbol.Type, enabledNullable),
				IFieldSymbol fieldSymbol => BindingGenerationUtilities.IsTypeNullable(fieldSymbol.Type, enabledNullable),
				_ => false,
			};
	}
}
