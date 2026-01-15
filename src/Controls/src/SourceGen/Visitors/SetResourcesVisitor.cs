using System;
using System.CodeDom.Compiler;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

class SetResourcesVisitor : IXamlNodeVisitor
{
	public SetResourcesVisitor(SourceGenContext context, bool stopOnStyle = true)
	{
		Context = context;
		StopOnStyle = stopOnStyle;
		VisitNodeOnStyle = !stopOnStyle;
	}

	SourceGenContext Context { get; }
	IndentedTextWriter Writer => Context.Writer;

	public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
	public bool StopOnDataTemplate => true;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => false;
	public bool StopOnStyle { get; }
	public bool VisitNodeOnStyle { get; }

	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);
	public bool IsStyle(ElementNode node) => node.IsStyle(Context);

	public void Visit(ValueNode node, INode parentNode)
	{
		if (!((ElementNode)parentNode).IsResourceDictionary(Context))
			return;

		node.Accept(new SetPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
	}

	public void Visit(MarkupNode node, INode parentNode)
	{
	}

	public void Visit(ElementNode node, INode parentNode)
	{
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
