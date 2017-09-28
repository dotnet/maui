using System;
using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

namespace Xamarin.Forms.Xaml
{
	class FillResourceDictionariesVisitor : IXamlNodeVisitor
	{
		public FillResourceDictionariesVisitor(HydrationContext context)
		{
			Context = context;
		}

		HydrationContext Context { get; }
		Dictionary<INode, object> Values => Context.Values;

		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;

		public void Visit(ValueNode node, INode parentNode)
		{
			if (!typeof(ResourceDictionary).IsAssignableFrom(Context.Types[((IElementNode)parentNode)]))
				return;

			node.Accept(new ApplyPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			var value = Values[node];
			XmlName propertyName;
			//Set RD to VE
			if (typeof(ResourceDictionary).IsAssignableFrom(Context.Types[node]) && ApplyPropertiesVisitor.TryGetPropertyName(node, parentNode, out propertyName)) {
				if ((propertyName.LocalName == "Resources" ||
					 propertyName.LocalName.EndsWith(".Resources", StringComparison.Ordinal)) && value is ResourceDictionary) {
					var source = Values[parentNode];
					ApplyPropertiesVisitor.SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
					return;
				}
			}

			//Only proceed further if the node is a RD
			if (parentNode is IElementNode && typeof(ResourceDictionary).IsAssignableFrom(Context.Types[((IElementNode)parentNode)]))
				node.Accept(new ApplyPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
			else if (parentNode is ListNode && typeof(ResourceDictionary).IsAssignableFrom(Context.Types[((IElementNode)parentNode.Parent)]))
				node.Accept(new ApplyPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
		}

		public void Visit(RootNode node, INode parentNode)
		{
		}

		public void Visit(ListNode node, INode parentNode)
		{
		}
	}
}