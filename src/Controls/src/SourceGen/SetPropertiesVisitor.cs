using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;

using Microsoft.Maui.Controls.Xaml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.CodeDom.Compiler;

using static Microsoft.Maui.Controls.SourceGen.TypeHelpers;

namespace Microsoft.Maui.Controls.SourceGen;

class SetPropertiesVisitor(SourceGenContext context, bool stopOnResourceDictionary = false) : IXamlNodeVisitor
{
    SourceGenContext Context => context;
    IndentedTextWriter Writer => Context.Writer;

    public static readonly IList<XmlName> skips = [
        XmlName.xArguments,
        XmlName.xClass,
        XmlName.xDataType,
        XmlName.xFactoryMethod,
        XmlName.xFieldModifier,
        XmlName.xKey,
        XmlName.xName,
        XmlName.xTypeArguments,
    ];
    public bool StopOnResourceDictionary => stopOnResourceDictionary;
    public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
    public bool StopOnDataTemplate => true;
    public bool VisitNodeOnDataTemplate => true;
    public bool SkipChildren(INode node, INode parentNode) => false;

	public bool IsResourceDictionary(ElementNode node) => false;
		// {
		// 	var parentVar = Context.Variables[(IElementNode)node];
		// 	return parentVar.VariableType.FullName == "Microsoft.Maui.Controls.ResourceDictionary"
		// 		|| parentVar.VariableType.Resolve().BaseType?.FullName == "Microsoft.Maui.Controls.ResourceDictionary";
		// }

    public void Visit(ValueNode node, INode parentNode)
    {
        //TODO support Label text as element

        // if it's implicit content property, get the content property name
        if (!parentNode.TryGetPropertyName(node, out XmlName propertyName))
        {
            if (!parentNode.IsCollectionItem(node))
                return;
            string? contentProperty;
            if (!Context.Variables.ContainsKey((IElementNode)parentNode))
                return;
            var parentVar = Context.Variables[(IElementNode)parentNode];
            if ((contentProperty = parentVar.Type.GetContentPropertyName()) != null)
                propertyName = new XmlName(((IElementNode)parentNode).NamespaceURI, contentProperty);
            else
                return;
        }

        //TODO set runtimename
        // 	if (TrySetRuntimeName(propertyName, Context.Variables[(IElementNode)parentNode], node))
        // return;
        if (skips.Contains(propertyName))
            return;
        if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains(propertyName))
            return;
        if (propertyName.Equals(XamlParser.McUri, "Ignorable"))
            return;
        SetPropertyValue(Writer, Context.Variables[(IElementNode)parentNode], propertyName, node, Context, node);
    }

    public void Visit(MarkupNode node, INode parentNode)
    {
    }

    public void Visit(ElementNode node, INode parentNode)
    {
        XmlName propertyName = XmlName.Empty;

        //Simplify ListNodes with single elements
        //TODO: this should be done with a transform
        var pList = parentNode as ListNode;
        if (pList != null && pList.CollectionItems.Count == 1)
        {
            propertyName = pList.XmlName;
            parentNode = parentNode.Parent;
        }

        if ((propertyName != XmlName.Empty || parentNode.TryGetPropertyName(node, out propertyName)) && skips.Contains(propertyName))
            return;

        // if (propertyName == XmlName._CreateContent)
        // {
        //     SetDataTemplate((IElementNode)parentNode, node, Context, node);
        //     return;
        // }

        // TODO ProvideValue on markupextensions

        if (propertyName != XmlName.Empty)
        {
            if (skips.Contains(propertyName))
                return;
            if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains(propertyName))
                return;
            SetPropertyValue(Writer, Context.Variables[(IElementNode)parentNode], propertyName, node, Context, node);
        }
        //single element in a collection
        else if (parentNode.IsCollectionItem(node) && parentNode is IElementNode)
        {

            var parentVar = Context.Variables[(IElementNode)parentNode];
            string? contentProperty;

            // if (CanAddToRD)
            // RD.Add()
            // }
            // else 
        
            if ((contentProperty = parentVar.Type.GetContentPropertyName()) != null)
            {
                var name = new XmlName(node.NamespaceURI, contentProperty);
                if (skips.Contains(name))
                    return;
                if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains(propertyName))
                    return;
                SetPropertyValue(Writer, parentVar, name, node, Context, node);
            }
            else if (parentVar.Type.CanAdd())
            {
                Writer.WriteLine($"// CanAdd {node} parent {parentNode}");
            }
            
            //     Context.IL.Append(SetPropertyValue(Context.Variables[(IElementNode)parentNode], name, node, Context, node));
            // }
            // // Collection element, implicit content, or implicit collection element.
            // else if (parentVar.VariableType.ImplementsInterface(Context.Cache, Module.ImportReference(Context.Cache, ("mscorlib", "System.Collections", "IEnumerable")))
            //             && parentVar.VariableType.GetMethods(Context.Cache, md => md.Name == "Add" && md.Parameters.Count == 1, Module).Any())
            // {
            //     var elementType = parentVar.VariableType;
            //     var adderTuple = elementType.GetMethods(Context.Cache, md => md.Name == "Add" && md.Parameters.Count == 1, Module).First();
            //     var adderRef = Module.ImportReference(adderTuple.Item1);
            //     adderRef = Module.ImportReference(adderRef.ResolveGenericParameters(adderTuple.Item2, Module));

            //     Context.IL.Emit(Ldloc, parentVar);
            //     Context.IL.Append(vardef.LoadAs(Context.Cache, adderRef.Parameters[0].ParameterType.ResolveGenericParameters(adderRef), Module));
            //     Context.IL.Emit(Callvirt, adderRef);
            //     if (adderRef.ReturnType.FullName != "System.Void")
            //         Context.IL.Emit(Pop);
            // }

            // else
            //     throw new BuildException(BuildExceptionCode.ContentPropertyAttributeMissing, node, null, ((IElementNode)parentNode).XmlType.Name);
        }
        else if (parentNode.IsCollectionItem(node) && parentNode is ListNode)
        {
            Writer.WriteLine($"// Collection item {node} parent {parentNode}");
        }
    }

    public void Visit(RootNode node, INode parentNode)
    {
    }

    public void Visit(ListNode node, INode parentNode)
    {
        Writer.WriteLine($"// ListNode {node} parent {parentNode}");
    }

    static void SetPropertyValue(IndentedTextWriter writer, LocalVariable parentVar, XmlName propertyName, INode valueNode, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        //TODO I believe ContentProperty should be resolved here
        var localName = propertyName.LocalName;
        var bpFieldSymbol = GetBindableProperty(parentVar.Type, propertyName.NamespaceURI, ref localName, out System.Boolean attached, context, iXmlLineInfo);

        //TODO event

        //TODO dynamicresource

        //TODO binding

        //If it's a BP, SetValue
        if (CanSetValue(bpFieldSymbol, valueNode, context)) {
            SetValue(writer, parentVar, bpFieldSymbol!, valueNode, context, iXmlLineInfo);
            return;
        }

        //TODO Set

        if (CanAdd(parentVar, propertyName, valueNode, context)) {
            Add(writer, parentVar, propertyName, valueNode, context, iXmlLineInfo);
            return;
        }
        writer.WriteLine($"// SetPropertyValue UNHANDLED {parentVar} {propertyName} {valueNode}");
    }

    static IFieldSymbol? GetBindableProperty(ITypeSymbol type, string ns, ref string localName, out System.Boolean attached, SourceGenContext context, IXmlLineInfo? iXmlLineInfo)
    {
        var bpParentType = type;
        //if the property assignment is attahced one, like Grid.Row, update the localname and the bpParentType
        attached = GetNameAndTypeRef(ref bpParentType, ns, ref localName, context, iXmlLineInfo);
        var name = $"{localName}Property";
        return bpParentType.GetAllMembers().FirstOrDefault(f => f.Name == name) as IFieldSymbol;        
    }

    static bool GetNameAndTypeRef(ref ITypeSymbol elementType, string namespaceURI, ref string localname,
        SourceGenContext context, IXmlLineInfo? lineInfo)
    {
        var dotIdx = localname.IndexOf('.');
        if (dotIdx > 0)
        {
            var typename = localname.Substring(0, dotIdx);
            localname = localname.Substring(dotIdx + 1);
            elementType = new XmlType(namespaceURI, typename, null).ResolveTypeSymbol(context)!;
            return true;
        }
        return false;
    }

    static bool CanSetValue(IFieldSymbol? bpFieldSymbol, INode node, SourceGenContext context)
    {
        if (bpFieldSymbol == null)
            return false;
        if (node is ValueNode vn) //TODO check if a conversion exists
            return true;
        if(!(node is ElementNode en))
            return false;
        //TODO additional checks
        return true;
    }

    static void SetValue(IndentedTextWriter writer, LocalVariable parentVar, IFieldSymbol bpFieldSymbol, INode node, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        var valueNode = node as ValueNode;
        var elementNode = node as ElementNode;
        string? valueString = null;

        if (valueNode != null) {
            valueString = valueNode.ConvertTo(bpFieldSymbol, iXmlLineInfo);
            writer.WriteLine($"{parentVar.Name}.SetValue({bpFieldSymbol}, {valueString});");
        } else if (elementNode != null) {
            writer.WriteLine($"{parentVar.Name}.SetValue({bpFieldSymbol}, {context.Variables[node].Name});");
        }
    }

    static bool CanAdd(LocalVariable parentVar, XmlName propertyName, INode valueNode, SourceGenContext context)
    {
        var localName = propertyName.LocalName;
        var bpFieldSymbol = GetBindableProperty(parentVar.Type, propertyName.NamespaceURI, ref localName, out System.Boolean attached, context, valueNode as IXmlLineInfo);
        if (!(valueNode is ElementNode en))
            return false;
        
        // if (!CanGetValue(parentVar, bpFieldSymbol, en, context)
        //     && !CanGet(parentVar, localName, en, context))
        //     return false;

        if (!context.Variables.TryGetValue(en, out var childVar))
            return false;
        
        //FIXME use the propertytype from CanGetValue or CanGet
        var prop = parentVar.Type.GetAllProperties().FirstOrDefault(p => p.Name == propertyName.LocalName);
        var propertyType = prop.Type;
                
        return propertyType.CanAdd();
    }

    static void Add(IndentedTextWriter writer, LocalVariable parentVar, XmlName propertyName, INode valueNode, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        //FIXME should handle BP
        var localName = propertyName.LocalName;
        var bpFieldSymbol = GetBindableProperty(parentVar.Type, propertyName.NamespaceURI, ref localName, out System.Boolean attached, context, valueNode as IXmlLineInfo);

        
        // if (!CanGetValue(parentVar, bpFieldSymbol, en, context)
        //     && !CanGet(parentVar, localName, en, context))
        //     return false;

        
        //FIXME use the propertytype from CanGetValue or CanGet
        var prop = parentVar.Type.GetAllProperties().FirstOrDefault(p => p.Name == propertyName.LocalName);
        var propertyType = prop.Type;
                

        writer.WriteLine($"{parentVar.Name}.{propertyName.LocalName}.Add({context.Variables[valueNode].Name});");
    }

}