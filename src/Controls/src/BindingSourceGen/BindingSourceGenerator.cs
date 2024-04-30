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
			.Where(static binding => binding.Diagnostics.Length == 0 && binding.Binding != null)
			.Select(static (binding, t) => binding.Binding!)
			.WithTrackingName(TrackingNames.Bindings)
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
			&& method.Name.Identifier.Text == "SetBinding"
			&& invocation.ArgumentList.Arguments.Count >= 2
			&& invocation.ArgumentList.Arguments[1].Expression is not LiteralExpressionSyntax
			&& invocation.ArgumentList.Arguments[1].Expression is not ObjectCreationExpressionSyntax;
	}

	static BindingDiagnosticsWrapper GetBindingForGeneration(GeneratorSyntaxContext context, CancellationToken t)
	{
		var diagnostics = new List<DiagnosticInfo>();
		NullableContext nullableContext = context.SemanticModel.GetNullableContext(context.Node.Span.Start);
		var enabledNullable = (nullableContext & NullableContext.Enabled) == NullableContext.Enabled;

		var invocation = (InvocationExpressionSyntax)context.Node;
		var method = (MemberAccessExpressionSyntax)invocation.Expression;

		var sourceCodeLocation = SourceCodeLocation.CreateFrom(method.Name.GetLocation());
		if (sourceCodeLocation == null)
		{
			return ReportDiagnostic(DiagnosticsFactory.UnableToResolvePath(invocation.GetLocation()));
		}

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
			return ReportDiagnostic(DiagnosticsFactory.UnableToResolvePath(lambdaBody.GetLocation()));
		}

		var pathParser = new PathParser(context, enabledNullable);
		var (pathDiagnostics, parts) = pathParser.ParsePath(lambdaBody);
		if (pathDiagnostics.Length > 0)
		{
			return ReportDiagnostics(pathDiagnostics);
		}

		var codeWriterBinding = new CodeWriterBinding(
			Location: sourceCodeLocation.ToInterceptorLocation(),
			SourceType: BindingGenerationUtilities.CreateTypeNameFromITypeSymbol(lambdaSymbol.Parameters[0].Type, enabledNullable),
			PropertyType: BindingGenerationUtilities.CreateTypeNameFromITypeSymbol(lambdaTypeInfo.Type, enabledNullable),
			Path: new EquatableArray<IPathPart>([.. parts]),
			SetterOptions: DeriveSetterOptions(lambdaBody, context.SemanticModel, enabledNullable));
		return new BindingDiagnosticsWrapper(codeWriterBinding, new EquatableArray<DiagnosticInfo>([.. diagnostics]));
	}

	private static EquatableArray<DiagnosticInfo> VerifyCorrectOverload(InvocationExpressionSyntax invocation, GeneratorSyntaxContext context, CancellationToken t)
	{
		var argumentList = invocation.ArgumentList.Arguments;

		if (argumentList.Count < 2)
		{
			throw new ArgumentOutOfRangeException(nameof(invocation));
		}

		var secondArgument = argumentList[1].Expression;

		if (secondArgument is IdentifierNameSyntax)
		{
			var type = context.SemanticModel.GetTypeInfo(secondArgument, cancellationToken: t).Type;
			if (type != null && type.Name == "Func")
			{
				return new EquatableArray<DiagnosticInfo>([DiagnosticsFactory.GetterIsNotLambda(secondArgument.GetLocation())]);
			}
			else // String and Binding
			{
				return new EquatableArray<DiagnosticInfo>([DiagnosticsFactory.SuboptimalSetBindingOverload(secondArgument.GetLocation())]);
			}
		}

		return [];
	}

	private static (ExpressionSyntax? lambdaBodyExpression, IMethodSymbol? lambdaSymbol, EquatableArray<DiagnosticInfo> diagnostics) GetLambda(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
	{
		var argumentList = invocation.ArgumentList.Arguments;
		var lambda = (LambdaExpressionSyntax)argumentList[1].Expression;

		if (lambda.Body is not ExpressionSyntax lambdaBody)
		{
			return (null, null, new EquatableArray<DiagnosticInfo>([DiagnosticsFactory.GetterLambdaBodyIsNotExpression(lambda.Body.GetLocation())]));
		}

		if (semanticModel.GetSymbolInfo(lambda).Symbol is not IMethodSymbol lambdaSymbol)
		{
			return (null, null, new EquatableArray<DiagnosticInfo>([DiagnosticsFactory.GetterIsNotLambda(lambda.GetLocation())]));
		}

		return (lambdaBody, lambdaSymbol, []);
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

	private static BindingDiagnosticsWrapper ReportDiagnostics(EquatableArray<DiagnosticInfo> diagnostics) => new(null, diagnostics);
	private static BindingDiagnosticsWrapper ReportDiagnostic(DiagnosticInfo diagnostic) => new(null, new EquatableArray<DiagnosticInfo>([diagnostic]));
}
