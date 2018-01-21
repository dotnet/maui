using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Xaml
{
	class RegisterXNamesVisitor : IXamlNodeVisitor
	{
		public RegisterXNamesVisitor(HydrationContext context)
		{
			Values = context.Values;
		}

		Dictionary<INode, object> Values { get; }

		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;
		public bool SkipChildren(INode node, INode parentNode) => false;

		public void Visit(ValueNode node, INode parentNode)
		{
			if (!IsXNameProperty(node, parentNode))
				return;
			try
			{
				((IElementNode)parentNode).Namescope.RegisterName((string)node.Value, Values[parentNode]);
			}
			catch (ArgumentException ae)
			{
				if (ae.ParamName != "name")
					throw ae;
				throw new XamlParseException($"An element with the name \"{(string)node.Value}\" already exists in this NameScope", node);
			}
			var element = Values[parentNode] as Element;
			if (element != null)
				element.StyleId = element.StyleId ?? (string)node.Value;
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ElementNode node, INode parentNode)
		{
		}

		public void Visit(RootNode node, INode parentNode)
		{
		}

		public void Visit(ListNode node, INode parentNode)
		{
		}

		static bool IsXNameProperty(ValueNode node, INode parentNode)
		{
			var parentElement = parentNode as IElementNode;
			INode xNameNode;
			if (parentElement != null && parentElement.Properties.TryGetValue(XmlName.xName, out xNameNode) && xNameNode == node)
				return true;
			return false;
		}
	}
}