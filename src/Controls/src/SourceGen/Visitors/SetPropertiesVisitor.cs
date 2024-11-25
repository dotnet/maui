using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;

using Microsoft.Maui.Controls.Xaml;

using Microsoft.CodeAnalysis;

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

    public void Visit(ValueNode node, INode parentNode)
    {
        //TODO support Label text as element

        // if it's implicit content property, get the content property name
        if (!node.TryGetPropertyName(parentNode, out XmlName propertyName))
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

        if ((propertyName != XmlName.Empty || node.TryGetPropertyName(parentNode, out propertyName)) && skips.Contains(propertyName))
            return;

        // if (propertyName == XmlName._CreateContent)
        // {
        //     SetDataTemplate((IElementNode)parentNode, node, Context, node);
        //     return;
        // }

        //IMarkupExtension or IValueProvider => ProvideValue()
        if (node.IsValueProvider(context, 
                out ITypeSymbol? returnType,
                out ITypeSymbol? valueProviderFace,
                out bool acceptEmptyServiceProvider,
                out ImmutableArray<ITypeSymbol>? requiredServices))
        {
            var valueProviderVariable = Context.Variables[node];
            var variableName = NamingHelpers.CreateUniqueVariableName(Context, returnType!.Name!.Split('.').Last());
            Context.Variables[node] = new LocalVariable(returnType, variableName);

            //if it require a serviceprovider, create one
            if (!acceptEmptyServiceProvider)
            {
                var serviceProviderVar = node.GetOrCreateServiceProvider(Writer, context, requiredServices);                
                Writer.WriteLine($"{returnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {variableName} = (({valueProviderFace!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}){valueProviderVariable.Name}).ProvideValue({serviceProviderVar.Name});");
            }
            else
                Writer.WriteLine($"{returnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {variableName} = (({valueProviderFace!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}){valueProviderVariable.Name}).ProvideValue(null);");
        }

        if (propertyName != XmlName.Empty)
        {
            if (skips.Contains(propertyName))
                return;
            if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains(propertyName))
                return;
            SetPropertyValue(Writer, Context.Variables[(IElementNode)parentNode], propertyName, node, Context, node);
        }
        else if (parentNode.IsCollectionItem(node) && parentNode is IElementNode)
        {
            var parentVar = Context.Variables[(IElementNode)parentNode];
            string? contentProperty;

            if (CanAddToResourceDictionary(parentVar, parentVar.Type, node, node as IXmlLineInfo, Context))
            {
                AddToResourceDictionary(Writer, parentVar, node, node as IXmlLineInfo, Context);
            }
            else if ((contentProperty = parentVar.Type.GetContentPropertyName()) != null)
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

    internal static void SetPropertyValue(IndentedTextWriter writer, LocalVariable parentVar, XmlName propertyName, INode valueNode, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        //FIXME Special case or RD.Source. should go away as as soon as we sourcegen the RDSourceTypeConverter
        if (propertyName.Equals("", "Source") && parentVar.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary")!))
        {
            // LoadFromSource(ResourceDictionary rd, Uri source, string resourcePath, Assembly assembly, IXmlLineInfo lineInfo)
            writer.WriteLine($"global::Microsoft.Maui.Controls.Xaml.ResourceDictionaryHelpers.LoadFromSource({parentVar.Name}, new global::System.Uri(\"{((ValueNode)valueNode).Value}\", global::System.UriKind.RelativeOrAbsolute), \"{((ValueNode)valueNode).Value}\", typeof({context.RootType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}).Assembly, null);");
            return;
        }

        //TODO I believe ContentProperty should be resolved here
        var localName = propertyName.LocalName;
        var bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out bool attached, context, iXmlLineInfo);

        //TODO event

        //TODO dynamicresource

        //If it's a BP and the value is BindingBase, SetBinding
        if (CanSetBinding(bpFieldSymbol, valueNode, context))
        {
            SetBinding(writer, parentVar, bpFieldSymbol!, valueNode, context, iXmlLineInfo);
            return;
        }

        //If it's a BP, SetValue
        if (CanSetValue(bpFieldSymbol, valueNode, context)) {
            SetValue(writer, parentVar, bpFieldSymbol!, valueNode, context, iXmlLineInfo);
            return;
        }

        //POCO, set the property
        if (CanSet(parentVar, localName, valueNode, context)) {
            Set(writer, parentVar, localName, valueNode, context, iXmlLineInfo);
            return;
        }

        if (CanAdd(parentVar, propertyName, valueNode, context)) {
            Add(writer, parentVar, propertyName, valueNode, context, iXmlLineInfo);
            return;
        }
        writer.WriteLine($"// SetPropertyValue UNHANDLED {parentVar} {propertyName} {valueNode}");
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
        if (node is ValueNode valueNode)
        {
            var valueString = valueNode.ConvertTo(bpFieldSymbol, context, iXmlLineInfo);
            writer.WriteLine($"{parentVar.Name}.SetValue({bpFieldSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType))}, {valueString});");
        }
        else if (node is ElementNode elementNode)
            writer.WriteLine($"{parentVar.Name}.SetValue({bpFieldSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType))}, {context.Variables[node].Name});");
	}

    static bool CanSet(LocalVariable parentVar, string localName, INode node, SourceGenContext context)
    {
        if (parentVar.Type.GetAllProperties().FirstOrDefault(p => p.Name == localName) is not IPropertySymbol property)
            return false;
        if (property.SetMethod is not IMethodSymbol propertySetter  || !propertySetter.IsPublic() || propertySetter.IsStatic)
            return false;
        if (node is ValueNode vn) //TODO check if a conversion exists
            return true;
        if (node is not IElementNode elementNode)
            return false;
        var localVar = context.Variables[elementNode];
        //TODO check if the property type is compatible with the element type
        return true;
    }

    static void Set(IndentedTextWriter writer, LocalVariable parentVar, string localName, INode node, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        var property = parentVar.Type.GetAllProperties().First(p => p.Name == localName);
        
        if (node is ValueNode valueNode)
        {
            var valueString = valueNode.ConvertTo(property, context, iXmlLineInfo);
            using (PrePost.NewLineInfo(writer, iXmlLineInfo, context.FilePath))
                writer.WriteLine($"{parentVar.Name}.{localName} = {valueString};");
        }
        else if (node is ElementNode elementNode)
            using (PrePost.NewLineInfo(writer, iXmlLineInfo, context.FilePath))
                writer.WriteLine($"{parentVar.Name}.{localName} = {context.Variables[elementNode].Name};");
    }

    static bool CanSetBinding(IFieldSymbol? bpFieldSymbol, INode node, SourceGenContext context)
    {
        if (bpFieldSymbol == null)
            return false;
        if (node is not ElementNode en)
            return false;
        if (!context.Variables.TryGetValue(en, out var localVariable))
            return false;

        //TODO check if there's an implicit operator to/from BindingBase

        if(localVariable.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindingBase")!))
            return true;
        return false;
    }

    static void SetBinding(IndentedTextWriter writer, LocalVariable parentVar, IFieldSymbol bpFieldSymbol, INode node, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        var localVariable = context.Variables[(ElementNode)node];
        writer.WriteLine($"{parentVar.Name}.SetBinding({bpFieldSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithMemberOptions(SymbolDisplayMemberOptions.IncludeContainingType))}, {localVariable.Name});");
    }

    static bool CanAdd(LocalVariable parentVar, XmlName propertyName, INode valueNode, SourceGenContext context)
    {
        var localName = propertyName.LocalName;
        var bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out bool attached, context, valueNode as IXmlLineInfo);
        
        if (valueNode is not ElementNode en)
            return false;
        
        // if (!CanGetValue(parentVar, bpFieldSymbol, en, context)
        //     && !CanGet(parentVar, localName, en, context))
        //     return false;

        if (!context.Variables.TryGetValue(en, out var childVar))
            return false;
        
        //FIXME use the propertytype from CanGetValue or CanGet
        var prop = parentVar.Type.GetAllProperties().FirstOrDefault(p => p.Name == propertyName.LocalName);
        var propertyType = prop.Type;
                
        if(propertyType.CanAdd())
            return true;

        return CanAddToResourceDictionary(parentVar, propertyType, en, valueNode as IXmlLineInfo, context);

    }

	static bool CanAddToResourceDictionary(LocalVariable parentVar, ITypeSymbol collectionType, IElementNode node, IXmlLineInfo? lineInfo, SourceGenContext context)
	{
		if (!collectionType.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary")!))
            return false;
        if (node.Properties.ContainsKey(XmlName.xKey))
            //TODO check for dupe key
            return true;
        //is there an Add() overload that takes the type of the element ?
        var nodeType = context.Variables[node].Type;
        if (collectionType.GetAllMethods("Add").FirstOrDefault(m => m.Parameters.Length == 1 && m.Parameters[0].Type.Equals(nodeType, SymbolEqualityComparer.Default)) != null)
            return true;
        //TODO FAIL ?
        return false;     
	}

	static void Add(IndentedTextWriter writer, LocalVariable parentVar, XmlName propertyName, INode valueNode, SourceGenContext context, IXmlLineInfo iXmlLineInfo)
    {
        //FIXME should handle BP
        var localName = propertyName.LocalName;
        var bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out System.Boolean attached, context, valueNode as IXmlLineInfo);

        
        // if (!CanGetValue(parentVar, bpFieldSymbol, en, context)
        //     && !CanGet(parentVar, localName, en, context))
        //     return false;

        
        //FIXME use the propertytype from CanGetValue or CanGet
        var prop = parentVar.Type.GetAllProperties().FirstOrDefault(p => p.Name == propertyName.LocalName);
        var propertyType = prop.Type;
                

        writer.WriteLine($"{parentVar.Name}.{propertyName.LocalName}.Add({context.Variables[valueNode].Name});");
    }

    static void AddToResourceDictionary(IndentedTextWriter writer, LocalVariable parentVar, IElementNode node, IXmlLineInfo? lineInfo, SourceGenContext context)
    {
        if (node.Properties.TryGetValue(XmlName.xKey, out var keyNode))
        {
            var key = ((ValueNode)keyNode).Value as string;
            writer.WriteLine($"{parentVar.Name}.Add(\"{key}\", {context.Variables[node].Name});");
            return;
        }
        writer.WriteLine($"{parentVar.Name}.Add({context.Variables[node].Name});");
    }
}