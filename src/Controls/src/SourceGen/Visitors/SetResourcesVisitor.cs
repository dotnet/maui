using System;
using System.CodeDom.Compiler;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

class SetResourcesVisitor(SourceGenContext context) : IXamlNodeVisitor
{
	SourceGenContext Context => context;
	IndentedTextWriter Writer => Context.Writer;

	public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
	public bool StopOnDataTemplate => true;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => false;

	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

	public void Visit(ValueNode node, INode parentNode)
	{
		if (!((ElementNode)parentNode).IsResourceDictionary(Context))
			return;
		// Skip lazy resources - they're handled separately in SetPropertiesVisitor
		if (((ElementNode)parentNode).IsLazyResource(parentNode.Parent, Context))
			return;

		node.Accept(new SetPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
	}

	public void Visit(MarkupNode node, INode parentNode)
	{
	}

	public void Visit(ElementNode node, INode parentNode)
	{
		// Handle lazy resources - emit AddFactory call
		if (node.IsLazyResource(parentNode, Context))
		{
			// Find the ResourceDictionary to add to
			ILocalValue? rdVar = null;
			
			if (parentNode is ElementNode parentElement && Context.Variables.TryGetValue(parentElement, out var pVar))
			{
				var rdType = Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary")!;
				if (pVar.Type.InheritsFrom(rdType, Context))
				{
					rdVar = pVar;
				}
			}
			else if (parentNode is ListNode listNode && listNode.Parent is ElementNode grandParentElement && Context.Variables.TryGetValue(grandParentElement, out var gpVar))
			{
				var rdType = Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary")!;
				if (gpVar.Type.InheritsFrom(rdType, Context))
				{
					rdVar = gpVar;
				}
			}
			
			if (rdVar != null)
			{
				SetPropertyHelpers.AddLazyResourceToResourceDictionary(Writer, rdVar, node, Context);
			}
			return;
		}

		//Set ResourcesDictionaries to their parents
		if (IsResourceDictionary(node) && node.TryGetPropertyName(parentNode, out XmlName propertyName))
		{
			if (propertyName.LocalName == "Resources"
				|| propertyName.LocalName.EndsWith(".Resources", StringComparison.Ordinal))
			{
				SetPropertyHelpers.SetPropertyValue(Writer, Context.Variables[(ElementNode)parentNode], propertyName, node, Context);
				return;
			}
		}

		//Only proceed further if the node is a keyless RD
		if (parentNode is ElementNode node1
			&& node1.IsResourceDictionary(Context)
			&& !node1.Properties.ContainsKey(XmlName.xKey))
			node.Accept(new SetPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
		else if (parentNode is ListNode
			&& ((ElementNode)parentNode.Parent).IsResourceDictionary(Context)
			&& !((ElementNode)parentNode.Parent).Properties.ContainsKey(XmlName.xKey))
			node.Accept(new SetPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
	}

	public void Visit(RootNode node, INode parentNode)
	{
	}

	public void Visit(ListNode node, INode parentNode)
	{
	}

	public bool SkipChildren(INode node, INode parentNode)
	{
		if (node is not ElementNode enode)
			return false;
		// Skip lazy resources - they're handled separately
		if (enode.IsLazyResource(parentNode, Context))
			return true;
		if (parentNode is ElementNode node1
			&& node1.IsResourceDictionary(Context)
			&& !node1.Properties.ContainsKey(XmlName.xKey))
			return true;
		if (parentNode is ListNode
			&& ((ElementNode)parentNode.Parent).IsResourceDictionary(Context)
			&& !((ElementNode)parentNode.Parent).Properties.ContainsKey(XmlName.xKey))
			return true;
		return false;
	}
}
