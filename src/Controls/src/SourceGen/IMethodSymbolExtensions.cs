using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

static class IMethodSymbolExtensions
{
    public static bool MatchXArguments(this IMethodSymbol method, IElementNode enode, SourceGenContext context, out IList<(INode node, ITypeSymbol type, ITypeSymbol? converter)>? parameters)
    {
        parameters = null;
        if (!enode.Properties.TryGetValue(XmlName.xArguments, out INode? value))
            return !method.Parameters.Any();

        var nodeparameters = new List<INode>();
		if (value is ElementNode node)
			nodeparameters.Add(node);

		if (value is ListNode list)
			foreach (var n in list.CollectionItems)
				nodeparameters.Add(n);

		if (method.Parameters.Length != nodeparameters.Count)
            return false;

        parameters = [];
        for (var i = 0; i < method.Parameters.Length; i++)
        {
            var paramType = method.Parameters[i].Type;
            // var genParam = paramType as ITypeParameterSymbol;
            // if (genParam != null)
            // {
            //     var index = genParam.DeclaringType.TypeParameters.IndexOf(genParam);
            //     paramType = (declaringTypeRef as INamedTypeSymbol).TypeArguments[index];
            // }

            var argType = context.Variables[(IElementNode)nodeparameters[i]].Type;
            if (!argType.InheritsFrom(paramType))
            {
                parameters = null;
                return false;
            }
            if (paramType.IsInterface() && !argType.Implements(paramType))
            {
                parameters = null;
                return false;
            }
            parameters.Add((nodeparameters[i], paramType, null));
        }
        return true;
    }

	public static IEnumerable<string> ToMethodParameters(this IEnumerable<(INode node, ITypeSymbol type, ITypeSymbol? converter)> parameters, SourceGenContext context)
        => parameters.Select(p => p.node is ValueNode vn ? vn.ConvertTo(p.type, p.converter, context) : p.node is ElementNode en ? context.Variables[en].Name : "null");
}