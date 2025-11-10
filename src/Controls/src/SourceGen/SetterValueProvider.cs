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
		
		// Get the value node (shared logic with TryProvideValue)
		var valueNode = GetValueNode(node);

		// Value must be a simple ValueNode (not a markup extension or element)
		if (valueNode is not ValueNode)
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
		returnType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Setter")!;

		// Get the value node (shared logic with CanProvideValue)
		var valueNode = GetValueNode(node);
		if (valueNode == null)
		{
			value = string.Empty;
			return false;
		}

		var bpNode = (ValueNode)node.Properties[new XmlName("", "Property")];
		var bpRef = bpNode.GetBindableProperty(context);

		string targetsetter;
		if (node.Properties.TryGetValue(new XmlName("", "TargetName"), out var targetNode))
			targetsetter = $"TargetName = \"{((ValueNode)targetNode).Value}\", ";
		else
			targetsetter = "";

		if (valueNode is ValueNode vn)
		{
			value = $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpRef.ToFQDisplayString()}, Value = {vn.ConvertTo(bpRef, writer, context)}}}";
			return true;
		}
		else if (getNodeValue != null)
		{
			var lvalue = getNodeValue(valueNode, bpRef.Type);
			value = $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpRef.ToFQDisplayString()}, Value = {lvalue.ValueAccessor}}}";
			return true;
		}
		else if (context.Variables.TryGetValue(valueNode, out var variable))
		{
			value = $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpRef.ToFQDisplayString()}, Value = {variable.ValueAccessor}}}";
			return true;
		}

		value = string.Empty;
		//FIXME context.ReportDiagnostic
		return false;
	}

	/// <summary>
	/// Shared helper to get the value node from a Setter element.
	/// Checks properties first, then collection items.
	/// </summary>
	private static INode? GetValueNode(ElementNode node)
	{
		INode? valueNode = null;
		if (!node.Properties.TryGetValue(new XmlName("", "Value"), out valueNode) &&
			!node.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Value"), out valueNode) &&
			node.CollectionItems.Count == 1)
			valueNode = node.CollectionItems[0];

		return valueNode;
	}
}
