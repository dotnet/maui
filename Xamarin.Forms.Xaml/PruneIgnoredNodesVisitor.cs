using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Forms.Xaml
{
	class PruneIgnoredNodesVisitor : IXamlNodeVisitor
	{
		public bool StopOnDataTemplate {
			get { return false; }
		}

		public bool StopOnResourceDictionary {
			get { return false; }
		}

		public bool VisitChildrenFirst {
			get { return false; }
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			foreach (var propertyKvp in node.Properties)
			{
				var propertyName = propertyKvp.Key;
				var propertyValue = (propertyKvp.Value as ValueNode)?.Value as string;
				if (propertyValue == null)
					continue;
				if (propertyName.NamespaceURI != "http://schemas.openxmlformats.org/markup-compatibility/2006" ||
					propertyName.LocalName != "Ignorable")
					continue;
				(parentNode.IgnorablePrefixes ?? (parentNode.IgnorablePrefixes = new List<string>())).AddRange(propertyValue.Split(','));
			}

			foreach (var propertyKvp in node.Properties.ToList())
			{
				// skip d:foo="bar"
				var prefix = node.NamespaceResolver.LookupPrefix(propertyKvp.Key.NamespaceURI);
				if (node.SkipPrefix(prefix))
					node.Properties.Remove(propertyKvp.Key);
				var propNs = (propertyKvp.Value as IElementNode)?.NamespaceURI ?? "";
				var propPrefix = node.NamespaceResolver.LookupPrefix(propNs);
				if (node.SkipPrefix(propPrefix))
					node.Properties.Remove(propertyKvp.Key);
			}

			foreach (var prop in node.CollectionItems.ToList())
			{
				var propNs = (prop as IElementNode)?.NamespaceURI ?? "";
				var propPrefix = node.NamespaceResolver.LookupPrefix(propNs);
				if (node.SkipPrefix(propPrefix))
					node.CollectionItems.Remove(prop);
			}

			if (node.SkipPrefix(node.NamespaceResolver.LookupPrefix(node.NamespaceURI)))
			{
				node.Properties.Clear();
				node.CollectionItems.Clear();
			}
		}

		public void Visit(RootNode node, INode parentNode)
		{
			Visit((ElementNode)node, node);
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ListNode node, INode parentNode)
		{
			foreach (var prop in node.CollectionItems.ToList())
			{
				var propNs = (prop as IElementNode)?.NamespaceURI ?? "";
				var propPrefix = node.NamespaceResolver.LookupPrefix(propNs);
				if (node.SkipPrefix(propPrefix))
					node.CollectionItems.Remove(prop);
			}
		}

		public void Visit(ValueNode node, INode parentNode)
		{
		}
	}
}