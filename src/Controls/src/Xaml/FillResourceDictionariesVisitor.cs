using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	[RequiresUnreferencedCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#if !NETSTANDARD
	[RequiresDynamicCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#endif
	class FillResourceDictionariesVisitor : IXamlNodeVisitor
	{
		public FillResourceDictionariesVisitor(HydrationContext context) => Context = context;

		HydrationContext Context { get; }
		Dictionary<INode, object> Values => Context.Values;

		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;

		public bool IsResourceDictionary(ElementNode node) => typeof(ResourceDictionary).IsAssignableFrom(Context.Types[node]);

		public void Visit(ValueNode node, INode parentNode)
		{
			if (!Context.Types.TryGetValue((ElementNode)parentNode, out var type) || !typeof(ResourceDictionary).IsAssignableFrom(type))
				return;

			node.Accept(new ApplyPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			if (!Values.TryGetValue(node, out var value) && Context.ExceptionHandler != null)
				return;

			//Set RD to VE
			if (typeof(ResourceDictionary).IsAssignableFrom(Context.Types[node]) && node.TryGetPropertyName(parentNode, out XmlName propertyName))
			{
				if ((propertyName.LocalName == "Resources" ||
					 propertyName.LocalName.EndsWith(".Resources", StringComparison.Ordinal)) && value is ResourceDictionary)
				{
					var source = Values[parentNode];
					ApplyPropertiesVisitor.SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
					return;
				}
			}

			//Only proceed further if the node is a keyless RD
			if (parentNode is ElementNode
				&& Context.Types.TryGetValue((ElementNode)parentNode, out var parentType)
				&& typeof(ResourceDictionary).IsAssignableFrom(parentType)
				&& !((ElementNode)parentNode).Properties.ContainsKey(XmlName.xKey))
				node.Accept(new ApplyPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
			else if (parentNode is ListNode
					 && typeof(ResourceDictionary).IsAssignableFrom(Context.Types[((ElementNode)parentNode.Parent)])
					 && !((ElementNode)parentNode.Parent).Properties.ContainsKey(XmlName.xKey))
				node.Accept(new ApplyPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
		}

		public void Visit(RootNode node, INode parentNode)
		{
		}

		public void Visit(ListNode node, INode parentNode)
		{
		}

		public bool SkipChildren(INode node, INode parentNode)
		{
			if (!(node is ElementNode))
				return false;
			if (parentNode is ElementNode parentElementNode
				&& Context.Types.TryGetValue(parentElementNode, out var parentType)
				&& typeof(ResourceDictionary).IsAssignableFrom(parentType)
				&& !((ElementNode)parentNode).Properties.ContainsKey(XmlName.xKey))
				return true;
			if (parentNode is ListNode
				&& typeof(ResourceDictionary).IsAssignableFrom(Context.Types[((ElementNode)parentNode.Parent)])
				&& !((ElementNode)parentNode.Parent).Properties.ContainsKey(XmlName.xKey))
				return true;
			return false;
		}

	}
}