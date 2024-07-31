using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Maui.Controls.BindingSourceGen;

internal class PathParser
{
    internal PathParser(GeneratorSyntaxContext context, bool enabledNullable)
    {
        Context = context;
        EnabledNullable = enabledNullable;
    }

    private GeneratorSyntaxContext Context { get; }
    private bool EnabledNullable { get; }

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
        var typeInfo = Context.SemanticModel.GetTypeInfo(memberAccess).Type;
        var symbol = Context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;

		if (symbol is IFieldSymbol fieldSymbol && !BindingGenerationUtilities.IsAccessible(fieldSymbol.DeclaredAccessibility))
        {
            return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnaccessibleFieldInPath(memberAccess.GetLocation()));
        }

		if (symbol is IPropertySymbol propertySymbol && !BindingGenerationUtilities.IsAccessible(propertySymbol.DeclaredAccessibility))
        {
            return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnaccessiblePropertyInPath(memberAccess.GetLocation()));
        }

        var isReferenceType = typeInfo?.IsReferenceType ?? false;
        IPathPart part = new MemberAccess(member, !isReferenceType);
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

        var elementAccessSymbol = Context.SemanticModel.GetSymbolInfo(elementAccess).Symbol;
        var elementType = Context.SemanticModel.GetTypeInfo(elementAccess).Type;

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
        var typeInfo = Context.SemanticModel.GetTypeInfo(memberBinding).Type;
        var isReferenceType = typeInfo?.IsReferenceType ?? false;
        IPathPart part = new MemberAccess(member, !isReferenceType);
        part = new ConditionalAccess(part);

        return Result<List<IPathPart>>.Success(new List<IPathPart>([part]));
    }

    private Result<List<IPathPart>> HandleElementBindingExpression(ElementBindingExpressionSyntax elementBinding)
    {
        var elementAccessSymbol = Context.SemanticModel.GetSymbolInfo(elementBinding).Symbol;
        var elementType = Context.SemanticModel.GetTypeInfo(elementBinding).Type;

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
        var typeInfo = Context.SemanticModel.GetTypeInfo(castTo).Type;
        if (typeInfo == null)
        {
            return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(castTo.GetLocation()));
        };

        leftResult.Value.Add(new Cast(BindingGenerationUtilities.CreateTypeDescription(typeInfo, EnabledNullable)));

        return Result<List<IPathPart>>.Success(leftResult.Value);
    }

    private Result<List<IPathPart>> HandleCastExpression(CastExpressionSyntax castExpression)
    {
        var result = ParsePath(castExpression.Expression);
        if (result.HasDiagnostics)
        {
            return result;
        }

        var typeInfo = Context.SemanticModel.GetTypeInfo(castExpression.Type).Type;
        if (typeInfo == null)
        {
            return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(castExpression.GetLocation()));
        };

        result.Value.Add(new Cast(BindingGenerationUtilities.CreateTypeDescription(typeInfo, EnabledNullable)));

        return Result<List<IPathPart>>.Success(result.Value);
    }

    private Result<List<IPathPart>> HandleDefaultCase()
    {
        return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(Context.Node.GetLocation()));
    }

    private Result<List<IPathPart>> CreateIndexAccess(ISymbol? elementAccessSymbol, ITypeSymbol? typeSymbol, SeparatedSyntaxList<ArgumentSyntax> argumentList, Location location)
    {
        if (argumentList.Count != 1)
        {
            return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(location));
        }

        var indexExpression = argumentList[0].Expression;
        object? indexValue = Context.SemanticModel.GetConstantValue(indexExpression).Value;
        if (indexValue is null)
        {
            return Result<List<IPathPart>>.Failure(DiagnosticsFactory.UnableToResolvePath(indexExpression.GetLocation()));
        }

        var name = GetIndexerName(elementAccessSymbol);
        var isReferenceType = typeSymbol?.IsReferenceType ?? false;
        IPathPart part = new IndexAccess(name, indexValue, !isReferenceType);

        return Result<List<IPathPart>>.Success(new List<IPathPart>([part]));
    }

    private string GetIndexerName(ISymbol? elementAccessSymbol)
    {
        const string defaultName = "Item";

        if (elementAccessSymbol is not IPropertySymbol propertySymbol)
        {
            return defaultName;
        }

        var containgType = propertySymbol.ContainingType;
        if (containgType == null)
        {
            return defaultName;
        }

        var defaultMemberAttribute = GetAttribute(containgType, "DefaultMemberAttribute");
        if (defaultMemberAttribute != null)
        {
            return GetAttributeValue(defaultMemberAttribute);
        }

        var indexerNameAttr = GetAttribute(propertySymbol, "IndexerNameAttribute");
        if (indexerNameAttr != null)
        {
            return GetAttributeValue(indexerNameAttr);
        }

        return defaultName;

        AttributeData? GetAttribute(ISymbol symbol, string attributeName)
        {
            return symbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == attributeName);
        }

        string GetAttributeValue(AttributeData attribute)
        {
            return (attribute.ConstructorArguments.Length > 0 ? attribute.ConstructorArguments[0].Value as string : null) ?? defaultName;
        }
    }
}
