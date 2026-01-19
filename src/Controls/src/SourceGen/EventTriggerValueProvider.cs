using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Provides AOT-safe EventTrigger creation using the EventTrigger.Create factory methods.
/// 
/// Unlike other value providers, EventTrigger is handled specially:
/// - CreateValuesVisitor emits the declaration (either factory call or reflection-based)
/// - This provider returns true with the pre-existing variable name
/// 
/// The declaration must happen in CreateValuesVisitor because EventTrigger has children
/// (TriggerActions) that need the variable to exist before they're processed in 
/// SetPropertiesVisitor (which uses bottom-up visiting order).
/// </summary>
internal class EventTriggerValueProvider : IKnownMarkupValueProvider
{
	public bool CanProvideValue(ElementNode node, SourceGenContext context)
	{
		// Return true when Event property exists - CreateValuesVisitor will handle generation
		return HasEventProperty(node);
	}

	public bool TryProvideValue(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		// The variable was already declared in CreateValuesVisitor.
		// Return true with the variable name to prevent any additional code generation.
		if (context.Variables.TryGetValue(node, out var variable))
		{
			returnType = variable.Type;
			value = variable.ValueAccessor;
			return true;
		}
		
		// Shouldn't happen - CreateValuesVisitor always registers the variable
		returnType = null;
		value = string.Empty;
		return false;
	}

	internal static bool HasEventProperty(ElementNode node)
	{
		foreach (var key in node.Properties.Keys)
		{
			if (key is XmlName xmlName && xmlName.LocalName == "Event")
				return true;
		}
		return false;
	}
}
