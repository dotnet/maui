using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;


namespace Microsoft.Maui.Controls.SourceGen;

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
    //TODO FIXME
    public bool IsResourceDictionary(ElementNode node) => false;//typeof(ResourceDictionary).IsAssignableFrom(Context.Types[node]);

    public void Visit(ValueNode node, INode parentNode)
    {
        if (!IsXNameProperty(node, parentNode))
            return;

        Writer.WriteLine($"this.{CodeBehindGenerator.EscapeIdentifier((string)(node.Value))} = {Context.Variables[(IElementNode)parentNode].Name};");
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