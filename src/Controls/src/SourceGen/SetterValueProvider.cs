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

		// Value must be a simple ValueNode (not a MarkupNode or ElementNode)
		if (valueNode is MarkupNode or ElementNode)
			return false;

		// All properties must be simple ValueNodes (no ElementNode or MarkupNode)
		foreach (var prop in node.Properties.Values)
		{
			if (prop is MarkupNode or ElementNode)
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
		IFieldSymbol? bpRef = bpNode.GetBindableProperty(context);
		var (bpName, bpType) = GetBindablePropertyNameAndType(bpRef, bpNode, context);

		string targetsetter;
		if (node.Properties.TryGetValue(new XmlName("", "TargetName"), out var targetNode))
			targetsetter = $"TargetName = \"{((ValueNode)targetNode).Value}\", ";
		else
			targetsetter = "";

		if (valueNode is ValueNode vn)
		{
			var valueString = bpRef != null
				? vn.ConvertTo(bpRef, writer, context)
				: (bpType != null ? vn.ConvertTo(bpType, null, writer, context) : string.Empty);
			value = $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpName}, Value = {valueString}}}";
			return true;
		}
		else if (getNodeValue != null)
		{
			var lvalue = getNodeValue(valueNode, bpType ?? context.Compilation.ObjectType);
			value = $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpName}, Value = {lvalue.ValueAccessor}}}";
			return true;
		}
		else if (context.Variables.TryGetValue(valueNode, out var variable))
		{
			value = $"new global::Microsoft.Maui.Controls.Setter {{{targetsetter}Property = {bpName}, Value = {variable.ValueAccessor}}}";
			return true;
		}

		value = string.Empty;
		//FIXME context.ReportDiagnostic
		return false;
	}

	private static (string bpName, ITypeSymbol? bpType) GetBindablePropertyNameAndType(IFieldSymbol? bpRef, ValueNode bpNode, SourceGenContext context)
	{
		if (bpRef != null)
			return (bpRef.ToFQDisplayString(), bpRef.GetBPTypeAndConverter(context)?.type);

		static ITypeSymbol? GetTargetTypeSymbol(INode node, SourceGenContext context)
		{
			var ttnode = (node as ElementNode)?.Properties[new XmlName("", "TargetType")];
			if (ttnode is ValueNode { Value: string tt })
				return XmlTypeExtensions.GetTypeSymbol(tt, context, node);
			if (ttnode != null && context.Types.TryGetValue(ttnode, out var typeSymbol))
				return typeSymbol;
			return null;
		}

		var propertyText = bpNode.Value as string ?? string.Empty;
		var parts = propertyText.Split('.');

		ITypeSymbol? targetType = null;
		string propertyName;

		if (parts.Length == 1)
		{
			propertyName = parts[0];
			var parent = bpNode.Parent?.Parent as ElementNode ?? (bpNode.Parent?.Parent as IListNode)?.Parent as ElementNode;
			if (parent?.XmlType.IsOfAnyType("Trigger", "DataTrigger", "MultiTrigger", "Style") == true)
				targetType = GetTargetTypeSymbol(parent, context);
		}
		else if (parts.Length == 2)
		{
			targetType = XmlTypeExtensions.GetTypeSymbol(parts[0], context, bpNode);
			propertyName = parts[1];
		}
		else
		{
			return ("null", null);
		}

		if (targetType != null && targetType.HasBindablePropertyHeuristic(propertyName, context, out var explicitPropertyName))
		{
			var bpFieldName = explicitPropertyName ?? $"{propertyName}Property";
			var bpName = $"{targetType.ToFQDisplayString()}.{bpFieldName}";
			var bpType = targetType.GetAllProperties(propertyName, context).FirstOrDefault()?.Type;
			return (bpName, bpType);
		}

		return ("null", null);
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
