using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.SourceGen;
static class ISymbolExtensions
{
    //FIXME use IMutable instead of IEnumerable
    public static bool IsPublic(this ISymbol symbol)
        => symbol.DeclaredAccessibility == Accessibility.Public;

    public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, string name)
    {
        foreach (var attribute in symbol.GetAttributes()) {
            if (attribute.AttributeClass?.ToString() == name) {
                yield return attribute;
            }
        }
    }
}