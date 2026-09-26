#nullable disable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml;

interface INode
{
	List<string> IgnorablePrefixes { get; set; }
	IXmlNamespaceResolver NamespaceResolver { get; }
	INode Parent { get; set; }

	void Accept(IXamlNodeVisitor visitor, INode parentNode);
	INode Clone();
}

interface IValueNode : INode
{
}


interface IListNode : INode
{
	List<INode> CollectionItems { get; }
}

class NameScopeRef
{
	public INameScope NameScope { get; set; }
}

abstract class BaseNode(IXmlNamespaceResolver namespaceResolver, int linenumber = -1, int lineposition = -1) : IXmlLineInfo, INode
{
	public IXmlNamespaceResolver NamespaceResolver { get; } = namespaceResolver;
	public INode Parent { get; set; }
	public List<string> IgnorablePrefixes { get; set; }
	public int LineNumber { get; set; } = linenumber;
	public int LinePosition { get; set; } = lineposition;

	public bool HasLineInfo() => LineNumber >= 0 && LinePosition >= 0;

	public abstract void Accept(IXamlNodeVisitor visitor, INode parentNode);
	public abstract INode Clone();
}

[DebuggerDisplay("{Value}")]
class ValueNode(object value, IXmlNamespaceResolver namespaceResolver, int linenumber = -1, int lineposition = -1) : BaseNode(namespaceResolver, linenumber, lineposition), IValueNode
{
	public object Value { get; set; } = value;

	/// <summary>
	/// Indicates that this ValueNode was created from an escaped markup string (e.g., "{}{Foo}").
	/// When true, the Value should be treated as a literal and not processed as a C# expression.
	/// </summary>
	public bool IsEscaped { get; init; }

	public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
		=> visitor.Visit(this, parentNode);

	public override INode Clone()
		=> new ValueNode(Value, NamespaceResolver, LineNumber, LinePosition)
		{
			IgnorablePrefixes = IgnorablePrefixes,
			IsEscaped = IsEscaped
		};
}

[DebuggerDisplay("{MarkupString}")]
class MarkupNode(string markupString, IXmlNamespaceResolver namespaceResolver, int linenumber = -1, int lineposition = -1) : BaseNode(namespaceResolver, linenumber, lineposition), IValueNode
{
	public string MarkupString { get; } = markupString;

	public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
		=> visitor.Visit(this, parentNode);

	public override INode Clone()
		=> new MarkupNode(MarkupString, NamespaceResolver, LineNumber, LinePosition)
		{
			IgnorablePrefixes = IgnorablePrefixes
		};
}

static class XmlNameExtensions
{
	public static bool TryGetValue(this Dictionary<XmlName, INode> properties, string name, out INode node, out XmlName xmlName)
	{
		xmlName = new XmlName("", name);
		if (properties.TryGetValue(xmlName, out node))
			return true;

#if NETSTANDARD2_0
		var kvp = properties.FirstOrDefault(kvp => kvp.Key.LocalName == name);
		if (kvp.Key != null)
		{
			xmlName = kvp.Key;
			node = kvp.Value;
		}
#else
		(xmlName, node) = properties.FirstOrDefault(kvp => kvp.Key.LocalName == name);
#endif
		return node != null;
	}

	public static bool TryGetValue(this Dictionary<XmlName, INode> properties, string name, out INode node) => properties.TryGetValue(name, out node, out _);
}

[DebuggerDisplay("{XmlType.Name}")]
class ElementNode(XmlType type, string namespaceURI, IXmlNamespaceResolver namespaceResolver, int linenumber = -1,
	int lineposition = -1) : BaseNode(namespaceResolver, linenumber, lineposition), IValueNode, INode, IListNode
{
	public Dictionary<XmlName, INode> Properties { get; } = [];
	public List<XmlName> SkipProperties { get; } = [];
	public List<INode> CollectionItems { get; } = [];
	public XmlType XmlType { get; } = type;
	public string NamespaceURI { get; } = namespaceURI;
	public NameScopeRef NameScopeRef { get; set; }
	
	/// <summary>
	/// When true, this node represents a default value for an OnPlatform element
	/// where no matching platform was found. SourceGen should generate default(T)
	/// instead of trying to instantiate the type.
	/// </summary>
	public bool IsOnPlatformDefaultValue { get; set; }

	public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
	{
		if (visitor.VisitingMode == TreeVisitingMode.TopDown && !SkipVisitNode(visitor, parentNode))
			visitor.Visit(this, parentNode);

		if (!SkipChildren(visitor, this, parentNode))
		{
			foreach (var node in Properties.Values.ToArray())
				node.Accept(visitor, this);
			foreach (var node in CollectionItems.ToArray())
				node.Accept(visitor, this);
		}

		if (visitor.VisitingMode == TreeVisitingMode.BottomUp && !SkipVisitNode(visitor, parentNode))
			visitor.Visit(this, parentNode);
	}

	bool IsDataTemplate(INode parentNode)
	{
		if (parentNode is ElementNode parentElement &&
			parentElement.Properties.TryGetValue(XmlName._CreateContent, out INode createContent) &&
			createContent == this)
			return true;
		return false;
	}

	bool IsStyleContent(IXamlNodeVisitor visitor, INode parentNode)
	{
		if (parentNode is ElementNode parentElement &&
			parentElement.Properties.TryGetValue(XmlName._StyleContent, out INode styleContent) &&
			styleContent == this)
			return true;
		// Also check if parent is a Style (detected by visitor)
		if (parentNode is ElementNode parentEl && visitor.IsStyle(parentEl))
			return true;
		return false;
	}

	protected bool SkipChildren(IXamlNodeVisitor visitor, INode node, INode parentNode) =>
		   (visitor.StopOnDataTemplate && IsDataTemplate(parentNode))
		|| (visitor.StopOnResourceDictionary && visitor.IsResourceDictionary(this))
		|| (visitor.StopOnStyle && visitor.IsStyle(this))  // Skip children when THIS node is a Style
		|| visitor.SkipChildren(node, parentNode);

	protected bool SkipVisitNode(IXamlNodeVisitor visitor, INode parentNode) =>
		   (!visitor.VisitNodeOnDataTemplate && IsDataTemplate(parentNode))
		|| (!visitor.VisitNodeOnStyle && IsStyleContent(visitor, parentNode));  // Skip visiting when parent is a Style

	public override INode Clone()
	{
		var clone = new ElementNode(XmlType, NamespaceURI, NamespaceResolver, LineNumber, LinePosition)
		{
			IgnorablePrefixes = IgnorablePrefixes,
			IsOnPlatformDefaultValue = IsOnPlatformDefaultValue
		};
		foreach (var kvp in Properties)
			clone.Properties.Add(kvp.Key, kvp.Value.Clone());
		foreach (var p in SkipProperties)
			clone.SkipProperties.Add(p);
		foreach (var p in CollectionItems)
			clone.CollectionItems.Add(p.Clone());
		return clone;
	}
}

abstract class RootNode(XmlType xmlType, IXmlNamespaceResolver nsResolver, int linenumber = -1, int lineposition = -1) : ElementNode(xmlType, xmlType.NamespaceUri, nsResolver, linenumber: linenumber, lineposition: lineposition)
{
	public List<(string message, int lineNumber, int linePosition)> Warnings { get; } = new();

	public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
	{
		if (visitor.VisitingMode == TreeVisitingMode.TopDown && !SkipVisitNode(visitor, parentNode))
			visitor.Visit(this, parentNode);

		if (!SkipChildren(visitor, this, parentNode))
		{
			foreach (var node in Properties.Values.ToList())
				node.Accept(visitor, this);
			foreach (var node in CollectionItems.ToList())
				node.Accept(visitor, this);
		}

		if (visitor.VisitingMode == TreeVisitingMode.BottomUp && !SkipVisitNode(visitor, parentNode))
			visitor.Visit(this, parentNode);
	}
}

class ListNode(IList<INode> nodes, IXmlNamespaceResolver namespaceResolver, int linenumber = -1, int lineposition = -1) : BaseNode(namespaceResolver, linenumber, lineposition), IListNode, IValueNode
{
	public XmlName XmlName { get; set; }
	public List<INode> CollectionItems { get; set; } = [.. nodes];

	public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
	{
		if (visitor.VisitingMode == TreeVisitingMode.TopDown)
			visitor.Visit(this, parentNode);
		foreach (var node in CollectionItems)
			node.Accept(visitor, this);
		if (visitor.VisitingMode == TreeVisitingMode.BottomUp)
			visitor.Visit(this, parentNode);
	}

	public override INode Clone()
	{
		var items = new List<INode>();
		foreach (var p in CollectionItems)
			items.Add(p.Clone());
		return new ListNode(items, NamespaceResolver, LineNumber, LinePosition)
		{
			IgnorablePrefixes = IgnorablePrefixes
		};
	}
}

static class INodeExtensions
{
	public static bool SkipPrefix(this INode node, string prefix)
	{
		do
		{
			if (node.IgnorablePrefixes != null && node.IgnorablePrefixes.Contains(prefix))
				return true;
			node = node.Parent;
		} while (node != null);
		return false;
	}
}
