using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class BindablePropertyConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["BindableProperty", "Microsoft.Maui.Controls.BindableProperty"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		if (node is not ValueNode vNode)
			return "default";
		var bpRef = NodeSGExtensions.GetBindableProperty(vNode, context);
		if (bpRef is not null)
			return bpRef.ToFQDisplayString();

		// BindableProperty field not found - try heuristic for source-generated properties
		var propertyText = vNode.Value as string ?? string.Empty;
		var parts = propertyText.Split('.');
		ITypeSymbol? targetType = null;
		string propertyName;

		if (parts.Length == 1)
		{
			propertyName = parts[0];
			var parent = vNode.Parent?.Parent as ElementNode ?? (vNode.Parent?.Parent as IListNode)?.Parent as ElementNode;
			if (parent?.XmlType.IsOfAnyType("Trigger", "DataTrigger", "MultiTrigger", "Style") == true)
				targetType = NodeSGExtensions.GetTargetTypeSymbol(parent, context);
		}
		else if (parts.Length == 2)
		{
			targetType = XmlTypeExtensions.GetTypeSymbol(parts[0], context, vNode);
			propertyName = parts[1];
		}
		else
		{
			return "default";
		}

		if (targetType is not null && targetType.HasBindablePropertyHeuristic(propertyName, context, out var explicitPropertyName))
		{
			var bpFieldName = explicitPropertyName ?? $"{propertyName}Property";
			return $"{targetType.ToFQDisplayString()}.{bpFieldName}";
		}

		return "default";
	}
}