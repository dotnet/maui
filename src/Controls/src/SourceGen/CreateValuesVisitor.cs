using System;
using System.Linq;

using Microsoft.Maui.Controls.Xaml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.CodeDom.Compiler;

using static Microsoft.Maui.Controls.SourceGen.TypeHelpers;

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
    public bool IsResourceDictionary(ElementNode node) => false;

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
        //var typeName = GetTypeName(node.XmlType, Context, globalAlias: false);
        var type = node.XmlType.ResolveTypeSymbol(Context);
        //TODO fail if type is null

        //if type is ArrayExtension

        //if type is Xaml2009Primitive
        if (IsXaml2009LanguagePrimitive(node)) {
            var variableName = NamingHelpers.CreateUniqueVariableName(Context, type!.Name!.Split('.').Last().ToLowerInvariant());
            Context.Variables[node] = new LocalVariable(type, variableName);

            Writer.WriteLine($"global::{type.Name} {variableName} = {ValueForLanguagePrimitive(type, node)};");
        }
        
        //suports factorymethod, ctor args, etc
        var namedType = type as INamedTypeSymbol;
        if (namedType != null)
        {
            var ctors = namedType.InstanceConstructors;
            var ctor = ctors.FirstOrDefault(c => c.Parameters.Length == 0);
            if (ctor != null)
            {
                var variableName = NamingHelpers.CreateUniqueVariableName(Context, type!.Name!.Split('.').Last().ToLowerInvariant());
                Context.Variables[node] = new LocalVariable(type, variableName);
                Writer.WriteLine($"global::{type} {variableName} = new global::{type}();");
            }
        }
    }

    public void Visit(ListNode node, INode parentNode)
    {

    }

    public void Visit(RootNode node, INode parentNode)
    {
        var variableName = "__root";                    
        Context.Variables[node] = new LocalVariable(Context.RootType, variableName);
        Writer.WriteLine($"global::{Context.RootType} {variableName} = this;");
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
            case "System.Uri": return $"new global::System.Uri(\"{valueString}\", global:System.UriKind.RelativeOrAbsolute)";
            default: return "default";
        }
    }
}

class LocalVariable (ITypeSymbol type, string name)
{
    public ITypeSymbol Type => type;
    public string Name => name;
}