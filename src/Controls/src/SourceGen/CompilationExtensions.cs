using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

static class CompilationExtensions
{
	public static ImmutableArray<IAssemblySymbol> GetAllAssemblies(this Compilation compilation)
	{
		var assemblies = new List<IAssemblySymbol>
		{
			compilation.Assembly,
			compilation.ObjectType.ContainingAssembly
		};
		assemblies.AddRange(compilation.SourceModule.ReferencedAssemblySymbols);
		return assemblies.ToImmutableArray();
	}

	public static IAssemblySymbol? GetAssembly(this Compilation compilation, string assemblyName)
		=> compilation.GetAllAssemblies().FirstOrDefault(a => a.Identity.Name == assemblyName);
}