using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml
{
	internal interface INode
	{
		List<string> IgnorablePrefixes { get; set; }

		IXmlNamespaceResolver NamespaceResolver { get; }

		INode Parent { get; set; }

		void Accept(IXamlNodeVisitor visitor, INode parentNode);
		INode Clone();
	}

	internal interface IValueNode : INode
	{
	}

	internal interface IElementNode : INode, IListNode
	{
		Dictionary<XmlName, INode> Properties { get; }

		List<XmlName> SkipProperties { get; }

		INameScope Namescope { get; }

		XmlType XmlType { get; }

		string NamespaceURI { get; }
	}

	internal interface IListNode : INode
	{
		List<INode> CollectionItems { get; }
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
		public IList<XmlType> TypeArguments { get; }
	}

	internal abstract class BaseNode : IXmlLineInfo, INode
	{
		protected BaseNode(IXmlNamespaceResolver namespaceResolver, int linenumber = -1, int lineposition = -1)
		{
			NamespaceResolver = namespaceResolver;
			LineNumber = linenumber;
			LinePosition = lineposition;
		}

		public IXmlNamespaceResolver NamespaceResolver { get; }

		public abstract void Accept(IXamlNodeVisitor visitor, INode parentNode);

		public INode Parent { get; set; }

		public List<string> IgnorablePrefixes { get; set; }

		public bool HasLineInfo()
		{
			return LineNumber >= 0 && LinePosition >= 0;
		}

		public int LineNumber { get; set; }

		public int LinePosition { get; set; }

		public abstract INode Clone();
	}

	[DebuggerDisplay("{Value}")]
	internal class ValueNode : BaseNode, IValueNode
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

		public override INode Clone()
		{
			return new ValueNode(Value, NamespaceResolver, LineNumber, LinePosition) {
				IgnorablePrefixes = IgnorablePrefixes
			};
		}
	}

	[DebuggerDisplay("{MarkupString}")]
	internal class MarkupNode : BaseNode, IValueNode
	{
		public MarkupNode(string markupString, IXmlNamespaceResolver namespaceResolver, int linenumber = -1,
			int lineposition = -1)
			: base(namespaceResolver, linenumber, lineposition)
		{
			MarkupString = markupString;
		}

		public string MarkupString { get; }

		public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
		{
			visitor.Visit(this, parentNode);
		}

		public override INode Clone()
		{
			return new MarkupNode(MarkupString, NamespaceResolver, LineNumber, LinePosition) {
				IgnorablePrefixes = IgnorablePrefixes
			};
		}
	}

	internal class ElementNode : BaseNode, IValueNode, IElementNode
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

		public INameScope Namescope { get; set; }

		public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
		{
			if (!visitor.VisitChildrenFirst)
				visitor.Visit(this, parentNode);
			if ((!visitor.StopOnDataTemplate || !IsDataTemplate(this, parentNode)) &&
			    (!visitor.StopOnResourceDictionary || !IsResourceDictionary(this, parentNode)))
			{
				foreach (var node in Properties.Values.ToList())
					node.Accept(visitor, this);
				foreach (var node in CollectionItems)
					node.Accept(visitor, this);
			}
			if (visitor.VisitChildrenFirst)
				visitor.Visit(this, parentNode);
		}

		internal static bool IsDataTemplate(INode node, INode parentNode)
		{
			var parentElement = parentNode as IElementNode;
			INode createContent;
			if (parentElement != null && parentElement.Properties.TryGetValue(XmlName._CreateContent, out createContent) &&
			    createContent == node)
				return true;
			return false;
		}

		internal static bool IsResourceDictionary(INode node, INode parentNode)
		{
			var enode = node as ElementNode;
			return enode.XmlType.Name == "ResourceDictionary";
		}

		public override INode Clone()
		{
			var clone = new ElementNode(XmlType, NamespaceURI, NamespaceResolver, LineNumber, LinePosition) {
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

	internal abstract class RootNode : ElementNode
	{
		protected RootNode(XmlType xmlType, IXmlNamespaceResolver nsResolver) : base(xmlType, xmlType.NamespaceUri, nsResolver)
		{
		}

		public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
		{
			if (!visitor.VisitChildrenFirst)
				visitor.Visit(this, parentNode);
			if ((!visitor.StopOnDataTemplate || !IsDataTemplate(this, parentNode)) &&
				(!visitor.StopOnResourceDictionary || !IsResourceDictionary(this, parentNode)))
			{
				foreach (var node in Properties.Values.ToList())
					node.Accept(visitor, this);
				foreach (var node in CollectionItems)
					node.Accept(visitor, this);
			}
			if (visitor.VisitChildrenFirst)
				visitor.Visit(this, parentNode);
		}
	}

	internal class ListNode : BaseNode, IListNode, IValueNode
	{
		public ListNode(IList<INode> nodes, IXmlNamespaceResolver namespaceResolver, int linenumber = -1,
			int lineposition = -1) : base(namespaceResolver, linenumber, lineposition)
		{
			CollectionItems = nodes.ToList();
		}

		public XmlName XmlName { get; set; }

		public List<INode> CollectionItems { get; set; }

		public override void Accept(IXamlNodeVisitor visitor, INode parentNode)
		{
			if (!visitor.VisitChildrenFirst)
				visitor.Visit(this, parentNode);
			foreach (var node in CollectionItems)
				node.Accept(visitor, this);
			if (visitor.VisitChildrenFirst)
				visitor.Visit(this, parentNode);
		}

		public override INode Clone()
		{
			var items = new List<INode>();
			foreach (var p in CollectionItems)
				items.Add(p.Clone());
			return new ListNode(items, NamespaceResolver, LineNumber, LinePosition) {
				IgnorablePrefixes = IgnorablePrefixes
			};
		}
	}

	internal static class INodeExtensions
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