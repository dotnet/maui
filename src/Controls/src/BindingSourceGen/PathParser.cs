using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal class PathParser
{
	private readonly GeneratorSyntaxContext _context;
	private readonly bool _enabledNullable;

	internal PathParser(GeneratorSyntaxContext context, bool enabledNullable)
	{
		_context = context;
		_enabledNullable = enabledNullable;
	}

	internal Result<List<IPathPart>> ParsePath(CSharpSyntaxNode? expressionSyntax)
	{
		return expressionSyntax switch
		{
			IdentifierNameSyntax _ => Result<List<IPathPart>>.Success(new List<IPathPart>()),
			MemberAccessExpressionSyntax memberAccess => HandleMemberAccessExpression(memberAccess),
			ElementAccessExpressionSyntax elementAccess => HandleElementAccessExpression(elementAccess),
			ElementBindingExpressionSyntax elementBinding => HandleElementBindingExpression(elementBinding),
			ConditionalAccessExpressionSyntax conditionalAccess => HandleConditionalAccessExpression(conditionalAccess),
			MemberBindingExpressionSyntax memberBinding => HandleMemberBindingExpression(memberBinding),
			ParenthesizedExpressionSyntax parenthesized => ParsePath(parenthesized.Expression),
			BinaryExpressionSyntax asExpression when asExpression.Kind() == SyntaxKind.AsExpression => HandleBinaryExpression(asExpression),
			CastExpressionSyntax castExpression => HandleCastExpression(castExpression),
			_ => HandleDefaultCase(),
		};
	}

	private Result<List<IPathPart>> HandleMemberAccessExpression(MemberAccessExpressionSyntax memberAccess)
	{
		var result = ParsePath(memberAccess.Expression);
		if (result.HasDiagnostics)
		{
			return result;
		}

		var member = memberAccess.Name.Identifier.Text;
		var typeInfo = _context.SemanticModel.GetTypeInfo(memberAccess).Type;
		var symbol = _context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;

		if (symbol == null || typeInfo == null)
		{
			return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(memberAccess.GetLocation()));
		}

		var isReferenceType = typeInfo.IsReferenceType;
		var accessorKind = symbol.ToAccessorKind();
		var memberType = typeInfo.CreateTypeDescription(_enabledNullable);
		var containgType = symbol.ContainingType.CreateTypeDescription(_enabledNullable);

		IPathPart part = symbol.IsAccessible()
			? new MemberAccess(member, !isReferenceType)
			: new InaccessibleMemberAccess(containgType, memberType, accessorKind, member, !isReferenceType);

		result.Value.Add(part);
		return Result<List<IPathPart>>.Success(result.Value);
	}

	private Result<List<IPathPart>> HandleElementAccessExpression(ElementAccessExpressionSyntax elementAccess)
	{
		var result = ParsePath(elementAccess.Expression);
		if (result.HasDiagnostics)
		{
			return result;
		}

		var elementAccessSymbol = _context.SemanticModel.GetSymbolInfo(elementAccess).Symbol;
		var elementType = _context.SemanticModel.GetTypeInfo(elementAccess).Type;

		var elementAccessResult = CreateIndexAccess(elementAccessSymbol, elementType, elementAccess.ArgumentList.Arguments, elementAccess.GetLocation());
		if (elementAccessResult.HasDiagnostics)
		{
			return elementAccessResult;
		}
		result.Value.AddRange(elementAccessResult.Value);

		return Result<List<IPathPart>>.Success(result.Value);
	}

	private Result<List<IPathPart>> HandleConditionalAccessExpression(ConditionalAccessExpressionSyntax conditionalAccess)
	{
		var expressionResult = ParsePath(conditionalAccess.Expression);
		if (expressionResult.HasDiagnostics)
		{
			return expressionResult;
		}

		var whenNotNullResult = ParsePath(conditionalAccess.WhenNotNull);
		if (whenNotNullResult.HasDiagnostics)
		{
			return whenNotNullResult;
		}

		expressionResult.Value.AddRange(whenNotNullResult.Value);

		return Result<List<IPathPart>>.Success(expressionResult.Value);
	}

	private Result<List<IPathPart>> HandleMemberBindingExpression(MemberBindingExpressionSyntax memberBinding)
	{
		var member = memberBinding.Name.Identifier.Text;
		var typeInfo = _context.SemanticModel.GetTypeInfo(memberBinding).Type;
		var isReferenceType = typeInfo?.IsReferenceType ?? false;
		IPathPart part = new MemberAccess(member, !isReferenceType);
		part = new ConditionalAccess(part);

		return Result<List<IPathPart>>.Success(new List<IPathPart>([part]));
	}

	private Result<List<IPathPart>> HandleElementBindingExpression(ElementBindingExpressionSyntax elementBinding)
	{
		var elementAccessSymbol = _context.SemanticModel.GetSymbolInfo(elementBinding).Symbol;
		var elementType = _context.SemanticModel.GetTypeInfo(elementBinding).Type;

		var elementAccessResult = CreateIndexAccess(elementAccessSymbol, elementType, elementBinding.ArgumentList.Arguments, elementBinding.GetLocation());
		if (elementAccessResult.HasDiagnostics)
		{
			return elementAccessResult;
		}

		elementAccessResult.Value[0] = new ConditionalAccess(elementAccessResult.Value[0]);

		return Result<List<IPathPart>>.Success(elementAccessResult.Value);
	}

	private Result<List<IPathPart>> HandleBinaryExpression(BinaryExpressionSyntax asExpression)
	{
		var leftResult = ParsePath(asExpression.Left);
		if (leftResult.HasDiagnostics)
		{
			return leftResult;
		}

		var castTo = asExpression.Right;
		var typeInfo = _context.SemanticModel.GetTypeInfo(castTo).Type;
		if (typeInfo == null)
		{
			return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(castTo.GetLocation()));
		}
		;

		leftResult.Value.Add(new Cast(typeInfo.CreateTypeDescription(_enabledNullable)));

		return Result<List<IPathPart>>.Success(leftResult.Value);
	}

	private Result<List<IPathPart>> HandleCastExpression(CastExpressionSyntax castExpression)
	{
		var result = ParsePath(castExpression.Expression);
		if (result.HasDiagnostics)
		{
			return result;
		}

		var typeInfo = _context.SemanticModel.GetTypeInfo(castExpression.Type).Type;
		if (typeInfo == null)
		{
			return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(castExpression.GetLocation()));
		}
		;

		result.Value.Add(new Cast(typeInfo.CreateTypeDescription(_enabledNullable)));

		return Result<List<IPathPart>>.Success(result.Value);
	}

	private Result<List<IPathPart>> HandleDefaultCase()
	{
		return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(_context.Node.GetLocation()));
	}

	private Result<List<IPathPart>> CreateIndexAccess(ISymbol? elementAccessSymbol, ITypeSymbol? typeSymbol, SeparatedSyntaxList<ArgumentSyntax> argumentList, Location location)
	{
		if (argumentList.Count != 1)
		{
			return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(location));
		}

		var indexExpression = argumentList[0].Expression;
		object? indexValue = _context.SemanticModel.GetConstantValue(indexExpression).Value;
		if (indexValue is null)
		{
			return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(indexExpression.GetLocation()));
		}

		var name = elementAccessSymbol.GetIndexerName();
		var isReferenceType = typeSymbol?.IsReferenceType ?? false;
		IPathPart part = new IndexAccess(name, indexValue, !isReferenceType);

		return Result<List<IPathPart>>.Success(new List<IPathPart>([part]));
	}
}
