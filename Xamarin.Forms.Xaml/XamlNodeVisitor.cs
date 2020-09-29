using System;

namespace Xamarin.Forms.Xaml
{
	interface IXamlNodeVisitor
	{
		TreeVisitingMode VisitingMode { get; }
		bool StopOnDataTemplate { get; }
		bool VisitNodeOnDataTemplate { get; }
		bool StopOnResourceDictionary { get; }

		void Visit(ValueNode node, INode parentNode);
		void Visit(MarkupNode node, INode parentNode);
		void Visit(ElementNode node, INode parentNode);
		void Visit(RootNode node, INode parentNode);
		void Visit(ListNode node, INode parentNode);
		bool SkipChildren(INode node, INode parentNode);
		bool IsResourceDictionary(ElementNode node);
	}

	enum TreeVisitingMode
	{
		TopDown,
		BottomUp
	}

	class XamlNodeVisitor : IXamlNodeVisitor
	{
		readonly Action<INode, INode> action;

		public XamlNodeVisitor(Action<INode, INode> action, TreeVisitingMode visitingMode = TreeVisitingMode.TopDown, bool stopOnDataTemplate = false, bool visitNodeOnDataTemplate = true)
		{
			this.action = action;
			VisitingMode = visitingMode;
			StopOnDataTemplate = stopOnDataTemplate;
			VisitNodeOnDataTemplate = visitNodeOnDataTemplate;
		}

		public TreeVisitingMode VisitingMode { get; }
		public bool StopOnDataTemplate { get; }
		public bool StopOnResourceDictionary { get; }
		public bool VisitNodeOnDataTemplate { get; }

		public void Visit(ValueNode node, INode parentNode) => action(node, parentNode);
		public void Visit(MarkupNode node, INode parentNode) => action(node, parentNode);
		public void Visit(ElementNode node, INode parentNode) => action(node, parentNode);
		public void Visit(RootNode node, INode parentNode) => action(node, parentNode);
		public void Visit(ListNode node, INode parentNode) => action(node, parentNode);
		public bool SkipChildren(INode node, INode parentNode) => false;
		public bool IsResourceDictionary(ElementNode node) => false;
	}
}