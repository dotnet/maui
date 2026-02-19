using System;
using System.Linq;
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

		if (dataTypeNode.RepresentsType(XamlParser.X2009Uri, "NullExtension"))
			return false;

		// TypeExtension: lookup from context's type cache
		if (dataTypeNode.RepresentsType(XamlParser.X2009Uri, "TypeExtension"))
		{
			SourceGenContext? ctx = context;
			while (ctx is not null)
			{
				if (ctx.Types.TryGetValue(dataTypeNode, out dataTypeSymbol))
					return true;
				ctx = ctx.ParentContext;
			}
			return false;
		}

		// Value node with type name string
		string? dataTypeName = (dataTypeNode as ValueNode)?.Value as string;
		if (dataTypeName is null)
			return false;

		XmlType? dataType = null;
		try
		{
			dataType = TypeArgumentsParser.ParseSingle(dataTypeName, node.NamespaceResolver, dataTypeNode as System.Xml.IXmlLineInfo);
		}
		catch (XamlParseException)
		{
			return false;
		}

		if (dataType is null)
			return false;

		if (!dataType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, context.TypeCache, out INamedTypeSymbol? symbol))
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
