using System;
using System.Linq;

using Microsoft.Maui.Controls.Xaml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml;
using System.Collections.Immutable;
using System.Collections;

namespace Microsoft.Maui.Controls.SourceGen;

using static LocationHelpers;
class CreateValuesVisitor : IXamlNodeVisitor
{
   public CreateValuesVisitor(SourceGenContext context) => Context = context;

    SourceGenContext Context { get; }
    IndentedTextWriter Writer => Context.Writer;

    public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
    public bool StopOnDataTemplate => true;
    public bool StopOnResourceDictionary => false;
    public bool VisitNodeOnDataTemplate => false;
    public bool SkipChildren(INode node, INode parentNode) => false;
    public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(Context);

    public void Visit(ValueNode node, INode parentNode)
    {
        Context.Values[node] = node.Value;
    }

    public void Visit(MarkupNode node, INode parentNode)
    {
        //At this point, all MarkupNodes are expanded to ElementNodes
    }

    public void Visit(ElementNode node, INode parentNode)
    {
        if (!node.XmlType.TryResolveTypeSymbol(null, Context.Compilation, Context.XmlnsCache, out var type) || type is null)
            return;

        //x:Array
        if (type.Equals(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Xaml.ArrayExtension"), SymbolEqualityComparer.Default))
        {
            //we might want to mive this to a separate method
            var visitor = new SetPropertiesVisitor(Context);
            // var children = node.Properties.Values.ToList();
            // children.AddRange(node.CollectionItems);
            foreach (var cn in node.CollectionItems)
            {
                if (cn is not ElementNode en)
                    continue;
                foreach (var n in en.Properties.Values.ToList())
                    n.Accept(visitor, cn);
                foreach (var n in en.CollectionItems)
                    n.Accept(visitor, cn);
            }

            var typeNode = node.Properties[new XmlName("", "Type")];
            if (!Context.Types.TryGetValue(typeNode, out var arrayType))
                throw new Exception("ArrayExtension Type not found");

            var variableName = NamingHelpers.CreateUniqueVariableName(Context, Context.Compilation.CreateArrayTypeSymbol(arrayType));

            //Provide value for Providers
            foreach (var cn in node.CollectionItems)
            {
                if (cn is not ElementNode en)
                    continue;

                en.TryProvideValue(Context);                
            }

            Context.Variables[node] = new LocalVariable(Context.Compilation.CreateArrayTypeSymbol(arrayType), variableName);
            Writer.WriteLine($"var {variableName} = new {arrayType.ToFQDisplayString()}[]");
            using (PrePost.NewBlock(Writer, begin: "{", end: "};"))
            {
                foreach (var cn in node.CollectionItems)
                {
                    if (cn is not ElementNode en)
                        continue;

                    var enVariable = Context.Variables[en];
                    Writer.WriteLine($"({arrayType.ToFQDisplayString()}){enVariable.Name},");
                }
            }

            //clean the node as it has been fully exhausted
            foreach (var prop in node.Properties)
                if (!node.SkipProperties.Contains(prop.Key))
                    node.SkipProperties.Add(prop.Key);
            node.CollectionItems.Clear();
            return;
        }

		//if type is Xaml2009Primitive
		if (IsXaml2009LanguagePrimitive(node)) {
            var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
            Writer.WriteLine($"{type.ToFQDisplayString()} {variableName} = {ValueForLanguagePrimitive(type, node, Context)};");
            Context.Variables[node] = new LocalVariable(type, variableName);
            node.RegisterSourceInfo(Context, Writer);
            return;
        }


        if (NodeSGExtensions.GetKnownMarkupExtensions(Context).TryGetValue(type, out var handler))
        {
            var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
            Writer.WriteLine($"var {variableName} = {handler(node, Context, out var returnType)};");
            Context.Variables[node] = new LocalVariable(returnType!, variableName);
            node.RegisterSourceInfo(Context, Writer);

            //skip the node as it has been fully exhausted
            foreach (var prop in node.Properties)
                if (!node.SkipProperties.Contains(prop.Key))
                    node.SkipProperties.Add(prop.Key);
            node.CollectionItems.Clear();

            return;
        }

        IMethodSymbol? ctor = null, factoryMethod = null;
        IList<(INode node, ITypeSymbol type, ITypeSymbol? converter)>? parameters = null;
        //ctor with x:Arguments
		if (node.Properties.ContainsKey(XmlName.xArguments) && !node.Properties.ContainsKey(XmlName.xFactoryMethod))
        {
            ctor = type.InstanceConstructors.FirstOrDefault(c 
                => c.MatchXArguments(node, Context, out parameters));
            if (ctor is null)
#pragma warning disable RS0030 // Do not use banned APIs
				Context.ReportDiagnostic(Diagnostic.Create(Descriptors.MethodResolution, LocationCreate(Context.FilePath!, node, type.Name), type.ToDisplayString()));
#pragma warning restore RS0030 // Do not use banned APIs
		}
        //x:FactoryMethod
        else if (node.Properties.ContainsKey(XmlName.xFactoryMethod))
        {
            var factoryMehodName = (node.Properties[XmlName.xFactoryMethod] as ValueNode)!.Value as string;
            //TODO report diagnostic if factoryMethod is null/not a string

            factoryMethod = type.GetAllMethods(factoryMehodName!, Context).FirstOrDefault(m =>
                       m.IsStatic 
                    && m.MatchXArguments(node, Context, out parameters));
            if (factoryMethod is null)
                Context.ReportDiagnostic(Diagnostic.Create(Descriptors.MethodResolution, LocationCreate(Context.FilePath!, node, factoryMehodName!), factoryMehodName));
        }
        
        //does it has a default parameterless ctor ?
        ctor ??= type.InstanceConstructors.FirstOrDefault(c => c.Parameters.Length == 0);

        if (ctor is null && factoryMethod is null)
        {
            //TODO we might need an extension method for that and cache the result. it happens eveytime we have a Style
            ctor = type.InstanceConstructors.FirstOrDefault(c 
                => c.Parameters.Length >=0
                && c.Parameters.All(p => p.GetAttributes().Any(a => a?.AttributeClass?.Equals(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ParameterAttribute")!, SymbolEqualityComparer.Default)?? false))
                );
            //TODO validate ctor arguments
            if (ctor is not null)
            {
                var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
                var paramsTuple = ctor.Parameters
                    .Select(p => (  p.Type, 
                                    p.GetAttributes().FirstOrDefault(a => a.AttributeClass!.Equals(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ParameterAttribute")!, SymbolEqualityComparer.Default))?.ConstructorArguments[0].Value as string,
                                    p.GetAttributes().FirstOrDefault(a => a.AttributeClass!.Equals(Context.Compilation.GetTypeByMetadataName("System.ComponentModel.TypeConverterAttribute")!, SymbolEqualityComparer.Default))?.ConstructorArguments[0].Value as ITypeSymbol
                                    ))
                    .Select(p => (p.Type, new XmlName("", p.Item2), node.Properties[new XmlName("", p.Item2)], p.Item3)).ToList();
                
                parameters = [.. paramsTuple.Select(p => (p.Item3, p.Type, p.Item4))];
                 
                foreach (var n in paramsTuple.Select(p => p.Item2))
                    if (!node.SkipProperties.Contains(n))
                        node.SkipProperties.Add(n);

                var paramString = string.Join(", ", parameters.ToMethodParameters(Context));
                Writer.WriteLine($"var {variableName} = new {type.ToFQDisplayString()}({paramString});");
                Context.Variables[node] = new LocalVariable(type, variableName);
                node.RegisterSourceInfo(Context, Writer);

                return;
            }

        }

        bool isColor = type.Equals(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color"), SymbolEqualityComparer.Default);

        if (   node.CollectionItems.Count == 1 
            && node.CollectionItems[0] is ValueNode valueNode
            && (isColor || type.IsValueType))
        { //<Color>HotPink</Color>
            var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
            var valueString = valueNode.ConvertTo(type, Context);
            
            Writer.WriteLine($"var {variableName} = {valueString};");
            Context.Variables[node] = new LocalVariable(type, variableName);
            node.RegisterSourceInfo(Context, Writer);
            return;
        }
        else if (factoryMethod != null)
        {
            var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
            Writer.WriteLine($"var {variableName} = {factoryMethod.ToFQDisplayString()}({string.Join(", ", parameters?.ToMethodParameters(Context) ?? [])});");
            Context.Variables[node] = new LocalVariable(factoryMethod.ReturnType, variableName);
            node.RegisterSourceInfo(Context, Writer);
            return;
        }
        else if (ctor != null)
        {
            var variableName = NamingHelpers.CreateUniqueVariableName(Context, type);
            
            Writer.WriteLine($"var {variableName} = new {type.ToFQDisplayString()}({string.Join(", ", parameters?.ToMethodParameters(Context) ?? [])});");
            Context.Variables[node] = new LocalVariable(type, variableName);
            node.RegisterSourceInfo(Context, Writer);
            return;
        }
	}

    public void Visit(ListNode node, INode parentNode)
    {
        if (node.TryGetPropertyName(parentNode, out XmlName name))
            node.XmlName = name;
    }

    public void Visit(RootNode node, INode parentNode)
    {
        var variableName = "__root";

        Writer.WriteLine($"var {variableName} = this;");
        Context.Variables[node] = new LocalVariable(Context.RootType, variableName);

        node.RegisterSourceInfo(Context, Writer);
    }
    		
    static bool IsXaml2009LanguagePrimitive(IElementNode node)
    {
        // if (node.NamespaceURI == XamlParser.X2009Uri)
        // {
        //     return node.XmlType.Name != "Array";
        // }
        if (node.NamespaceURI != "clr-namespace:System;assembly=mscorlib" && node.NamespaceURI != XamlParser.X2009Uri)
            return false;
        var name = node.XmlType.Name;
        if (name == "SByte" ||
            name == "Int16" ||
            name == "Int32" ||
            name == "Int64" ||
            name == "Byte" ||
            name == "UInt16" ||
            name == "UInt32" ||
            name == "UInt64" ||
            name == "Single" ||
            name == "Double" ||
            name == "Boolean" ||
            name == "String" ||
            name == "Char" ||
            name == "Decimal" ||
            name == "TimeSpan" ||
            name == "Uri")
            return true;
        return false;
    }

    // TODO duplicate code with NodeSGExtensions.cs, should we share somehow?
    static string ValueForLanguagePrimitive(ITypeSymbol type, ElementNode node, SourceGenContext context)
    {
        var hasValue = node.CollectionItems.Count == 1 && node.CollectionItems[0] is ValueNode &&
                ((ValueNode)node.CollectionItems[0]).Value is string;
        if (!hasValue)
            return "default";
        var valueString = (string)((ValueNode)node.CollectionItems[0]).Value;

        return NodeSGExtensions.ValueForLanguagePrimitive(valueString, toType: type, context, node);
    }
}

class LocalVariable (ITypeSymbol type, string name)
{
    public ITypeSymbol Type => type;
    public string Name => name;
}
