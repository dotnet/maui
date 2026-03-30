using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Top-down visitor that resolves x:DataType for every ElementNode in the tree and stores
/// the result in <see cref="SourceGenContext.BindingContextDataTypes"/>.
///
/// This pre-computes type information so binding compilation can do an O(1) lookup instead
/// of walking up the tree for every binding.
/// </summary>
class PropagateDataTypeVisitor(SourceGenContext context) : IXamlNodeVisitor
{
	public TreeVisitingMode VisitingMode => TreeVisitingMode.TopDown;
	public bool StopOnDataTemplate => true;
	public bool StopOnResourceDictionary => false;
	public bool VisitNodeOnDataTemplate => true;
	public bool SkipChildren(INode node, INode parentNode) => false;
	public bool IsResourceDictionary(ElementNode node) => node.IsResourceDictionary(context);

	public void Visit(ValueNode node, INode parentNode) { }
	public void Visit(MarkupNode node, INode parentNode) { }
	public void Visit(ListNode node, INode parentNode) { }
	public void Visit(RootNode node, INode parentNode) => Visit((ElementNode)node, parentNode);

	public void Visit(ElementNode node, INode parentNode)
	{
		// 1. Check if this node declares x:DataType directly
		if (node.Properties.TryGetValue(XmlName.xDataType, out INode? dataTypeNode))
		{
			var resolved = ResolveDataTypeNode(dataTypeNode, node);
			context.BindingContextDataTypes[node] = resolved;
			return;
		}

		// 2. Try to inherit from parent
		var parentElement = GetParentElement(node);
		if (parentElement is null)
			return;

		if (!context.BindingContextDataTypes.TryGetValue(parentElement, out var parentDataType))
			return;

		// ExplicitNull and Unresolved block inheritance
		if (parentDataType.Kind != BindingContextDataTypeKind.Resolved)
			return;

		// DoesNotInheritDataType blocks inheritance
		if (DoesNotInheritDataType(node))
			return;

		// Check if we're crossing a DataTemplate boundary
		bool crossedBoundary = parentDataType.CrossedTemplateBoundary
			|| node.RepresentsType(XamlParser.MauiUri, "DataTemplate");

		context.BindingContextDataTypes[node] = parentDataType with
		{
			IsInherited = true,
			CrossedTemplateBoundary = crossedBoundary,
		};
	}

	/// <summary>
	/// Resolves an x:DataType property value node into a <see cref="BindingContextDataType"/>.
	/// Handles {x:Null}, {x:Type}, and string type names.
	/// </summary>
	private BindingContextDataType ResolveDataTypeNode(INode dataTypeNode, ElementNode ownerNode)
	{
		// x:DataType="{x:Null}" — explicit opt-out
		if (dataTypeNode.RepresentsType(XamlParser.X2009Uri, "NullExtension"))
		{
			return new BindingContextDataType(
				BindingContextDataTypeKind.ExplicitNull,
				Symbol: null,
				Origin: ownerNode,
				IsInherited: false,
				CrossedTemplateBoundary: false);
		}

		// x:DataType="{x:Type vm:MyViewModel}" — lookup from context.Types
		if (dataTypeNode.RepresentsType(XamlParser.X2009Uri, "TypeExtension"))
		{
			SourceGenContext? ctx = context;
			while (ctx is not null)
			{
				if (ctx.Types.TryGetValue(dataTypeNode, out var symbol))
				{
					return new BindingContextDataType(
						BindingContextDataTypeKind.Resolved,
						Symbol: symbol,
						Origin: ownerNode,
						IsInherited: false,
						CrossedTemplateBoundary: false);
				}
				ctx = ctx.ParentContext;
			}

			return Unresolved(ownerNode);
		}

		// x:DataType="vm:MyViewModel" — string type name
		if (dataTypeNode is ValueNode { Value: string dataTypeName })
		{
			try
			{
				var dataType = TypeArgumentsParser.ParseSingle(
					dataTypeName, ownerNode.NamespaceResolver, dataTypeNode as IXmlLineInfo);

				if (dataType is not null
					&& dataType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, context.TypeCache, out INamedTypeSymbol? resolved)
					&& resolved is not null)
				{
					return new BindingContextDataType(
						BindingContextDataTypeKind.Resolved,
						Symbol: resolved,
						Origin: ownerNode,
						IsInherited: false,
						CrossedTemplateBoundary: false);
				}
			}
			catch (XamlParseException)
			{
				// Fall through to Unresolved
			}
		}

		return Unresolved(ownerNode);
	}

	private static BindingContextDataType Unresolved(ElementNode ownerNode)
		=> new(BindingContextDataTypeKind.Unresolved, Symbol: null, Origin: ownerNode, IsInherited: false, CrossedTemplateBoundary: false);

	private bool DoesNotInheritDataType(ElementNode node)
	{
		return GetParentElement(node) is ElementNode parentNode
			&& parentNode.XmlType.TryResolveTypeSymbol(null, context.Compilation, context.XmlnsCache, context.TypeCache, out INamedTypeSymbol? parentTypeSymbol)
			&& parentTypeSymbol is not null
			&& node.TryGetPropertyName(parentNode, out XmlName propertyName)
			&& parentTypeSymbol.GetAllProperties(propertyName.LocalName, context).FirstOrDefault() is IPropertySymbol propertySymbol
			&& propertySymbol.GetAttributes().Any(a => a.AttributeClass?.ToFQDisplayString() == "global::Microsoft.Maui.Controls.Xaml.DoesNotInheritDataTypeAttribute");
	}

	private static ElementNode? GetParentElement(ElementNode node)
	{
		var parent = node.Parent;
		if (parent is ListNode listNode)
			return listNode.Parent as ElementNode;
		if (parent is ElementNode elementNode)
			return elementNode;
		return null;
	}
}
