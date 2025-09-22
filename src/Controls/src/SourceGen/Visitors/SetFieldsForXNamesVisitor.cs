using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;


namespace Microsoft.Maui.Controls.SourceGen;

using static GeneratorHelpers;

class SetFieldsForXNamesVisitor : IXamlNodeVisitor
{
	public SetFieldsForXNamesVisitor(SourceGenContext context) => Context = context;

	SourceGenContext Context { get; }
	IndentedTextWriter Writer => Context.Writer;
	public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
	public bool StopOnDataTemplate => true;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => false;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

	public void Visit(ValueNode node, INode parentNode)
	{
		if (!IsXNameProperty(node, parentNode))
			return;

		if (parentNode is not ElementNode parentElement)
			return;

		if (IsVisualStateGroup(parentElement))
			return;

		if (IsVisualState(parentElement))
			return;


		Writer.WriteLine($"this.{EscapeIdentifier((string)(node.Value))} = {Context.Variables[(ElementNode)parentNode].Name};");
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
		=> parentNode is ElementNode parentElement && parentElement.Properties.TryGetValue(XmlName.xName, out INode xNameNode) && xNameNode == node;

	static bool IsVisualStateGroup(ElementNode node) => node?.XmlType.Name == "VisualStateGroup" && node?.Parent is IListNode;
	static bool IsVisualState(ElementNode node) => node?.XmlType.Name == "VisualState" && node?.Parent is IListNode;
}