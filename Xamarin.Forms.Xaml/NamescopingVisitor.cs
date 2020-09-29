using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml
{
	class NamescopingVisitor : IXamlNodeVisitor
	{
		readonly Dictionary<INode, NameScopeRef> _scopes = new Dictionary<INode, NameScopeRef>();

		public NamescopingVisitor(HydrationContext context)
		{
		}

		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => false;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => true;
		public bool SkipChildren(INode node, INode parentNode) => false;
		public bool IsResourceDictionary(ElementNode node) => false;

		public void Visit(ValueNode node, INode parentNode) => _scopes[node] = _scopes[parentNode];
		public void Visit(MarkupNode node, INode parentNode) => _scopes[node] = _scopes[parentNode];

		public void Visit(ElementNode node, INode parentNode)
			=> _scopes[node] = node.NameScopeRef = (parentNode == null || IsDataTemplate(node, parentNode) || IsStyle(node, parentNode) || IsVisualStateGroupList(node))
								   ? new NameScopeRef { NameScope = new NameScope() }
								   : _scopes[parentNode];

		public void Visit(RootNode node, INode parentNode) => _scopes[node] = node.NameScopeRef = new NameScopeRef { NameScope = new NameScope() };

		public void Visit(ListNode node, INode parentNode) =>
			_scopes[node] = _scopes[parentNode];

		static bool IsDataTemplate(INode node, INode parentNode)
		{
			if (parentNode is IElementNode parentElement
				&& parentElement.Properties.TryGetValue(XmlName._CreateContent, out var createContent)
				&& createContent == node)
				return true;
			return false;
		}

		static bool IsStyle(INode node, INode parentNode) => (parentNode as ElementNode)?.XmlType.Name == "Style";
		static bool IsVisualStateGroupList(ElementNode node) => node?.XmlType.Name == "VisualStateGroup" && node?.Parent is IListNode;
	}
}