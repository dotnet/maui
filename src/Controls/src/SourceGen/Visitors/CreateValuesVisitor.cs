using System;
using System.Linq;

using Microsoft.Maui.Controls.Xaml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Maui.Controls.SourceGen;

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
        var type = node.XmlType.ResolveTypeSymbol(Context) 
            ?? throw new Exception($"Type {node.XmlType.Name} not found");

		//if type is ArrayExtension

		//if type is Xaml2009Primitive
		if (IsXaml2009LanguagePrimitive(node)) {
            var variableName = NamingHelpers.CreateUniqueVariableName(Context, type!.Name!.Split('.').Last());
            Context.Variables[node] = new LocalVariable(type, variableName);

            Writer.WriteLine($"{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {variableName} = {ValueForLanguagePrimitive(type, node)};");
            return;
        }

        //TODO xome markup extensions can be vcompiled here

		//TODO suports factorymethod, ctor args, etc

        IMethodSymbol? ctor = null;

        //does it has a default parameterless ctor ?
		if (type is INamedTypeSymbol namedType)
		{
			var ctors = namedType.InstanceConstructors;
			ctor = ctors.FirstOrDefault(c => c.Parameters.Length == 0);
		}

        bool isColor = type.Equals(Context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color"), SymbolEqualityComparer.Default);

        if (   node.CollectionItems.Count == 1 
            && node.CollectionItems[0] is ValueNode valueNode
            && (isColor || type.IsValueType))
        { //<Color>HotPink</Color>
            var variableName = NamingHelpers.CreateUniqueVariableName(Context, type!.Name!.Split('.').Last());
            Context.Variables[node] = new LocalVariable(type, variableName);
            var valueString = valueNode.ConvertTo(type, Context, valueNode as IXmlLineInfo);
            Writer.WriteLine($"var {variableName} = {valueString};");
            return;
        } 
        else if (ctor != null)
        {
            var variableName = NamingHelpers.CreateUniqueVariableName(Context, type!.Name!.Split('.').Last());
            Context.Variables[node] = new LocalVariable(type, variableName);

            Writer.WriteLine($"var {variableName} = new {type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}();");
            return;
        }
	}

    public void Visit(ListNode node, INode parentNode)
    {

    }

    public void Visit(RootNode node, INode parentNode)
    {
        var variableName = "__root";                    
        Context.Variables[node] = new LocalVariable(Context.RootType, variableName);
        Writer.WriteLine($"{Context.RootType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {variableName} = this;");
    }
    		
    static bool IsXaml2009LanguagePrimitive(IElementNode node)
    {
        if (node.NamespaceURI == XamlParser.X2009Uri)
        {
            var n = node.XmlType.Name.Split(':')[1];
            return n != "Array";
        }
        if (node.NamespaceURI != "clr-namespace:System;assembly=mscorlib")
            return false;
        var name = node.XmlType.Name.Split(':')[1];
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

    static string ValueForLanguagePrimitive(ITypeSymbol type, ElementNode node)
    {
        var hasValue = node.CollectionItems.Count == 1 && node.CollectionItems[0] is ValueNode &&
                ((ValueNode)node.CollectionItems[0]).Value is string;
        if (!hasValue)
            return "default";
        var valueString = (string)((ValueNode)node.CollectionItems[0]).Value;
        switch (type.ToString())
        {
            case "System.SByte":
            case "System.Int16":
            case "System.Int32":
            case "System.Int64":
            case "System.Byte":
            case "System.UInt16":
            case "System.UInt32":
            case "System.UInt64":
            case "System.Single":
            case "System.Double":
            case "System.Decimal": return valueString;
            case "System.Boolean": return valueString.ToLowerInvariant();
            case "System.String": return $"\"{valueString}\"";
            case "System.Object": return "new()";
            case "System.Char": return $"'{valueString}'";
            case "System.TimeSpan":
                var span = TimeSpan.Parse(valueString);
                return $"new global::System.TimeSpan({span.Ticks})";
            case "System.Uri": return $"new global::System.Uri(\"{valueString}\", global::System.UriKind.RelativeOrAbsolute)";
            default: return "default";
        }
    }
}

class LocalVariable (ITypeSymbol type, string name)
{
    public ITypeSymbol Type => type;
    public string Name => name;
}