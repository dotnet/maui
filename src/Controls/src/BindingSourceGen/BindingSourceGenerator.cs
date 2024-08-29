using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.BindingSourceGen;

public class TrackingNames
{
	public const string BindingsWithDiagnostics = nameof(BindingsWithDiagnostics);
	public const string Bindings = nameof(Bindings);
}

[Generator(LanguageNames.CSharp)]
public class BindingSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var bindingsWithDiagnostics = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (node, _) => IsSetBindingMethod(node) || IsCreateMethod(node),
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
			.WithTrackingName(TrackingNames.Bindings);

		context.RegisterPostInitializationOutput(spc =>
		{
			spc.AddSource("GeneratedBindingInterceptorsCommon.g.cs", BindingCodeWriter.GenerateCommonCode());
		});

		context.RegisterImplementationSourceOutput(bindings, (spc, binding) =>
		{
			var fileName = $"{binding.Location.FilePath}-GeneratedBindingInterceptors-{binding.Location.Line}-{binding.Location.Column}.g.cs";
			var sanitizedFileName = fileName.Replace('/', '-').Replace('\\', '-').Replace(':', '-');
			var code = BindingCodeWriter.GenerateBinding(binding, (uint)Math.Abs(binding.Location.GetHashCode()));
			spc.AddSource(sanitizedFileName, code);
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

	private static bool IsCreateMethod(SyntaxNode node)
	{
		return node is InvocationExpressionSyntax invocation
			&& invocation.Expression is MemberAccessExpressionSyntax method
			&& method.Name.Identifier.Text == "Create"
			&& invocation.ArgumentList.Arguments.Count >= 1
			&& invocation.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax
			&& invocation.ArgumentList.Arguments[0].Expression is not ObjectCreationExpressionSyntax;
	}

	private static Result<BindingInvocationDescription> GetBindingForGeneration(GeneratorSyntaxContext context, CancellationToken t)
	{
		var enabledNullable = IsNullableContextEnabled(context);

		var invocation = (InvocationExpressionSyntax)context.Node;
		var method = (MemberAccessExpressionSyntax)invocation.Expression;

		var invocationParser = new InvocationParser(context);
		var interceptedMethodTypeResult = invocationParser.ParseInvocation(invocation, t);

		if (interceptedMethodTypeResult.HasDiagnostics)
		{
			return Result<BindingInvocationDescription>.Failure(interceptedMethodTypeResult.Diagnostics);
		}

		var sourceCodeLocation = SourceCodeLocation.CreateFrom(method.Name.GetLocation());
		if (sourceCodeLocation == null)
		{
			return Result<BindingInvocationDescription>.Failure(DiagnosticsFactory.UnableToResolvePath(invocation.GetLocation()));
		}

		var lambdaResult = ExtractLambda(invocation, interceptedMethodTypeResult.Value);
		if (lambdaResult.HasDiagnostics)
		{
			return Result<BindingInvocationDescription>.Failure(lambdaResult.Diagnostics);
		}

		if (!lambdaResult.Value.Modifiers.Any(SyntaxKind.StaticKeyword))
		{
			return Result<BindingInvocationDescription>.Failure(DiagnosticsFactory.LambdaIsNotStatic(lambdaResult.Value.GetLocation()));
		}

		var lambdaBodyResult = ExtractLambdaBody(lambdaResult.Value);
		if (lambdaBodyResult.HasDiagnostics)
		{
			return Result<BindingInvocationDescription>.Failure(lambdaBodyResult.Diagnostics);
		}

		var lambdaSymbolResult = GetLambdaSymbol(lambdaResult.Value, context.SemanticModel);
		if (lambdaSymbolResult.HasDiagnostics)
		{
			return Result<BindingInvocationDescription>.Failure(lambdaSymbolResult.Diagnostics);
		}

		var lambdaParams = lambdaSymbolResult.Value.Parameters;
		if (lambdaParams.Length == 0 || lambdaParams[0].Type is IErrorTypeSymbol)
		{
			return Result<BindingInvocationDescription>.Failure(DiagnosticsFactory.LambdaParameterCannotBeResolved(lambdaBodyResult.Value.GetLocation()));
		}
		var lambdaParamType = lambdaParams[0].Type;

		var lambdaResultType = context.SemanticModel.GetTypeInfo(lambdaBodyResult.Value, t).Type;
		if (lambdaResultType == null || lambdaResultType is IErrorTypeSymbol)
		{
			return Result<BindingInvocationDescription>.Failure(DiagnosticsFactory.LambdaResultCannotBeResolved(lambdaBodyResult.Value.GetLocation()));
		}

		if (!lambdaParamType.IsAccessible())
		{
			return Result<BindingInvocationDescription>.Failure(DiagnosticsFactory.UnaccessibleTypeUsedAsLambdaParameter(lambdaBodyResult.Value.GetLocation()));
		}

		var pathParser = new PathParser(context, enabledNullable);
		var pathParseResult = pathParser.ParsePath(lambdaBodyResult.Value);
		if (pathParseResult.HasDiagnostics)
		{
			return Result<BindingInvocationDescription>.Failure(pathParseResult.Diagnostics);
		}

		var binding = new BindingInvocationDescription(
			Location: sourceCodeLocation.ToInterceptorLocation(),
			SourceType: lambdaParamType.CreateTypeDescription(enabledNullable),
			PropertyType: lambdaResultType.CreateTypeDescription(enabledNullable),
			Path: new EquatableArray<IPathPart>([.. pathParseResult.Value]),
			SetterOptions: DeriveSetterOptions(lambdaBodyResult.Value, context.SemanticModel, enabledNullable),
			NullableContextEnabled: enabledNullable,
			MethodType: interceptedMethodTypeResult.Value);
		return Result<BindingInvocationDescription>.Success(binding);
	}

	private static bool IsNullableContextEnabled(GeneratorSyntaxContext context)
	{
		NullableContext nullableContext = context.SemanticModel.GetNullableContext(context.Node.Span.Start);
		return (nullableContext & NullableContext.Enabled) == NullableContext.Enabled;
	}

	private static Result<LambdaExpressionSyntax> ExtractLambda(InvocationExpressionSyntax invocation, InterceptedMethodType methodType)
	{
		var argumentList = invocation.ArgumentList.Arguments;
		var lambda = methodType switch
		{
			InterceptedMethodType.SetBinding => argumentList[1].Expression,
			InterceptedMethodType.Create => argumentList[0].Expression,
			_ => throw new NotSupportedException()
		};

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
				IPropertySymbol propertySymbol => propertySymbol.Type.IsTypeNullable(enabledNullable),
				IFieldSymbol fieldSymbol => fieldSymbol.Type.IsTypeNullable(enabledNullable),
				_ => false,
			};
	}
}
