using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;

using Microsoft.Maui.Controls.Xaml;

using Microsoft.CodeAnalysis;

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.Controls.SourceGen;

using static LocationHelpers;
using static GeneratorHelpers;

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
            if ((contentProperty = parentVar.Type.GetContentPropertyName(context)) != null)
                propertyName = new XmlName(((IElementNode)parentNode).NamespaceURI, contentProperty);
            else
                return;
        }

        if (TrySetRuntimeName(propertyName, Context.Variables[(IElementNode)parentNode], node))
            return;
        if (skips.Contains(propertyName))
            return;
        if (parentNode is IElementNode node1 && node1.SkipProperties.Contains(propertyName))
            return;
        if (propertyName.Equals(XamlParser.McUri, "Ignorable"))
            return;
        SetPropertyValue(Writer, Context.Variables[(IElementNode)parentNode], propertyName, node, Context);
    }

	bool TrySetRuntimeName(XmlName propertyName, LocalVariable localVariable, ValueNode node)
	{
        if (propertyName != XmlName.xName)
            return false;
        var runtimeNameAttr = localVariable.Type.GetAllAttributes(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.RuntimeNamePropertyAttribute")!, Context).FirstOrDefault();
        if (runtimeNameAttr == null)
            return false;
        var name = runtimeNameAttr.ConstructorArguments[0].Value as string;
        if (string.IsNullOrEmpty(name))
            return false;
        Writer.WriteLine($"{localVariable.Name}.{name} = \"{node.Value}\";");
        return true;
	}

	public void Visit(MarkupNode node, INode parentNode)
    {
    }

    public void Visit(ElementNode node, INode parentNode)
    {
        XmlName propertyName = XmlName.Empty;

		//Simplify ListNodes with single elements
		//TODO: this should be done with a transform
		if (parentNode is ListNode pList && pList.CollectionItems.Count == 1)
		{
			propertyName = pList.XmlName;
			parentNode = parentNode.Parent;
		}

		if ((propertyName != XmlName.Empty || node.TryGetPropertyName(parentNode, out propertyName)) 
        && skips.Contains(propertyName) || parentNode is IElementNode epn && epn.SkipProperties.Contains(propertyName))
            return;

        if (propertyName == XmlName._CreateContent)
        {
            var variable = Context.Variables[parentNode];
            Writer.WriteLine($"{variable.Name}.LoadTemplate = () =>");
            using (PrePost.NewBlock(Writer, begin: "{", end: "};"))
            {
                var templateContext = new SourceGenContext(Writer, context.Compilation, context.SourceProductionContext, context.XmlnsCache, context.TypeCache, context.RootType!, null) {FilePath = context.FilePath, ParentContext = context};

                //inflate the template
			    node.Accept(new CreateValuesVisitor(templateContext), null);
			    node.Accept(new SetNamescopesAndRegisterNamesVisitor(templateContext), null);
			    // node.Accept(new SetFieldVisitor(templateContext), null);
			    node.Accept(new SetResourcesVisitor(templateContext), null);
			    node.Accept(new SetPropertiesVisitor(templateContext, stopOnResourceDictionary: true), null);
                Writer.WriteLine($"return {templateContext.Variables[node].Name};");
            }
            return;
        }

        //IMarkupExtension or IValueProvider => ProvideValue()
        node.TryProvideValue(context);

        if (propertyName != XmlName.Empty)
        {
            // if (skips.Contains(propertyName))
            //     return;
            // if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains(propertyName))
            //     return;
            SetPropertyValue(Writer, Context.Variables[(IElementNode)parentNode], propertyName, node, Context);
        }
        else if (parentNode.IsCollectionItem(node) && parentNode is IElementNode)
        {
            var parentVar = Context.Variables[(IElementNode)parentNode];
            string? contentProperty;

            if (CanAddToResourceDictionary(parentVar, parentVar.Type, node, Context))
                AddToResourceDictionary(Writer, parentVar, node, Context);
            else if ((contentProperty = parentVar.Type.GetContentPropertyName(context)) != null)
            {
                var name = new XmlName(node.NamespaceURI, contentProperty);
                if (skips.Contains(name))
                    return;
                if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains(propertyName))
                    return;
                SetPropertyValue(Writer, parentVar, name, node, Context);
            }
            else if (parentVar.Type.CanAdd(context))
            {
                Writer.WriteLine($"{parentVar.Name}.Add({Context.Variables[node].Name});");
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
        else if (parentNode.IsCollectionItem(node) && parentNode is ListNode parentList)
        {
            if (skips.Contains(parentList.XmlName))
                return;

            var parentVar = Context.Variables[(IElementNode)parentNode.Parent];
            if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains(propertyName))
					return;
            var elementType = parentVar.Type;
            var localName = parentList.XmlName.LocalName;
            var bpFieldSymbol = parentVar.Type.GetBindableProperty(parentList.XmlName.NamespaceURI, ref localName, out System.Boolean attached, context, node as IXmlLineInfo);        
            var propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault();
            var typeandconverter = bpFieldSymbol?.GetBPTypeAndConverter(context);

            var propertyType =  typeandconverter?.type ?? propertySymbol?.Type;
            if (propertyType == null)
                return;

            //TODO if the var is already created and in context.Variables, use that
            //TODO if we don't need the var, don't create it (this will likely be optimized by the compiler anyway, but...)
            var variable = new LocalVariable(propertyType, NamingHelpers.CreateUniqueVariableName(Context, propertyType));
            Writer.WriteLine($"var {variable.Name} = {GetOrGetValue(parentVar, bpFieldSymbol, propertySymbol, node, Context)};");

            if (CanAddToResourceDictionary(variable, propertyType, node, Context))
            {
                AddToResourceDictionary(Writer, variable, node, Context);
                return;
            }
            Writer.WriteLine($"{variable.Name}.Add({Context.Variables[node].Name});");
        }
    }

    public void Visit(RootNode node, INode parentNode)
    {
    }

    public void Visit(ListNode node, INode parentNode)
    {
    }

    internal static void SetPropertyValue(IndentedTextWriter writer, LocalVariable parentVar, XmlName propertyName, INode valueNode, SourceGenContext context)
    {
        //TODO I believe ContentProperty should be resolved here
        var localName = propertyName.LocalName;
        var bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out bool attached, context, (IXmlLineInfo)valueNode);
        if (bpFieldSymbol != null && !context.Compilation.IsSymbolAccessibleWithin(bpFieldSymbol, context.RootType))
        {
            //not a diagnostic, as it might have a visible symbol matching for CanSet()
            bpFieldSymbol = null;
        }

        // event
        if (CanConnectEvent(parentVar, localName, valueNode, attached, context))
        {
            ConnectEvent(writer, parentVar, localName, valueNode, context);
            return;
        }

        //DynamicResource
        if (CanSetDynamicResource(bpFieldSymbol, valueNode, context))
        {
            SetDynamicResource(writer, parentVar, bpFieldSymbol!, valueNode, context);
            return;
        }

        //If it's a BP and the value is BindingBase, SetBinding
        if (CanSetBinding(bpFieldSymbol, valueNode, context))
        {
            SetBinding(writer, parentVar, bpFieldSymbol!, valueNode, context);
            return;
        }

        //If it's a BP, SetValue
        if (CanSetValue(bpFieldSymbol, valueNode, context)) {
            SetValue(writer, parentVar, bpFieldSymbol!, valueNode, context);
            return;
        }

        //POCO, set the property
        if (CanSet(parentVar, localName, valueNode, context)) {
            Set(writer, parentVar, localName, valueNode, context);
            return;
        }

        if (CanAdd(parentVar, localName, bpFieldSymbol, attached, valueNode, context)) {
            Add(writer, parentVar, propertyName, valueNode, context);
            return;
        }
		
        var location = LocationCreate(context.FilePath!, (IXmlLineInfo)valueNode, localName);
        context.ReportDiagnostic(Diagnostic.Create(Descriptors.MemberResolution, location, localName));
    }

	private static bool CanSetDynamicResource(IFieldSymbol? bpFieldSymbol, INode node, SourceGenContext context)
	{
        if (bpFieldSymbol == null)
            return false;
        if (node is not IElementNode en)
            return false;

        //TODO we could get the type directly from the XmlType of the node, so no need to instantiate de extension at all
        if (!context.Variables.TryGetValue(en, out var localVar))
            return false;
        
        return localVar.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Internals.DynamicResource")!);
	}

	private static void SetDynamicResource(IndentedTextWriter writer, LocalVariable parentVar, IFieldSymbol fieldSymbol, INode valueNode, SourceGenContext context)
        => writer.WriteLine($"{parentVar.Name}.SetDynamicResource({fieldSymbol.ToFQDisplayString()}, {context.Variables[(IElementNode)valueNode].Name}.Key);");

	static bool CanConnectEvent(LocalVariable parentVar, string localName, INode valueNode, bool attached, SourceGenContext context)
        //FIXME check event signature
        => !attached && valueNode is ValueNode && parentVar.Type.GetAllEvents(localName, context).Any();

	static void ConnectEvent(IndentedTextWriter writer, LocalVariable parentVar, string localName, INode valueNode, SourceGenContext context)
        => writer.WriteLine($"{parentVar.Name}.{localName} += {((ValueNode)valueNode).Value};");

	static bool CanSetValue(IFieldSymbol? bpFieldSymbol, INode node, SourceGenContext context)
    {
        if (bpFieldSymbol == null)
            return false;
        if (node is ValueNode vn && vn.CanConvertTo(bpFieldSymbol, context))
            return true;
        if(node is not ElementNode en)
            return false;
        
        var localVar = context.Variables[en];

        // If it's an attached BP, there's no second chance to handle IMarkupExtensions, so we try here.
        // Worst case scenario ? InvalidCastException at runtime
        if (localVar.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
            return true;

        var bpTypeAndConverter = bpFieldSymbol.GetBPTypeAndConverter(context);
        if (context.Compilation.HasImplicitConversion(localVar.Type, bpTypeAndConverter?.type))
            return true;
        
        if (HasDoubleImplicitConversion(localVar.Type, bpTypeAndConverter?.type, context, out _))
            return true;
        
        return localVar.Type.InheritsFrom(bpTypeAndConverter?.type!) 
            || bpFieldSymbol.Type.IsInterface() && localVar.Type.Implements(bpTypeAndConverter?.type!);
    }

    static void SetValue(IndentedTextWriter writer, LocalVariable parentVar, IFieldSymbol bpFieldSymbol, INode node, SourceGenContext context)
    {
        var pType = bpFieldSymbol.GetBPTypeAndConverter(context)?.type;
        if (node is ValueNode valueNode)
        {
            var valueString = valueNode.ConvertTo(bpFieldSymbol, context, parentVar);
            writer.WriteLine($"{parentVar.Name}.SetValue({bpFieldSymbol.ToFQDisplayString()}, {valueString});");
        }
        else if (node is ElementNode elementNode)
            writer.WriteLine($"{parentVar.Name}.SetValue({bpFieldSymbol.ToFQDisplayString()}, {(HasDoubleImplicitConversion(context.Variables[elementNode].Type, pType, context, out var conv) ? "(" + conv!.ReturnType.ToFQDisplayString() + ")" : string.Empty)}{context.Variables[node].Name});");
	}

    static bool CanGet(LocalVariable parentVar, string localName, SourceGenContext context, out ITypeSymbol? propertyType, out IPropertySymbol? propertySymbol)
    {
        propertyType = null;
		if ((propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault()) == null)
            return false;
        if (propertySymbol!.GetMethod is not IMethodSymbol propertyGetter || !propertyGetter.IsPublic() || propertyGetter.IsStatic)
            return false;

        propertyType = propertySymbol.Type;
        return true;
    }

    static bool CanGetValue(LocalVariable parentVar, IFieldSymbol? bpFieldSymbol, bool attached, SourceGenContext context, out ITypeSymbol? propertyType)
    {
        propertyType = null;
        if (bpFieldSymbol == null)
            return false;

        if (!parentVar.Type.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindableObject")!))
            return false;

        propertyType = bpFieldSymbol.GetBPTypeAndConverter(context)?.type;
        return true;
    }

    static string GetOrGetValue(LocalVariable parentVar, IFieldSymbol? bpFieldSymbol, IPropertySymbol? property, INode node, SourceGenContext context)
    {
        if (bpFieldSymbol != null)
        {
            var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter(context);
            return $"({typeandconverter?.type.ToFQDisplayString()}){parentVar.Name}.GetValue({bpFieldSymbol.ToFQDisplayString()})";
        }
        else if (property != null)
            return $"{parentVar.Name}.{property.Name}";
        else
            return "null";        
    }

    static bool CanSet(LocalVariable parentVar, string localName, INode node, SourceGenContext context)
    {
        if (parentVar.Type.GetAllProperties(localName, context).FirstOrDefault() is not IPropertySymbol property)
            return false;
        if (property.SetMethod is not IMethodSymbol propertySetter  || !propertySetter.IsPublic() || propertySetter.IsStatic)
            return false;
        if (node is ValueNode vn && vn.CanConvertTo(property, context))
            return true;
        if (node is not IElementNode elementNode)
            return false;
        if (!context.Variables.TryGetValue(elementNode, out var localVar))
            return false;
        if (localVar.Type.InheritsFrom(property.Type))
            return true;
        if (property.Type.IsInterface() && localVar.Type.Implements(property.Type))
            return true;

        if (property.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
            return true;

        if (context.Compilation.HasImplicitConversion(localVar.Type, property.Type))
            return true;

        if (HasDoubleImplicitConversion(localVar.Type, property.Type, context, out _))
            return true;

        //TODO could we replace this by a runimt check (generating a if/else) ?            
        if (localVar.Type.Equals(context.Compilation.ObjectType, SymbolEqualityComparer.Default))
            return true;

        return false;
    }

    static bool HasDoubleImplicitConversion(ITypeSymbol? fromType, ITypeSymbol? toType, SourceGenContext context, out IMethodSymbol? op)
    {
        op = null;
        if (fromType == null || toType == null)
            return false;

        //return false, no need to multiple cast here    
        if (context.Compilation.HasImplicitConversion(fromType, toType))
            return false;

        IMethodSymbol[] implicitOps = 
            [
                .. fromType.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Conversion),
                .. toType.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Conversion)
            ];
        
        foreach (var implicitOp in implicitOps)
        {
            if (   context.Compilation.HasImplicitConversion(fromType, implicitOp.Parameters[0].Type)   
                && context.Compilation.HasImplicitConversion(implicitOp.ReturnType, toType))                
            {
                op = implicitOp;
                return true;
            }
        }

        return false;
    }

    static void Set(IndentedTextWriter writer, LocalVariable parentVar, string localName, INode node, SourceGenContext context)
    {
        var property = parentVar.Type.GetAllProperties(localName, context).First();
        
        if (node is ValueNode valueNode)
        {
            using (PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.FilePath)){
                var valueString = valueNode.ConvertTo(property, context, parentVar);
                writer.WriteLine($"{parentVar.Name}.{EscapeIdentifier(localName)} = {valueString};");
            }
        }
        else if (node is ElementNode elementNode)
            using (PrePost.NewLineInfo(writer, (IXmlLineInfo)node, context.FilePath))
                writer.WriteLine($"{parentVar.Name}.{EscapeIdentifier(localName)} = ({property.Type.ToFQDisplayString()}){(HasDoubleImplicitConversion(context.Variables[elementNode].Type, property.Type, context, out var conv) ? "(" + conv!.ReturnType.ToFQDisplayString()+ ")" : string.Empty)}{context.Variables[elementNode].Name};");
    }

    static bool CanSetBinding(IFieldSymbol? bpFieldSymbol, INode node, SourceGenContext context)
    {
        
        if (bpFieldSymbol == null)
            return false;
        if (node is not ElementNode en)
            return false;
        if (!context.Variables.TryGetValue(en, out var localVariable))
            return false;

        var bindingBaseSymbol = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.BindingBase")!;

        if(localVariable.Type.InheritsFrom(bindingBaseSymbol))
            return true;

        if (context.Compilation.HasImplicitConversion(localVariable.Type, bindingBaseSymbol))
            return true;    
        
        return false;
    }

    static void SetBinding(IndentedTextWriter writer, LocalVariable parentVar, IFieldSymbol bpFieldSymbol, INode node, SourceGenContext context)
    {
        var localVariable = context.Variables[(ElementNode)node];
        writer.WriteLine($"{parentVar.Name}.SetBinding({bpFieldSymbol.ToFQDisplayString()}, {localVariable.Name});");
    }

    static bool CanAdd(LocalVariable parentVar, string localName, IFieldSymbol? bpFieldSymbol, bool attached, INode valueNode, SourceGenContext context)
    {
        if (valueNode is not ElementNode en)
            return false;
        
        if (!CanGetValue(parentVar, bpFieldSymbol, attached, context, out ITypeSymbol? propertyType)
            && !CanGet(parentVar, localName, context, out propertyType, out _))
				return false;

        if (!context.Variables.TryGetValue(en, out var childVar))
            return false;

        if (CanAddToResourceDictionary(parentVar, propertyType!, en, context))
            return true;

        //FIXME should be better and test for value type
        if(propertyType!.CanAdd(context))
            return true;

        return false;

    }

	static bool CanAddToResourceDictionary(LocalVariable parentVar, ITypeSymbol collectionType, IElementNode node, SourceGenContext context)
	{
		if (!collectionType.InheritsFrom(context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ResourceDictionary")!))
            return false;
        if (node.Properties.ContainsKey(XmlName.xKey))
            //TODO check for dupe key
            return true;
        //is there an Add() overload that takes the type of the element ?
        var nodeType = context.Variables[node].Type;
        if (collectionType.GetAllMethods("Add", context).FirstOrDefault(m => m.Parameters.Length == 1 && m.Parameters[0].Type.Equals(nodeType, SymbolEqualityComparer.Default)) != null)
            return true;
        return false;     
	}

	static void Add(IndentedTextWriter writer, LocalVariable parentVar, XmlName propertyName, INode valueNode, SourceGenContext context)
    {
        var localName = propertyName.LocalName;
        var bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out System.Boolean attached, context, valueNode as IXmlLineInfo);
        IPropertySymbol? propertySymbol = null;

        //one of those will return true, but we need the propertyType
        _ = CanGetValue(parentVar, bpFieldSymbol, attached, context, out var propertyType) || CanGet(parentVar, localName, context, out propertyType, out propertySymbol);

        if (CanAddToResourceDictionary(parentVar, propertyType!, (IElementNode)valueNode, context))
        {
            var rdVar = new LocalVariable(propertyType!, NamingHelpers.CreateUniqueVariableName(context, propertyType!));
            writer.WriteLine($"var {rdVar.Name} = {GetOrGetValue(parentVar, bpFieldSymbol, propertySymbol, valueNode, context)};");
            AddToResourceDictionary(writer, rdVar, (IElementNode)valueNode, context);
            return;
        }

        ITypeSymbol itemType;
        if (propertyType!.ImplementsGeneric(context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1")!, out var typeArguments))
            itemType = typeArguments[0];
        else
            itemType = context.Compilation.ObjectType;

        var adder = propertyType!.GetAllMethods("Add", context).First(m => m.Parameters.Length == 1);
        var receiverType = adder.ReceiverType;
        
        var parentObj = $"{parentVar.Name}.{localName}";
        if (bpFieldSymbol != null)
        {
            var typeandconverter = bpFieldSymbol.GetBPTypeAndConverter(context);
            parentObj = $"(({typeandconverter?.type.ToFQDisplayString()}){parentVar.Name}.GetValue({bpFieldSymbol.ToFQDisplayString()}))";
        }

        if (receiverType is not null && !propertyType!.Equals(receiverType, SymbolEqualityComparer.Default))
            parentObj = $"(({receiverType.ToFQDisplayString()}){parentObj})";

        using (PrePost.NewLineInfo(writer, (IXmlLineInfo)valueNode, context.FilePath))
            writer.WriteLine($"{parentObj}.Add(({itemType.ToFQDisplayString()}){context.Variables[valueNode].Name});");
    }

    static void AddToResourceDictionary(IndentedTextWriter writer, LocalVariable parentVar, IElementNode node, SourceGenContext context)
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
