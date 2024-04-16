
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Microsoft.Maui.Controls.BindingSourceGen;

internal class PathParser
{
    internal PathParser(GeneratorSyntaxContext context)
    {
        Context = context;
    }

    private GeneratorSyntaxContext Context { get; }

    internal (DiagnosticInfo[] diagnostics, List<IPathPart> parts) ParsePath(CSharpSyntaxNode? expressionSyntax)
    {
        return expressionSyntax switch
        {
            IdentifierNameSyntax _ => ([], new List<IPathPart>()),
            MemberAccessExpressionSyntax memberAccess => HandleMemberAccessExpression(memberAccess),
            ElementAccessExpressionSyntax elementAccess => HandleElementAccessExpression(elementAccess),
            ElementBindingExpressionSyntax elementBinding => HandleElementBindingExpression(elementBinding),
            ConditionalAccessExpressionSyntax conditionalAccess => HandleConditionalAccessExpression(conditionalAccess),
            MemberBindingExpressionSyntax memberBinding => HandleMemberBindingExpression(memberBinding),
            ParenthesizedExpressionSyntax parenthesized => ParsePath(parenthesized.Expression),
            BinaryExpressionSyntax asExpression when asExpression.Kind() == SyntaxKind.AsExpression => HandleBinaryExpression(asExpression),
            _ => HandleDefaultCase(),
        };
    }

    private (DiagnosticInfo[] diagnostics, List<IPathPart> parts) HandleMemberAccessExpression(MemberAccessExpressionSyntax memberAccess)
    {
        var (diagnostics, parts) = ParsePath(memberAccess.Expression);
        if (diagnostics.Length > 0)
        {
            return (diagnostics, parts);
        }

        var member = memberAccess.Name.Identifier.Text;
        IPathPart part = new MemberAccess(member);
        parts.Add(part);
        return (diagnostics, parts);
    }

    private (DiagnosticInfo[] diagnostics, List<IPathPart> parts) HandleElementAccessExpression(ElementAccessExpressionSyntax elementAccess)
    {
        var (diagnostics, parts) = ParsePath(elementAccess.Expression);
        if (diagnostics.Length > 0)
        {
            return (diagnostics, parts);
        }

        var elementAccessSymbol = Context.SemanticModel.GetSymbolInfo(elementAccess).Symbol;
        var (elementAccessDiagnostics, elementAccessParts) = HandleElementAccessSymbol(elementAccessSymbol, elementAccess.ArgumentList.Arguments, elementAccess.GetLocation());
        if (elementAccessDiagnostics.Length > 0)
        {
            return (elementAccessDiagnostics, elementAccessParts);
        }

        parts.AddRange(elementAccessParts);
        return (diagnostics, parts);
    }

    private (DiagnosticInfo[] diagnostics, List<IPathPart> parts) HandleConditionalAccessExpression(ConditionalAccessExpressionSyntax conditionalAccess)
    {
        var (diagnostics, parts) = ParsePath(conditionalAccess.Expression);
        if (diagnostics.Length > 0)
        {
            return (diagnostics, parts);
        }

        var (diagnosticNotNull, partsNotNull) = ParsePath(conditionalAccess.WhenNotNull);
        if (diagnosticNotNull.Length > 0)
        {
            return (diagnosticNotNull, partsNotNull);
        }

        parts.AddRange(partsNotNull);
        return (diagnostics, parts);
    }

    private (DiagnosticInfo[] diagnostics, List<IPathPart> parts) HandleMemberBindingExpression(MemberBindingExpressionSyntax memberBinding)
    {
        var member = memberBinding.Name.Identifier.Text;
        IPathPart part = new MemberAccess(member);
        part = new ConditionalAccess(part);

        return ([], new List<IPathPart>([part]));
    }

    private (DiagnosticInfo[] diagnostics, List<IPathPart> parts) HandleElementBindingExpression(ElementBindingExpressionSyntax elementBinding)
    {
        var elementAccessSymbol = Context.SemanticModel.GetSymbolInfo(elementBinding).Symbol;
        var (elementAccessDiagnostics, elementAccessParts) = HandleElementAccessSymbol(elementAccessSymbol, elementBinding.ArgumentList.Arguments, elementBinding.GetLocation());
        if (elementAccessDiagnostics.Length > 0)
        {
            return (elementAccessDiagnostics, elementAccessParts);
        }

        elementAccessParts[0] = new ConditionalAccess(elementAccessParts[0]);
        return (elementAccessDiagnostics, elementAccessParts);
    }

    private (DiagnosticInfo[] diagnostics, List<IPathPart> parts) HandleBinaryExpression(BinaryExpressionSyntax asExpression)
    {
        var (diagnostics, parts) = ParsePath(asExpression.Left);
        if (diagnostics.Length > 0)
        {
            return (diagnostics, parts);
        }

        var castTo = asExpression.Right;
        var typeInfo = Context.SemanticModel.GetTypeInfo(castTo).Type;
        if (typeInfo == null)
        {
            return (new DiagnosticInfo[] { DiagnosticsFactory.UnableToResolvePath(asExpression.GetLocation()) }, new List<IPathPart>());
        };

        parts.Add(new Cast(BindingGenerationUtilities.CreateTypeDescriptionForCast(typeInfo)));
        return (diagnostics, parts);
    }

    private (DiagnosticInfo[] diagnostics, List<IPathPart> parts) HandleDefaultCase()
    {
        return (new DiagnosticInfo[] { DiagnosticsFactory.UnableToResolvePath(Context.Node.GetLocation()) }, new List<IPathPart>());
    }

    private (DiagnosticInfo[], List<IPathPart>) HandleElementAccessSymbol(ISymbol? elementAccessSymbol, SeparatedSyntaxList<ArgumentSyntax> argumentList, Location location)
    {
        if (argumentList.Count != 1)
        {
            return ([DiagnosticsFactory.UnableToResolvePath(location)], []);
        }

        var indexExpression = argumentList[0].Expression;
        object? indexValue = Context.SemanticModel.GetConstantValue(indexExpression).Value;
        if (indexValue is null)
        {
            return ([DiagnosticsFactory.UnableToResolvePath(location)], []);
        }

        var name = GetIndexerName(elementAccessSymbol);
        IPathPart part = new IndexAccess(name, indexValue);

        return ([], [part]);
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