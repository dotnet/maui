using System;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml
{
	class RegisterXNamesVisitor : IXamlNodeVisitor
	{
		public RegisterXNamesVisitor(HydrationContext context)
		{
			Context = context;
			Values = context.Values;
		}

		Dictionary<INode, object> Values { get; }
		HydrationContext Context { get; }
		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;
		public bool SkipChildren(INode node, INode parentNode) => false;
		public bool IsResourceDictionary(ElementNode node) => typeof(ResourceDictionary).IsAssignableFrom(Context.Types[node]);

		public void Visit(ValueNode node, INode parentNode)
		{
			if (!IsXNameProperty(node, parentNode))
				return;

			try
			{
				((IElementNode)parentNode).NameScopeRef.NameScope.RegisterName((string)node.Value, Values[parentNode]);
			}
			catch (ArgumentException ae)
			{
				if (ae.ParamName != "name")
					throw ae;
				var xpe = new XamlParseException($"An element with the name \"{(string)node.Value}\" already exists in this NameScope", node);
				if (Context.ExceptionHandler != null)
				{
					Context.ExceptionHandler(xpe);
					return;
				}
				throw xpe;
			}
			catch (KeyNotFoundException knfe)
			{
				if (Context.ExceptionHandler != null)
				{
					Context.ExceptionHandler(knfe);
					return;
				}
				throw knfe;
			}

			if (Values[parentNode] is Element element)
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
			=> parentNode is IElementNode parentElement && parentElement.Properties.TryGetValue(XmlName.xName, out INode xNameNode) && xNameNode == node;
	}
}