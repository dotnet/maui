using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

static partial class ITypeSymbolExtensions
{
	public static IFieldSymbol? GetBindableProperty(this ITypeSymbol type, string ns, ref string localName, out System.Boolean attached, SourceGenContext context, IXmlLineInfo? iXmlLineInfo)
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
    public static (ITypeSymbol type, ITypeSymbol? converter)? GetBPTypeAndConverter(this IFieldSymbol fieldSymbol)
    {
        if (!fieldSymbol.Name.EndsWith("Property", StringComparison.InvariantCulture))
            return null;
            // throw new BuildException(BuildExceptionCode.BPName, iXmlLineInfo, null, bpRef.Name);
        var bpName = fieldSymbol.Name.Substring(0, fieldSymbol.Name.Length - 8);
        var owner = fieldSymbol.ContainingType;
        var propertyName = fieldSymbol.Name.Substring(0, fieldSymbol.Name.Length - 8);
        var property = owner.GetMembers(propertyName).OfType<IPropertySymbol>().SingleOrDefault();
        var getter = property?.GetMethod
                  ?? owner.GetMembers($"Get{propertyName}").OfType<IMethodSymbol>().SingleOrDefault(m => m.IsStatic && m.IsPublic() && m.Parameters.Length == 1);
        if (getter == null)
            return null;
            // throw new BuildException(BuildExceptionCode.BPName, iXmlLineInfo, null, bpName, bpRef.DeclaringType);
        
        List<AttributeData> attributes = new();
        if (property != null){
            attributes.AddRange(property.GetAttributes().ToList());
            attributes.AddRange(property.Type.GetAttributes());
        }
        attributes.AddRange(getter.GetAttributes());
        attributes.AddRange(getter.ReturnType.GetAttributes());

        var typeConverter = attributes.FirstOrDefault(ad => ad.AttributeClass?.ToString() == "System.ComponentModel.TypeConverterAttribute")?.ConstructorArguments[0].Value as ITypeSymbol;
        return (getter.ReturnType, typeConverter);
    }
}