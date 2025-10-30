using System;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	class SetResourcesVisitor(ILContext context) : IXamlNodeVisitor
	{
		public ILContext Context { get; } = context;
		ModuleDefinition Module { get; } = context.Body.Method.Module;
		public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary => false;
		public bool VisitNodeOnDataTemplate => false;

		public void Visit(ValueNode node, INode parentNode)
		{
			if (!IsResourceDictionary((ElementNode)parentNode))
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
			if (IsResourceDictionary(node) && SetPropertiesVisitor.TryGetPropertyName(node, parentNode, out propertyName))
			{
				if ((propertyName.LocalName == "Resources" || propertyName.LocalName.EndsWith(".Resources", StringComparison.Ordinal)))
				{
					Context.IL.Append(SetPropertiesVisitor.SetPropertyValue(Context.Variables[(ElementNode)parentNode], propertyName, node, Context, node));
					return;
				}
			}

			//Only proceed further if the node is a keyless RD
			if (parentNode is ElementNode node1
				&& IsResourceDictionary(node1)
				&& !node1.Properties.ContainsKey(XmlName.xKey))
				node.Accept(new SetPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
			else if (parentNode is ListNode
					 && IsResourceDictionary((ElementNode)parentNode.Parent)
					 && !((ElementNode)parentNode.Parent).Properties.ContainsKey(XmlName.xKey))
				node.Accept(new SetPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
		}

		public void Visit(RootNode node, INode parentNode)
		{
		}

		public void Visit(ListNode node, INode parentNode)
		{
		}

		public bool IsResourceDictionary(ElementNode node)
		{
			var parentVar = Context.Variables[node];
			return parentVar.VariableType.FullName == "Microsoft.Maui.Controls.ResourceDictionary"
				|| parentVar.VariableType.ResolveCached(Context.Cache).BaseType?.FullName == "Microsoft.Maui.Controls.ResourceDictionary";
		}

		public bool SkipChildren(INode node, INode parentNode)
		{
			if (node is not ElementNode enode)
				return false;
			if (parentNode is ElementNode node1
				&& IsResourceDictionary(node1)
				&& !node1.Properties.ContainsKey(XmlName.xKey))
				return true;
			if (parentNode is ListNode
				&& IsResourceDictionary((ElementNode)parentNode.Parent)
				&& !((ElementNode)parentNode.Parent).Properties.ContainsKey(XmlName.xKey))
				return true;
			return false;
		}
	}
}