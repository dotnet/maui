#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Maui.Controls.Xaml;

[DebuggerDisplay("{NamespaceUri}:{Name}")]
class XmlType(string namespaceUri, string name, IList<XmlType> typeArguments)
{
	public string NamespaceUri { get; } = namespaceUri;
	public string Name { get; } = name;
	public IList<XmlType> TypeArguments { get; } = typeArguments;

	public override bool Equals(object obj)
	{
		if (obj is not XmlType other)
			return false;

		return NamespaceUri == other.NamespaceUri
			&& Name == other.Name
			&& (TypeArguments == null && other.TypeArguments == null
				|| TypeArguments != null && other.TypeArguments != null && TypeArguments.SequenceEqual(other.TypeArguments));
	}

	public bool IsOfAnyType(params string[] types)
	{
		if (types == null || types.Length == 0)
			return false;
		if (NamespaceUri != XamlParser.MauiUri && NamespaceUri != XamlParser.MauiGlobalUri)
			return false;
		if (types.Contains(Name))
			return true;
		return false;
	}
	public override int GetHashCode()
	{
		unchecked
		{
#if NETSTANDARD2_0
			int hashCode = NamespaceUri.GetHashCode();
			hashCode = (hashCode * 397) ^ Name.GetHashCode();
			return hashCode;
#else
			return HashCode.Combine(NamespaceUri, Name);
#endif
		}
	}
}
