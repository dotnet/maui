using System;
using System.Collections;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	class SetResourcesVisitor : IXamlNodeVisitor
	{
		public SetResourcesVisitor(ILContext context)
		{
			Context = context;
			Module = context.Body.Method.Module;
		}

		public ILContext Context { get; }
		ModuleDefinition Module { get; }
		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;

		public void Visit(ValueNode node, INode parentNode)
		{
			if (!IsResourceDictionary((IElementNode)parentNode))
				return;

			node.Accept(new SetPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}


		public void Visit(ElementNode node, INode parentNode)
		{
			XmlName propertyName;
			//Set ResourcesDictionaries to their parents
			if (IsResourceDictionary(node) && SetPropertiesVisitor.TryGetPropertyName(node, parentNode, out propertyName)) {
				if ((propertyName.LocalName == "Resources" || propertyName.LocalName.EndsWith(".Resources", StringComparison.Ordinal))) {
					Context.IL.Append(SetPropertiesVisitor.SetPropertyValue(Context.Variables[(IElementNode)parentNode], propertyName, node, Context, node));
					return;
				}
			}

			//Only proceed further if the node is a keyless RD
			if (   parentNode is IElementNode
				&& IsResourceDictionary((IElementNode)parentNode)
				&& !((IElementNode)parentNode).Properties.ContainsKey(XmlName.xKey))
				node.Accept(new SetPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
			else if (   parentNode is ListNode
					 && IsResourceDictionary((IElementNode)parentNode.Parent)
					 && !((IElementNode)parentNode.Parent).Properties.ContainsKey(XmlName.xKey))
				node.Accept(new SetPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
		}

		public void Visit(RootNode node, INode parentNode)
		{
		}

		public void Visit(ListNode node, INode parentNode)
		{
		}

		bool IsResourceDictionary(IElementNode node)
		{
			var parentVar = Context.Variables[(IElementNode)node];
			return parentVar.VariableType.FullName == "Xamarin.Forms.ResourceDictionary"
				|| parentVar.VariableType.Resolve().BaseType?.FullName == "Xamarin.Forms.ResourceDictionary";
		}

		public bool SkipChildren(INode node, INode parentNode)
		{
			var enode = node as ElementNode;
			if (enode == null)
				return false;
			if (   parentNode is IElementNode
			    && IsResourceDictionary((IElementNode)parentNode)
			    && !((IElementNode)parentNode).Properties.ContainsKey(XmlName.xKey))
				return true;
			if (   parentNode is ListNode
			    && IsResourceDictionary((IElementNode)parentNode.Parent)
			    && !((IElementNode)parentNode.Parent).Properties.ContainsKey(XmlName.xKey))
				return true;
			return false;
		}
	}
}