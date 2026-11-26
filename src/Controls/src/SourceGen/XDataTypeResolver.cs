using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Helper to resolve x:DataType from XAML element tree.
/// Extracted from KnownMarkups.BindingMarkup for reuse in expression resolution.
/// </summary>
internal static class XDataTypeResolver
{
	/// <summary>
	/// Tries to get the x:DataType type symbol for an element by walking up the element tree.
	/// </summary>
	/// <param name="node">The element node to start from</param>
	/// <param name="context">The source generation context</param>
	/// <param name="dataTypeSymbol">The resolved type symbol if found</param>
	/// <returns>True if x:DataType was found and resolved</returns>
	public static bool TryGetXDataType(ElementNode node, SourceGenContext context, out ITypeSymbol? dataTypeSymbol)
	{
		dataTypeSymbol = null;

		if (!TryFindXDataTypeNode(node, context, out INode? dataTypeNode, out _) || dataTypeNode is null)
			return false;

		var scopeNode = dataTypeNode.Parent as ElementNode ?? node;
		return TryResolveXDataTypeNode(
			dataTypeNode,
			scopeNode,
			context.Compilation,
			context.XmlnsCache,
			context.TypeCache,
			context,
			out dataTypeSymbol);
	}

	/// <summary>
	/// Resolves an x:DataType property value node (ValueNode, MarkupNode, or TypeExtension ElementNode) to a type symbol.
	/// </summary>
	public static bool TryResolveXDataTypeNode(
		INode dataTypeNode,
		ElementNode scopeNode,
		Compilation compilation,
		AssemblyAttributes xmlnsCache,
		IDictionary<XmlType, INamedTypeSymbol> typeCache,
		SourceGenContext? context,
		out ITypeSymbol? dataTypeSymbol)
	{
		dataTypeSymbol = null;

		if (dataTypeNode.RepresentsType(XamlParser.X2009Uri, "NullExtension"))
			return false;

		// TypeExtension: check context cache first (when already created), otherwise parse before CreateValuesVisitor runs.
		if (dataTypeNode is ElementNode typeElement
			&& (typeElement.RepresentsType(XamlParser.X2009Uri, "TypeExtension")
				|| typeElement.RepresentsType(XamlParser.X2006Uri, "TypeExtension")
				|| typeElement.RepresentsType(XamlParser.X2009Uri, "Type")
				|| typeElement.RepresentsType(XamlParser.X2006Uri, "Type")))
		{
			var ctx = context;
			while (ctx is not null)
			{
				if (ctx.Types.TryGetValue(dataTypeNode, out dataTypeSymbol))
					return true;
				ctx = ctx.ParentContext;
			}

			XmlType? typeExtensionXmlType;
			try
			{
				typeExtensionXmlType = TypeArgumentsParser.ParseTypeExtension(typeElement, dataTypeNode as IXmlLineInfo);
			}
			catch (XamlParseException)
			{
				return false;
			}

			if (typeExtensionXmlType is not null
				&& typeExtensionXmlType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out INamedTypeSymbol? typeExtensionSymbol))
			{
				dataTypeSymbol = typeExtensionSymbol;
				return true;
			}

			return false;
		}

		// ValueNode with type name string or unexpanded MarkupNode (e.g., {x:Type local:MyViewModel}).
		string? dataTypeName = dataTypeNode switch
		{
			ValueNode valueNode => valueNode.Value as string,
			MarkupNode markupNode => markupNode.MarkupString,
			_ => null,
		};
		if (dataTypeName is null)
			return false;

		XmlType? dataType;
		try
		{
			dataType = TypeArgumentsParser.ParseSingle(dataTypeName, scopeNode.NamespaceResolver, dataTypeNode as IXmlLineInfo);
		}
		catch (XamlParseException)
		{
			return false;
		}

		if (dataType is null)
			return false;

		if (!dataType.TryResolveTypeSymbol(null, compilation, xmlnsCache, typeCache, out INamedTypeSymbol? symbol))
			return false;

		dataTypeSymbol = symbol;
		return true;
	}

	/// <summary>
	/// Walks up the element tree to find the nearest x:DataType declaration.
	/// </summary>
	private static bool TryFindXDataTypeNode(ElementNode elementNode, SourceGenContext context, out INode? dataTypeNode, out bool isInOuterScope)
	{
		isInOuterScope = false;
		dataTypeNode = null;

		// Special handling for BindingContext={Binding ...}
		ElementNode? skipNode = null;
		if (IsBindingContextBinding(elementNode))
		{
			skipNode = GetParent(elementNode);
		}

		ElementNode? node = elementNode;
		while (node is not null)
		{
			if (node != skipNode && node.Properties.TryGetValue(XmlName.xDataType, out dataTypeNode))
				return true;

			if (DoesNotInheritDataType(node, context))
				return false;

			if (node.RepresentsType(XamlParser.MauiUri, "DataTemplate"))
				isInOuterScope = true;

			node = GetParent(node);
		}

		return false;
	}

	private static bool IsBindingContextBinding(ElementNode elementNode)
	{
		return GetParent(elementNode) is ElementNode parentNode
			&& elementNode.TryGetPropertyName(parentNode, out var propertyName)
			&& propertyName.NamespaceURI == ""
			&& propertyName.LocalName == "BindingContext";
	}

	private static bool DoesNotInheritDataType(ElementNode node, SourceGenContext context)
	{
		return GetParent(node) is ElementNode parentNode
			&& parentNode.XmlType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, context.TypeCache, out INamedTypeSymbol? parentTypeSymbol)
			&& parentTypeSymbol is not null
			&& node.TryGetPropertyName(parentNode, out XmlName propertyName)
			&& parentTypeSymbol.GetAllProperties(propertyName.LocalName, context).FirstOrDefault() is IPropertySymbol propertySymbol
			&& propertySymbol.GetAttributes().Any(a => a.AttributeClass?.ToFQDisplayString() == "global::Microsoft.Maui.Controls.Xaml.DoesNotInheritDataTypeAttribute");
	}

	private static ElementNode? GetParent(ElementNode node)
	{
		var parent = node.Parent;
		if (parent is ListNode listNode)
			return listNode.Parent as ElementNode;
		if (parent is ElementNode elementNode)
			return elementNode;
		return null;
	}
}
