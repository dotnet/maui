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
		if (!TryGetBindablePropertyNameAndType(bpRef, bpNode, context, out var bpName, out var bpType))
		{
			value = string.Empty;
			return false;
		}

		string targetsetter;
		if (node.Properties.TryGetValue(new XmlName("", "TargetName"), out var targetNode))
			targetsetter = $"TargetName = \"{((ValueNode)targetNode).Value}\", ";
		else
			targetsetter = "";

		if (valueNode is ValueNode vn)
		{
			string valueString;
			if (bpRef != null)
			{
				valueString = vn.ConvertTo(bpRef, writer, context);
			}
			else if (bpType != null)
			{
				valueString = vn.ConvertTo(bpType, null, writer, context);
			}
			else
			{
				value = string.Empty;
				return false;
			}
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

	/// <summary>
	/// Gets the bindable property name and type for a Setter's Property attribute.
	/// Uses the resolved field symbol if available, otherwise uses heuristics for source-generated properties.
	/// </summary>
	/// <returns>True if the property could be resolved, false otherwise.</returns>
	private static bool TryGetBindablePropertyNameAndType(IFieldSymbol? bpRef, ValueNode bpNode, SourceGenContext context, out string bpName, out ITypeSymbol? bpType)
	{
		if (bpRef != null)
		{
			bpName = bpRef.ToFQDisplayString();
			bpType = bpRef.GetBPTypeAndConverter(context)?.type;
			return true;
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
				targetType = NodeSGExtensions.GetTargetTypeSymbol(parent, context);
		}
		else if (parts.Length == 2)
		{
			targetType = XmlTypeExtensions.GetTypeSymbol(parts[0], context, bpNode);
			propertyName = parts[1];
		}
		else
		{
			bpName = string.Empty;
			bpType = null;
			return false;
		}

		if (targetType != null && targetType.HasBindablePropertyHeuristic(propertyName, context, out var explicitPropertyName))
		{
			var bpFieldName = explicitPropertyName ?? $"{propertyName}Property";
			bpName = $"{targetType.ToFQDisplayString()}.{bpFieldName}";
			bpType = targetType.GetAllProperties(propertyName, context).FirstOrDefault()?.Type;
			return true;
		}

		bpName = string.Empty;
		bpType = null;
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
