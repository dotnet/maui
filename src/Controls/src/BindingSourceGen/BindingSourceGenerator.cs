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

		var interceptedMethodType = method.Name.Identifier.Text switch
		{
			"SetBinding" => InterceptedMethodType.SetBinding,
			"Create" => InterceptedMethodType.Create,
			_ => throw new NotSupportedException()
		};

		var sourceCodeLocation = SourceCodeLocation.CreateFrom(method.Name.GetLocation());
		if (sourceCodeLocation == null)
		{
			return Result<BindingInvocationDescription>.Failure(DiagnosticsFactory.UnableToResolvePath(invocation.GetLocation()));
		}

		var overloadDiagnostics = VerifyCorrectOverload(invocation, interceptedMethodType, context, t);
		if (overloadDiagnostics.Length > 0)
		{
			return Result<BindingInvocationDescription>.Failure(new EquatableArray<DiagnosticInfo>(overloadDiagnostics));
		}

		var lambdaResult = ExtractLambda(invocation, interceptedMethodType);
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

		if (!BindingGenerationUtilities.IsAccessible(lambdaParamType.DeclaredAccessibility))
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
			SourceType: BindingGenerationUtilities.CreateTypeDescription(lambdaParamType, enabledNullable),
			PropertyType: BindingGenerationUtilities.CreateTypeDescription(lambdaResultType, enabledNullable),
			Path: new EquatableArray<IPathPart>([.. pathParseResult.Value]),
			SetterOptions: DeriveSetterOptions(lambdaBodyResult.Value, context.SemanticModel, enabledNullable),
			NullableContextEnabled: enabledNullable,
			MethodType: interceptedMethodType);
		return Result<BindingInvocationDescription>.Success(binding);
	}

	private static bool IsNullableContextEnabled(GeneratorSyntaxContext context)
	{
		NullableContext nullableContext = context.SemanticModel.GetNullableContext(context.Node.Span.Start);
		return (nullableContext & NullableContext.Enabled) == NullableContext.Enabled;
	}

	private static DiagnosticInfo[] VerifyCorrectOverload(InvocationExpressionSyntax invocation, InterceptedMethodType methodType, GeneratorSyntaxContext context, CancellationToken t) =>
		methodType switch
		{
			InterceptedMethodType.SetBinding => VerifyCorrectOverloadSetBinding(invocation, context, t),
			InterceptedMethodType.Create => VerifyCorrectOverloadBindingCreate(invocation, context, t),
			_ => throw new NotSupportedException()
		};


	private static DiagnosticInfo[] VerifyCorrectOverloadBindingCreate(InvocationExpressionSyntax invocation, GeneratorSyntaxContext context, CancellationToken t)
	{
		var argumentList = invocation.ArgumentList.Arguments;

		var symbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol;
		if ((symbol?.ContainingType?.Name != "Binding" && symbol?.ContainingType?.Name != "BindingBase")
			|| symbol?.ContainingType?.ContainingNamespace.ToDisplayString() is not "Microsoft.Maui.Controls")
		{
			return [DiagnosticsFactory.SuboptimalSetBindingOverload(invocation.GetLocation())];
		}

		if (argumentList.Count == 0)
		{
			throw new ArgumentOutOfRangeException(nameof(invocation));
		}

		var firstArgument = argumentList[0].Expression;
		if (firstArgument is IdentifierNameSyntax)
		{
			var type = context.SemanticModel.GetTypeInfo(firstArgument, cancellationToken: t).Type;
			if (type != null && type.Name == "Func")
			{
				return [DiagnosticsFactory.GetterIsNotLambda(firstArgument.GetLocation())];
			}
			else // String and Binding
			{
				return [DiagnosticsFactory.SuboptimalSetBindingOverload(firstArgument.GetLocation())];
			}
		}

		return [];
	}

	private static DiagnosticInfo[] VerifyCorrectOverloadSetBinding(InvocationExpressionSyntax invocation, GeneratorSyntaxContext context, CancellationToken t)
	{
		var symbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol;
		if (symbol is not null)
		{
			if (symbol is not IMethodSymbol methodSymbol
				|| methodSymbol.Kind != SymbolKind.Method
				|| methodSymbol.Name != "SetBinding"
				|| !methodSymbol.IsGenericMethod
				|| methodSymbol.TypeParameters.Length != 2
				|| methodSymbol.Parameters.Length != 9
				|| methodSymbol.ContainingType?.Name != "BindableObjectExtensions"
				|| methodSymbol.ContainingType?.ContainingNamespace.ToDisplayString() is not "Microsoft.Maui.Controls")
			{
				// ignore this method invocation
				return [DiagnosticsFactory.SuboptimalSetBindingOverload(invocation.GetLocation())];
			}
		}
		else
		{
			// It is not possible to resolve the method symbol when the bindable object (the first argument or the object that the extension method
			// is called on) is referenced by a field that will be generated via XamlG based on the x:Name attributes. In that case, this source generator
			// cannot see the outputs of the other source generator and we have incomplete information about the method invocation and we can only work with
			// the syntax tree and not the semantic model.

			var argumentsList = invocation.ArgumentList.Arguments;
			if (argumentsList.Count < 2)
			{
				return [DiagnosticsFactory.SuboptimalSetBindingOverload(invocation.GetLocation())];
			}

			var secondArgument = argumentsList[1].Expression;
			if (secondArgument is not LambdaExpressionSyntax)
			{
				var secondArgumentType = context.SemanticModel.GetTypeInfo(secondArgument, cancellationToken: t).Type;
				return secondArgumentType switch
				{
					{ Name: "Func", ContainingNamespace.Name: "System" } => [DiagnosticsFactory.GetterIsNotLambda(secondArgument.GetLocation())],
					_ => [DiagnosticsFactory.SuboptimalSetBindingOverload(secondArgument.GetLocation())],
				};
			}
		}

		return [];
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
				IPropertySymbol propertySymbol => BindingGenerationUtilities.IsTypeNullable(propertySymbol.Type, enabledNullable),
				IFieldSymbol fieldSymbol => BindingGenerationUtilities.IsTypeNullable(fieldSymbol.Type, enabledNullable),
				_ => false,
			};
	}
}
