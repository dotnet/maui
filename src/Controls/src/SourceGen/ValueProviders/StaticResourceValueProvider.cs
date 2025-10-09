using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;
using static Microsoft.Maui.Controls.SourceGen.NodeSGExtensions;

namespace Microsoft.Maui.Controls.SourceGen.ValueProviders;

class StaticResourceValueProvider : ISGValueProvider
{
	//all of this could/should be better, but is already slightly better than XamlC

	public bool CanProvideValue(ElementNode node, SourceGenContext context, TryGetNodeValueDelegate getNodeValue)
    {
		// If the resource is defined locally, we can return the value directly
		var eNode = (node as ElementNode)!;

		if (!eNode.Properties.TryGetValue(new XmlName(null, "Key"), out INode keyNode) && eNode.CollectionItems.Count != 0)
			keyNode = eNode.CollectionItems[0];
		if (keyNode == null && !eNode.Properties.TryGetValue(new XmlName("", "Key"), out keyNode) && eNode.CollectionItems.Count != 0)
			keyNode = eNode.CollectionItems[0];

		if (keyNode is not ValueNode keyValueNode)
		{
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.XamlParserError, LocationHelpers.LocationCreate(context.ProjectItem.RelativePath!, (IXmlLineInfo)node, ""), "StaticResourceExtension: Key must be a string literal"));
			return false;
		}

		return GetResourceNode(eNode, context, (string)keyValueNode.Value) != null;
	
		// if (resource != null)
		// {
		// 	return true;
		// }

		// if (resource is null || !context.Variables.TryGetValue(resource, out _))
		// 	return false;

		// //if the resource is a string, try to convert it
		// if (resource.CollectionItems.Count == 1 && resource.CollectionItems[0] is ValueNode vn && vn.Value is string)
		// {
		// 	if (node.TryGetPropertyName(node.Parent, out XmlName propertyName) && context.Variables.TryGetValue(node.Parent, out ILocalValue parentVar))
		// 	{
		// 		var localName = propertyName.LocalName;
		// 		var bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out System.Boolean attached, context, node as IXmlLineInfo);
		// 		var propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault();
		// 		var typeandconverter = bpFieldSymbol?.GetBPTypeAndConverter(context);

		// 		var propertyType = typeandconverter?.type ?? propertySymbol?.Type;

		// 		if (propertyType!.Equals(context.Compilation.GetSpecialType(SpecialType.System_String), SymbolEqualityComparer.Default))
		// 		{
		// 			return true;
		// 		}
		// 		// try
		// 		// {
		// 		// 	value = vn.ConvertTo(propertyType!, typeandconverter?.converter, writer, context, parentVar);
		// 		// }
		// 		// catch (Exception)
		// 		// {
		// 		// 	//shouldn't happen, but does
		// 		// 	value = string.Empty;
		// 		// 	returnType = context.Compilation.ObjectType;
		// 		// 	return false;
		// 		// }
				
		// 		return true;
		// 	}
		// }

		//Fallback to runtime resolution of StaticResource
		// return false;        
    }
	

	public DirectValue ProvideDirectValue(ElementNode eNode, IndentedTextWriter writer, SourceGenContext context, TryGetNodeValueDelegate tryGetNodeValue)
	{
		// If the resource is defined locally, we can return the value directly
		if (!eNode.Properties.TryGetValue(new XmlName(null, "Key"), out INode keyNode) && eNode.CollectionItems.Count != 0)
			keyNode = eNode.CollectionItems[0];
		if (keyNode == null && !eNode.Properties.TryGetValue(new XmlName("", "Key"), out keyNode) && eNode.CollectionItems.Count != 0)
			keyNode = eNode.CollectionItems[0];

		var resource = GetResourceNode(eNode, context, (string)((ValueNode)keyNode).Value);
		if (resource != null && tryGetNodeValue(resource, context.Compilation.ObjectType, out var value) && value is DirectValue dValue)
			return dValue;
		
		//if the resource is a string, try to convert it
		if (   resource != null
			&& resource.CollectionItems.Count == 1
			&& resource.CollectionItems[0] is ValueNode vn
			&& vn.Value is string)
		{
			if (eNode.TryGetPropertyName(eNode.Parent, out XmlName propertyName) && context.Variables.TryGetValue(eNode.Parent, out ILocalValue parentVar))
			{
				var localName = propertyName.LocalName;
				var bpFieldSymbol = parentVar.Type.GetBindableProperty(propertyName.NamespaceURI, ref localName, out _, context, eNode as IXmlLineInfo);
				var propertySymbol = parentVar.Type.GetAllProperties(localName, context).FirstOrDefault();
				var typeandconverter = bpFieldSymbol?.GetBPTypeAndConverter(context);

				var propertyType = typeandconverter?.type ?? propertySymbol?.Type;

				if (propertyType!.Equals(context.Compilation.GetSpecialType(SpecialType.System_String), SymbolEqualityComparer.Default))
					return new DirectValue(context.Compilation.GetSpecialType(SpecialType.System_String),$"\"{vn.Value}\"");
				try
				{
					return new DirectValue(propertyType!, vn.ConvertTo(propertyType!, typeandconverter?.converter, writer, context, parentVar));
				}
				catch (Exception)
				{
					//shouldn't happen, but does
					return new DirectValue(context.Compilation.ObjectType, "default");
				}
			}
		}

		//this should never be reached
		return new DirectValue(context.Compilation.ObjectType, "default");
		
	}

	public ILocalValue ProvideValue(ElementNode elementNode, (IndentedTextWriter declarationWriter, IndentedTextWriter? ICWriter) writers, ImmutableArray<ScopeInfo> scopes, SourceGenContext context, TryGetNodeValueDelegate getNodeValue)
	{
		throw new NotImplementedException();
	}

	//FIXME this could be smarter and look into merged RDs
	static ElementNode? GetResourceNode(ElementNode en, SourceGenContext context, string key)
	{
		var n = en;
		while (n != null)
		{
			if (!n.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Resources"), out var resourcesNode))
			{
				var np = n.Parent;
				if (np is ElementNode pen)
					n = pen;
				else if (np is ListNode lnp && lnp.Parent is ElementNode elnp)
					n = elnp;
				else
					n = null;
				continue;
			}
			//single resource in <Resources>
			if (   resourcesNode is ElementNode irn
				&& irn.Properties.TryGetValue(XmlName.xKey, out INode xKeyNode)
				&& xKeyNode is ValueNode xKeyValueNode
				&& xKeyValueNode.Value as string == key)
			{
				return irn as ElementNode;
			}
			//multiple resources in <Resources>
			else if (resourcesNode is ListNode lr)
			{
				foreach (var rn in lr.CollectionItems)
				{
					if (   rn is ElementNode irn2
						&& irn2.Properties.TryGetValue(XmlName.xKey, out INode xKeyNode2)
						&& xKeyNode2 is ValueNode xKeyValueNode2
						&& xKeyValueNode2.Value as string == key)
					{
						return irn2 as ElementNode;
					}
				}
			}
			//explicit ResourceDictionary in Resources
			else if (  resourcesNode is ElementNode resourceDictionary
					&& resourceDictionary.XmlType.Name == "ResourceDictionary")
			{
				foreach (var rn in resourceDictionary.CollectionItems)
				{
					if (   rn is ElementNode irn3
						&& irn3.Properties.TryGetValue(XmlName.xKey, out INode xKeyNode3)
						&& irn3.XmlType.Name != "OnPlatform"
						&& xKeyNode3 is ValueNode xKeyValueNode3
						&& xKeyValueNode3.Value as string == key)
					{
						return irn3 as ElementNode;
					}
				}
			}

			n = n.Parent as ElementNode;
		}
		return null;
	}
}