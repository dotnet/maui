using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Provides deferred creation for EventTrigger to avoid generating dead code.
/// 
/// When an EventTrigger has an Event property, we defer its creation from CreateValuesVisitor
/// to SetPropertiesVisitor so we can use the EventTrigger.Create factory method with the
/// correct target type (which is only known after the parent element is processed).
/// 
/// This approach:
/// 1. Returns true from CanProvideValue so CreateValuesVisitor skips emitting "new EventTrigger()"
/// 2. Registers a variable name so children can reference .Actions
/// 3. SetEventTriggerEvent emits "var x = EventTrigger.Create&lt;T&gt;(...)" when Event property is set
/// </summary>
internal class EventTriggerValueProvider : IKnownMarkupValueProvider
{
	public bool CanProvideValue(ElementNode node, SourceGenContext context)
	{
		// Defer creation if EventTrigger has an Event property.
		// Without Event property, fall back to normal creation.
		return HasEventProperty(node);
	}

	public bool TryProvideValue(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		// We don't provide a value here - SetEventTriggerEvent handles creation.
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
