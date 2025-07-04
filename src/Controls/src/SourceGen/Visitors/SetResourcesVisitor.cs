using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.SourceGen;
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
        if (!((IElementNode)parentNode).IsResourceDictionary(Context))
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
        if (IsResourceDictionary(node) && node.TryGetPropertyName(parentNode, out propertyName))
        {
            if (   propertyName.LocalName == "Resources" 
                || propertyName.LocalName.EndsWith(".Resources", StringComparison.Ordinal))
            {
                SetPropertiesVisitor.SetPropertyValue(Writer, Context.Variables[(IElementNode)parentNode], propertyName, node, Context);
                return;
            }
        }

        //Only proceed further if the node is a keyless RD
        if (   parentNode is IElementNode
            && ((IElementNode)parentNode).IsResourceDictionary(Context)
            && !((IElementNode)parentNode).Properties.ContainsKey(XmlName.xKey))
            node.Accept(new SetPropertiesVisitor(Context, stopOnResourceDictionary: false), parentNode);
        else if (parentNode is ListNode
            && ((IElementNode)parentNode.Parent).IsResourceDictionary(Context)
            && !((IElementNode)parentNode.Parent).Properties.ContainsKey(XmlName.xKey))
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
        var enode = node as ElementNode;
        if (enode == null)
            return false;
        if (parentNode is IElementNode
            && ((IElementNode)parentNode).IsResourceDictionary(Context)
            && !((IElementNode)parentNode).Properties.ContainsKey(XmlName.xKey))
            return true;
        if (parentNode is ListNode
            && ((IElementNode)parentNode.Parent).IsResourceDictionary(Context)
            && !((IElementNode)parentNode.Parent).Properties.ContainsKey(XmlName.xKey))
            return true;
        return false;
    }
}