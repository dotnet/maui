using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

class AssemblyCaches
{
	public static readonly AssemblyCaches Empty = new(Array.Empty<XmlnsDefinitionAttribute>(), Array.Empty<IAssemblySymbol>());

	public AssemblyCaches(IReadOnlyList<XmlnsDefinitionAttribute> xmlnsDefinitions, IReadOnlyList<IAssemblySymbol> internalsVisible)
	{
		XmlnsDefinitions = xmlnsDefinitions;
		InternalsVisible = internalsVisible;
	}

	public IReadOnlyList<XmlnsDefinitionAttribute> XmlnsDefinitions { get; }

	public IReadOnlyList<IAssemblySymbol> InternalsVisible { get; }
}