using System.Linq;

namespace Xamarin.Forms.Xaml
{
	class RemoveDuplicateDesignNodes : IXamlNodeVisitor
	{
		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => false;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => true;
		public bool SkipChildren(INode node, INode parentNode) => false;
		public bool IsResourceDictionary(ElementNode node) => false;

		public void Visit(ValueNode node, INode parentNode)
		{
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			if (node.Properties == null || node.Properties.Count == 0)
				return;
			var props = node.Properties.ToList();
			for (var i = 0; i < props.Count; i++)
			{
				var key = props[i].Key;
				if (key.NamespaceURI != XamlParser.XFDesignUri)
					continue;
				var k = new XmlName(XamlParser.XFUri, key.LocalName);
				if (node.Properties.Remove(k))
					continue;
				if (node.NamespaceResolver.LookupPrefix(XamlParser.XFUri) == "")
					node.Properties.Remove(new XmlName("", k.LocalName));
			}
		}

		public void Visit(RootNode node, INode parentNode)
		{
			Visit((ElementNode)node, parentNode);
		}

		public void Visit(ListNode node, INode parentNode)
		{
		}
	}
}