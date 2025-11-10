using System.CodeDom.Compiler;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

internal class SetterValueProvider : IKnownMarkupValueProvider
{
	public bool CanProvideValue(ElementNode node, SourceGenContext context)
	{
		// Can only inline if all properties are simple ValueNodes (no markup extensions)
		// We need to check both the properties and any collection items
		
		// Check if Property is present and is a simple value
		if (!node.Properties.TryGetValue(new XmlName("", "Property"), out INode? propertyNode) ||
			propertyNode is not ValueNode)
			return false;

		// Check if Value is present
		INode? valueNode = null;
		if (!node.Properties.TryGetValue(new XmlName("", "Value"), out valueNode) &&
			!node.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Value"), out valueNode) &&
			node.CollectionItems.Count == 1)
			valueNode = node.CollectionItems[0];

		// Value must be a simple ValueNode (not a markup extension or element)
		if (valueNode is not ValueNode)
			return false;

		// TargetName (if present) must be a simple value
		if (node.Properties.TryGetValue(new XmlName("", "TargetName"), out var targetNode) &&
			targetNode is not ValueNode)
			return false;

		// All properties must be simple values (no ElementNode or MarkupNode)
		foreach (var prop in node.Properties.Values)
		{
			if (prop is not ValueNode)
				return false;
		}

		return true;
	}

	public bool TryProvideValue(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		return KnownMarkups.ProvideValueForSetter(node, writer, context, getNodeValue, out returnType, out value);
	}
}
