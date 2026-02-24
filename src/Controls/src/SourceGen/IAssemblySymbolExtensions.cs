using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

static class IAssemblySymbolExtensions
{
	public static ImmutableArray<AttributeData> GetAttributes(this IAssemblySymbol symbol, ITypeSymbol attributeType)
		=> symbol.GetAttributes().Where(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass, attributeType)).ToImmutableArray();
}