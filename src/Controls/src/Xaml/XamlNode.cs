#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
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

	interface IElementNode : INode, IListNode
	{
		Dictionary<XmlName, INode> Properties { get; }
		List<XmlName> SkipProperties { get; }
		NameScopeRef NameScopeRef { get; }
		XmlType XmlType { get; }
		string NamespaceURI { get; }
	}

	interface IListNode : INode
	{
		List<INode> CollectionItems { get; }
	}

	class NameScopeRef
	{
		public INameScope NameScope { get; set; }
	}

	[DebuggerDisplay("{NamespaceUri}:{Name}")]
	class XmlType
	{
		public XmlType(string namespaceUri, string name, IList<XmlType> typeArguments)
		{
			NamespaceUri = namespaceUri;
			Name = name;
			TypeArguments = typeArguments;
		}

		public string NamespaceUri { get; }
		public string Name { get; }
		public IList<XmlType> TypeArguments { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is not XmlType other)
			{
				return false;
			}

			return
				NamespaceUri == other.NamespaceUri &&
				Name == other.Name &&
				(TypeArguments == null && other.TypeArguments == null ||
				 TypeArguments != null && other.TypeArguments != null && TypeArguments.SequenceEqual(other.TypeArguments));
		}

		public override int GetHashCode()
		{
			unchecked
			{
#if NETSTANDARD2_0
				int hashCode = NamespaceUri.GetHashCode();
				hashCode = (hashCode * 397) ^ Name.GetHashCode();
#else
				int hashCode = NamespaceUri.GetHashCode(StringComparison.Ordinal);
				hashCode = (hashCode * 397) ^ Name.GetHashCode(StringComparison.Ordinal);
#endif
				return hashCode;
			}
		}
	}

	abstract class BaseNode : IXmlLineInfo, INode
	{
		protected BaseNode(IXmlNamespaceResolver namespaceResolver, int linenumber = -1, int lineposition = -1)
		{
			NamespaceResolver = namespaceResolver;
			LineNumber = linenumber;
			LinePosition = lineposition;
		}

		public IXmlNamespaceResolver NamespaceResolver { get; }
		public INode Parent { get; set; }
		public List<string> IgnorablePrefixes { get; set; }
		public int LineNumber { get; set; }
		public int LinePosition { get; set; }

		public bool HasLineInfo() => LineNumber >= 0 && LinePosition >= 0;

		public abstract void Accept(IXamlNodeVisitor visitor, INode parentNode);
		public abstract INode Clone();
	}

	[DebuggerDisplay("{Value}")]
	class ValueNode : BaseNode, IValueNode
	{
		public ValueNode(object value, IXmlNamespaceResolver namespaceResolver, int linenumber = -1, int lineposition = -1)
			: base(namespaceResolver, linenumber, lineposition)
		{
			Value = value;
		}

		public object Value { get; set; }

		public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
		{
			visitor.Visit(this, parentNode);
		}

		public override INode Clone() => new ValueNode(Value, NamespaceResolver, LineNumber, LinePosition)
		{
			IgnorablePrefixes = IgnorablePrefixes
		};
	}

	[DebuggerDisplay("{MarkupString}")]
	class MarkupNode : BaseNode, IValueNode
	{
		public MarkupNode(string markupString, IXmlNamespaceResolver namespaceResolver, int linenumber = -1, int lineposition = -1)
			: base(namespaceResolver, linenumber, lineposition)
		{
			MarkupString = markupString;
		}

		public string MarkupString { get; }

		public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
		{
			visitor.Visit(this, parentNode);
		}

		public override INode Clone() => new MarkupNode(MarkupString, NamespaceResolver, LineNumber, LinePosition)
		{
			IgnorablePrefixes = IgnorablePrefixes
		};
	}


	[DebuggerDisplay("{XmlType.Name}")]
	class ElementNode : BaseNode, IValueNode, IElementNode
	{
		public ElementNode(XmlType type, string namespaceURI, IXmlNamespaceResolver namespaceResolver, int linenumber = -1,
			int lineposition = -1)
			: base(namespaceResolver, linenumber, lineposition)
		{
			Properties = new Dictionary<XmlName, INode>();
			SkipProperties = new List<XmlName>();
			CollectionItems = new List<INode>();
			XmlType = type;
			NamespaceURI = namespaceURI;
		}

		public Dictionary<XmlName, INode> Properties { get; }
		public List<XmlName> SkipProperties { get; }
		public List<INode> CollectionItems { get; }
		public XmlType XmlType { get; }
		public string NamespaceURI { get; }
		public NameScopeRef NameScopeRef { get; set; }

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
			if (parentNode is IElementNode parentElement &&
				parentElement.Properties.TryGetValue(XmlName._CreateContent, out INode createContent) &&
				createContent == this)
				return true;
			return false;
		}

		protected bool SkipChildren(IXamlNodeVisitor visitor, INode node, INode parentNode) =>
			   (visitor.StopOnDataTemplate && IsDataTemplate(parentNode))
			|| (visitor.StopOnResourceDictionary && visitor.IsResourceDictionary(this))
			|| visitor.SkipChildren(node, parentNode);

		protected bool SkipVisitNode(IXamlNodeVisitor visitor, INode parentNode) =>
			!visitor.VisitNodeOnDataTemplate && IsDataTemplate(parentNode);

		public override INode Clone()
		{
			var clone = new ElementNode(XmlType, NamespaceURI, NamespaceResolver, LineNumber, LinePosition)
			{
				IgnorablePrefixes = IgnorablePrefixes
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

	abstract class RootNode : ElementNode
	{
		protected RootNode(XmlType xmlType, IXmlNamespaceResolver nsResolver, int linenumber = -1, int lineposition = -1) : base(xmlType, xmlType.NamespaceUri, nsResolver, linenumber: linenumber, lineposition: lineposition)
		{
		}

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

	class ListNode : BaseNode, IListNode, IValueNode
	{
		public ListNode(IList<INode> nodes, IXmlNamespaceResolver namespaceResolver, int linenumber = -1, int lineposition = -1)
			: base(namespaceResolver, linenumber, lineposition)
		{
			CollectionItems = nodes.ToList();
		}

		public XmlName XmlName { get; set; }
		public List<INode> CollectionItems { get; set; }

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
}
