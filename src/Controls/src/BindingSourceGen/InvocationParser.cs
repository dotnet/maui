using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal class InvocationParser
{
	private readonly GeneratorSyntaxContext _context;

	internal InvocationParser(GeneratorSyntaxContext context)
	{
		_context = context;
	}

	private class InterceptedMethodsNames
	{
		internal const string SetBinding = nameof(SetBinding);
		internal const string Create = nameof(Create);
	}

	internal Result<InterceptedMethodType> ParseInvocation(InvocationExpressionSyntax invocationSyntax, CancellationToken t)
	{
		return ((MemberAccessExpressionSyntax)invocationSyntax.Expression).Name.Identifier.Text switch
		{
			InterceptedMethodsNames.SetBinding => VerifyCorrectOverloadSetBinding(invocationSyntax, t),
			InterceptedMethodsNames.Create => VerifyCorrectOverloadBindingCreate(invocationSyntax, t),
			_ => throw new NotSupportedException()
		};
	}

	private Result<InterceptedMethodType> VerifyCorrectOverloadBindingCreate(InvocationExpressionSyntax invocation, CancellationToken t)
	{
		var symbol = _context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol;
		if (symbol is not null)
		{
			// If the symbol is available, ensure it is the expected method.
			if (symbol is not IMethodSymbol methodSymbol
				|| methodSymbol.Name != "Create"
				|| (methodSymbol.ContainingType?.Name != "Binding" && methodSymbol.ContainingType?.Name != "BindingBase")
				|| methodSymbol.ContainingType?.ContainingNamespace.ToDisplayString() is not "Microsoft.Maui.Controls")
			{
				return Result<InterceptedMethodType>.Failure(DiagnosticsFactory.SuboptimalSetBindingOverload(invocation.GetLocation()));
			}
		}
		else
		{
			// It is not possible to resolve the method symbol when the bindable object (the first argument or the object that the extension method
			// is called on) is referenced by a field that will be generated via XamlG based on the x:Name attributes. In that case, this source generator
			// cannot see the outputs of the other source generator and we have incomplete information about the method invocation and we can only work with
			// the syntax tree and not the semantic model.
			var argumentsList = invocation.ArgumentList.Arguments;

			var firstArgument = argumentsList[0].Expression; // Guaranteed to have at least one argument (checked by the caller)
			if (firstArgument is not LambdaExpressionSyntax)
			{
				return Result<InterceptedMethodType>.Failure(DiagnosticsFactory.GetterIsNotLambda(firstArgument.GetLocation()));
			}
		}

		return Result<InterceptedMethodType>.Success(InterceptedMethodType.Create);
	}

	private Result<InterceptedMethodType> VerifyCorrectOverloadSetBinding(InvocationExpressionSyntax invocation, CancellationToken t)
	{
		var symbol = _context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol;
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
				return Result<InterceptedMethodType>.Failure(DiagnosticsFactory.SuboptimalSetBindingOverload(invocation.GetLocation()));
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
				return Result<InterceptedMethodType>.Failure(DiagnosticsFactory.SuboptimalSetBindingOverload(invocation.GetLocation()));
			}

			var secondArgument = argumentsList[1].Expression;
			if (secondArgument is not LambdaExpressionSyntax)
			{
				var secondArgumentType = _context.SemanticModel.GetTypeInfo(secondArgument, cancellationToken: t).Type;
				return secondArgumentType switch
				{
					{ Name: "Func", ContainingNamespace.Name: "System" } => Result<InterceptedMethodType>.Failure(DiagnosticsFactory.GetterIsNotLambda(secondArgument.GetLocation())),
					_ => Result<InterceptedMethodType>.Failure(DiagnosticsFactory.SuboptimalSetBindingOverload(secondArgument.GetLocation())),
				};
			}
		}

		return Result<InterceptedMethodType>.Success(InterceptedMethodType.SetBinding);
	}
}
