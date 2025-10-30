using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

class AssemblyCaches(IReadOnlyList<XmlnsDefinitionAttribute> xmlnsDefinitions, IReadOnlyList<XmlnsPrefixAttribute> xmlnsPrefixes, IReadOnlyList<XmlnsDefinitionAttribute> globalGeneratedXmlnsDefinitions, IReadOnlyList<IAssemblySymbol> internalsVisible, bool allowImplicitXmlns)
{
	public static readonly AssemblyCaches Empty = new([], [], [], [], false);

	public IReadOnlyList<XmlnsDefinitionAttribute> XmlnsDefinitions { get; } = xmlnsDefinitions;
	public IReadOnlyList<XmlnsPrefixAttribute> XmlnsPrefixes { get; } = xmlnsPrefixes;
	public IReadOnlyList<XmlnsDefinitionAttribute> GlobalGeneratedXmlnsDefinitions { get; } = globalGeneratedXmlnsDefinitions;
	public IReadOnlyList<IAssemblySymbol> InternalsVisible { get; } = internalsVisible;
	public bool AllowImplicitXmlns { get; } = allowImplicitXmlns;
}