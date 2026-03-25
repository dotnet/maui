#nullable disable
using System;
using System.Diagnostics;

namespace Microsoft.Maui.Controls.Xaml;

[DebuggerDisplay("{NamespaceURI}:{LocalName}")]
internal readonly struct XmlName(string namespaceUri, string localName)
{
	public static readonly XmlName _CreateContent = new("_", "CreateContent");
	public static readonly XmlName xArguments = new("x", "Arguments");
	public static readonly XmlName xClass = new("x", "Class");
	public static readonly XmlName xClassModifier = new("x", "ClassModifier");
	public static readonly XmlName xDataType = new("x", "DataType");
	public static readonly XmlName xFactoryMethod = new("x", "FactoryMethod");
	public static readonly XmlName xFieldModifier = new("x", "FieldModifier");
	public static readonly XmlName xKey = new("x", "Key");
	public static readonly XmlName xName = new("x", "Name");
	public static readonly XmlName xTypeArguments = new("x", "TypeArguments");
	public static readonly XmlName mcIgnorable = new(XamlParser.McUri, "Ignorable");
	public static readonly XmlName Empty = new();

	public string NamespaceURI { get; } = namespaceUri;
	public string LocalName { get; } = localName;

	public override bool Equals(object obj)
	{
		if (obj == null)
			return false;
		if (obj.GetType() != typeof(XmlName))
			return false;
		var other = (XmlName)obj;
		return NamespaceURI == other.NamespaceURI && LocalName == other.LocalName;
	}

	public bool Equals(string namespaceUri, string localName)
		=> Equals(new XmlName(namespaceUri, localName));

	public override int GetHashCode()
	{
		unchecked
		{
#if NETSTANDARD2_0
			int hashCode = 0;
			if (NamespaceURI != null)
				hashCode = NamespaceURI.GetHashCode();
			if (LocalName != null)
				hashCode = (hashCode * 397) ^ LocalName.GetHashCode();
			return hashCode;
#else
			return HashCode.Combine(NamespaceURI, LocalName);
#endif
		}
	}

	public static bool operator ==(XmlName x1, XmlName x2)
		=> x1.NamespaceURI == x2.NamespaceURI && x1.LocalName == x2.LocalName;

	public static bool operator !=(XmlName x1, XmlName x2)
		=> !(x1 == x2);
}