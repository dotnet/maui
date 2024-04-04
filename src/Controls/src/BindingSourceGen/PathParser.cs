
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Microsoft.Maui.Controls.BindingSourceGen;

internal class PathParser
{
    internal static bool ParsePath(CSharpSyntaxNode? expressionSyntax, bool enabledNullable, GeneratorSyntaxContext context, List<IPathPart> parts, bool isNodeNullable = false)
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

            IPathPart part = new MemberAccess(member);
            if (isNodeNullable || BindingGenerationUtilities.IsTypeNullable(typeInfo, enabledNullable))
            {
                part = new ConditionalAccess(part);
            }

            parts.Add(part);
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
            IIndex? indexValue = context.SemanticModel.GetConstantValue(indexExpression).Value switch
            {
                int i => new NumericIndex(i),
                string s => new StringIndex(s),
                _ => null
            };

            if (indexValue is null)
            {
                return false;
            }

            if (!ParsePath(elementAccess.Expression, enabledNullable, context, parts))
            {
                return false;
            }

            var defaultMemberName = "Item"; // TODO we need to check the value of the `[DefaultMemberName]` attribute on the member type
            IPathPart part = new IndexAccess(defaultMemberName, indexValue);
            if (isNodeNullable || BindingGenerationUtilities.IsTypeNullable(typeInfo, enabledNullable))
            {
                part = new ConditionalAccess(part);
            }
            parts.Add(part);
            return true;
        }
        else if (expressionSyntax is ConditionalAccessExpressionSyntax conditionalAccess)
        {
            return ParsePath(conditionalAccess.Expression, enabledNullable, context, parts, isNodeNullable: true) &&
            ParsePath(conditionalAccess.WhenNotNull, enabledNullable, context, parts);
        }
        else if (expressionSyntax is MemberBindingExpressionSyntax memberBinding)
        {
            var member = memberBinding.Name.Identifier.Text;
            IPathPart part = new MemberAccess(member);
            if (isNodeNullable)
            {
                part = new ConditionalAccess(part);
            }
            parts.Add(part);
            return true;
        }
        else if (expressionSyntax is ParenthesizedExpressionSyntax parenthesized)
        {
            return ParsePath(parenthesized.Expression, enabledNullable, context, parts);
        }
        else if (expressionSyntax is BinaryExpressionSyntax asExpression && asExpression.Kind() == SyntaxKind.AsExpression)
        {
            var castTo = asExpression.Right;
            var typeInfo = context.SemanticModel.GetTypeInfo(castTo).Type;
            if (typeInfo == null)
            {
                return false;
            };

            if (!ParsePath(asExpression.Left, enabledNullable, context, parts))
            {
                return false;
            }

            var lastPart = parts.Last();
            parts.RemoveAt(parts.Count - 1);
            parts.Add(new Cast(lastPart, BindingGenerationUtilities.CreateTypeNameFromITypeSymbol(typeInfo, enabledNullable)));
            return true;
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
}