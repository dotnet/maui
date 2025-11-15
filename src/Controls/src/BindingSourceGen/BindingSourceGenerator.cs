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
			var location = binding.SimpleLocation;
			if (location == null)
			{
				throw new InvalidOperationException("Location cannot be null");
			}

			var fileName = $"{location.FilePath}-GeneratedBindingInterceptors-{location.Line}-{location.Column}.g.cs";
			var sanitizedFileName = fileName.Replace('/', '-').Replace('\\', '-').Replace(':', '-');
			var methodNamePrefix = binding.MethodType switch
			{
				InterceptedMethodType.SetBinding => "SetBinding",
				InterceptedMethodType.Create => "Create",
				_ => throw new NotSupportedException()
			};
			var uniqueId = (uint)Math.Abs(location.GetHashCode());

			var code = BindingCodeWriter.GenerateBinding(binding, $"{methodNamePrefix}{uniqueId}");
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
			&& invocation.ArgumentList.Arguments[0].Expression is not ObjectCreationExpressionSyntax
			&& GetTypeNameFromExpression(method.Expression) switch
			{
				"Binding" => true,
				"BindingBase" => true,
				_ => false
			};
	}

	private static string GetTypeNameFromExpression(ExpressionSyntax expression)
	{
		// Handle simple identifiers (Binding.Create)
		if (expression is IdentifierNameSyntax identifier)
		{
			return identifier.Identifier.Text;
		}

		// Handle qualified names (Microsoft.Maui.Controls.Binding.Create)
		if (expression is MemberAccessExpressionSyntax memberAccess)
		{
			return memberAccess.Name.Identifier.Text;
		}

		return string.Empty;
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

#pragma warning disable RSEXPERIMENTAL002
		var interceptableLocation = context.SemanticModel.GetInterceptableLocation(invocation, t);
#pragma warning restore RSEXPERIMENTAL002

		var sourceCodeLocation = SourceCodeLocation.CreateFrom(method.Name.GetLocation());


		if (interceptableLocation == null || sourceCodeLocation == null)
		{
			return Result<BindingInvocationDescription>.Failure(DiagnosticsFactory.UnableToResolvePath(invocation.GetLocation()));
		}

		var lambdaResult = GetLambda(invocation, interceptedMethodTypeResult.Value);
		if (lambdaResult.HasDiagnostics)
		{
			return Result<BindingInvocationDescription>.Failure(lambdaResult.Diagnostics);
		}

		var lambdaParamTypeResult = GetLambdaParameterType(lambdaResult.Value, context.SemanticModel, t);
		if (lambdaParamTypeResult.HasDiagnostics)
		{
			return Result<BindingInvocationDescription>.Failure(lambdaParamTypeResult.Diagnostics);
		}

		var lambdaReturnTypeResult = GetLambdaReturnType(lambdaResult.Value, context.SemanticModel, t);
		if (lambdaReturnTypeResult.HasDiagnostics)
		{
			return Result<BindingInvocationDescription>.Failure(lambdaReturnTypeResult.Diagnostics);
		}

		var pathParser = new PathParser(context, enabledNullable);
		var pathParseResult = pathParser.ParsePath(lambdaResult.Value.ExpressionBody);
		if (pathParseResult.HasDiagnostics)
		{
			return Result<BindingInvocationDescription>.Failure(pathParseResult.Diagnostics);
		}

		var binding = new BindingInvocationDescription(
			InterceptableLocation: new InterceptableLocationRecord(interceptableLocation.Version, interceptableLocation.Data),
			SimpleLocation: sourceCodeLocation.ToSimpleLocation(),
			SourceType: lambdaParamTypeResult.Value.CreateTypeDescription(enabledNullable),
			PropertyType: lambdaReturnTypeResult.Value.CreateTypeDescription(enabledNullable),
			Path: new EquatableArray<IPathPart>([.. pathParseResult.Value]),
			SetterOptions: DeriveSetterOptions(lambdaResult.Value.ExpressionBody, context.SemanticModel, enabledNullable),
			NullableContextEnabled: enabledNullable,
			MethodType: interceptedMethodTypeResult.Value,
			IsPublic: true);
		return Result<BindingInvocationDescription>.Success(binding);
	}

	private static bool IsNullableContextEnabled(GeneratorSyntaxContext context)
	{
		NullableContext nullableContext = context.SemanticModel.GetNullableContext(context.Node.Span.Start);
		return (nullableContext & NullableContext.Enabled) == NullableContext.Enabled;
	}

	private static Result<LambdaExpressionSyntax> GetLambda(InvocationExpressionSyntax invocation, InterceptedMethodType methodType)
	{
		var argumentList = invocation.ArgumentList.Arguments;
		var expression = methodType switch
		{
			InterceptedMethodType.SetBinding => argumentList[1].Expression,
			InterceptedMethodType.Create => argumentList[0].Expression,
			_ => throw new NotSupportedException()
		};

		if (expression is not LambdaExpressionSyntax lambda)
		{
			return Result<LambdaExpressionSyntax>.Failure(DiagnosticsFactory.GetterIsNotLambda(expression.GetLocation()));
		}

		// We only support static lambdas
		if (!lambda.Modifiers.Any(SyntaxKind.StaticKeyword))
		{
			return Result<LambdaExpressionSyntax>.Failure(DiagnosticsFactory.LambdaIsNotStatic(lambda.GetLocation()));
		}

		return Result<LambdaExpressionSyntax>.Success(lambda);
	}

	private static Result<ITypeSymbol> GetLambdaReturnType(LambdaExpressionSyntax lambda, SemanticModel semanticModel, CancellationToken t)
	{
		if (lambda.Body is not ExpressionSyntax lambdaBody)
		{
			return Result<ITypeSymbol>.Failure(DiagnosticsFactory.GetterLambdaBodyIsNotExpression(lambda.Body.GetLocation()));
		}

		var lambdaResultType = semanticModel.GetTypeInfo(lambdaBody, t).Type;
		if (lambdaResultType == null || lambdaResultType is IErrorTypeSymbol)
		{
			return Result<ITypeSymbol>.Failure(DiagnosticsFactory.LambdaResultCannotBeResolved(lambdaBody.GetLocation()));
		}

		return Result<ITypeSymbol>.Success(lambdaResultType);
	}

	private static Result<ITypeSymbol> GetLambdaParameterType(LambdaExpressionSyntax lambda, SemanticModel semanticModel, CancellationToken t)
	{
		if (semanticModel.GetSymbolInfo(lambda, t).Symbol is not IMethodSymbol lambdaSymbol)
		{
			return Result<ITypeSymbol>.Failure(DiagnosticsFactory.GetterIsNotLambda(lambda.GetLocation()));
		}

		var parameters = lambdaSymbol.Parameters;
		if (parameters.Length == 0 || parameters[0].Type is IErrorTypeSymbol)
		{
			return Result<ITypeSymbol>.Failure(DiagnosticsFactory.LambdaParameterCannotBeResolved(lambda.GetLocation()));
		}

		var lambdaParamType = parameters[0].Type;

		return Result<ITypeSymbol>.Success(lambdaParamType);
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
				IPropertySymbol { OriginalDefinition.SetMethod.IsInitOnly: true } => false,
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
