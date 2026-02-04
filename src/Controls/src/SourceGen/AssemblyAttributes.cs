using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.Controls.SourceGen;

class AssemblyAttributes(IReadOnlyList<XmlnsDefinitionAttribute> xmlnsDefinitions, IReadOnlyList<XmlnsPrefixAttribute> xmlnsPrefixes, IReadOnlyList<XmlnsDefinitionAttribute> globalGeneratedXmlnsDefinitions, IReadOnlyList<IAssemblySymbol> internalsVisible, Dictionary<string, List<string>> clrNamespacesForXmlns)
{
	public static readonly AssemblyAttributes Empty = new([], [], [], [], []);

	public IReadOnlyList<XmlnsDefinitionAttribute> XmlnsDefinitions { get; } = xmlnsDefinitions;
	public IReadOnlyList<XmlnsPrefixAttribute> XmlnsPrefixes { get; } = xmlnsPrefixes;
	public IReadOnlyList<XmlnsDefinitionAttribute> GlobalGeneratedXmlnsDefinitions { get; } = globalGeneratedXmlnsDefinitions;
	public IReadOnlyList<IAssemblySymbol> InternalsVisible { get; } = internalsVisible;
	public Dictionary<string, List<string>> ClrNamespacesForXmlns { get; } = clrNamespacesForXmlns;
	public bool AllowImplicitXmlns => true; // Always enabled in .NET 11+
}