using Microsoft.Maui.Controls.Xaml;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.SourceGen;

class ExpandMarkupsVisitor : IXamlNodeVisitor
{
	public ExpandMarkupsVisitor(SourceGenContext context) => Context = context;

	public static readonly IList<XmlName> Skips = new List<XmlName>
	{
		XmlName.xKey,
		XmlName.xTypeArguments,
		XmlName.xFactoryMethod,
		XmlName.xName,
		XmlName.xDataType
	};

	SourceGenContext Context { get; }
	public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
	public bool StopOnDataTemplate => false;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => true;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => false;

	public void Visit(ValueNode node, INode parentNode)
	{
	}

	public void Visit(MarkupNode markupnode, INode parentNode)
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
}