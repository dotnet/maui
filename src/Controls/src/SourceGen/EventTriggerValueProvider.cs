using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Provides AOT-safe EventTrigger creation using the EventTrigger.Create factory methods.
/// 
/// Unlike other value providers, EventTrigger is handled specially:
/// - GenerateCreateInstanceCall is called from CreateValuesVisitor to emit the declaration early
/// - TryProvideValue returns the pre-existing variable name
/// 
/// The declaration must happen in CreateValuesVisitor because EventTrigger has children
/// (TriggerActions) that need the variable to exist before they're processed in 
/// SetPropertiesVisitor (which uses bottom-up visiting order).
/// </summary>
internal class EventTriggerValueProvider : IKnownMarkupValueProvider
{
	public bool CanProvideValue(ElementNode node, SourceGenContext context)
	{
		// Return true when Event property exists
		return HasEventProperty(node);
	}

	public bool TryProvideValue(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		// The variable was already declared via GenerateCreateInstanceCall called from CreateValuesVisitor.
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

	/// <summary>
	/// Generates the EventTrigger creation expression (without variable declaration).
	/// Writes either "EventTrigger.Create&lt;...&gt;(...);" or "new EventTrigger { ... };".
	/// </summary>
	internal static void GenerateCreateExpression(ElementNode node, SourceGenContext context)
	{
		var writer = context.Writer;
		
		// Get the Event property value
		string? eventName = null;
		if (node.Properties.TryGetValue(new XmlName("", "Event"), out var eventNode) && eventNode is ValueNode eventValueNode)
			eventName = (string)eventValueNode.Value;
		
		// Find target type from XAML tree (parent elements)
		ITypeSymbol? targetType = FindTargetType(node, context);
		
		if (eventName != null && targetType != null)
		{
			// Look up event on target type
			var eventSymbols = targetType.GetAllEvents(eventName, context).ToList();
			if (eventSymbols.Count > 0)
			{
				var eventSymbol = eventSymbols.First();
				var eventType = eventSymbol.Type;
				var invoke = eventType.GetAllMethods("Invoke", context).FirstOrDefault();
				
				if (invoke != null && invoke.Parameters.Length == 2)
				{
					var eventArgsType = invoke.Parameters[1].Type;
					var isGenericEventHandler = !eventArgsType.Equals(
						context.Compilation.GetTypeByMetadataName("System.EventArgs"), 
						SymbolEqualityComparer.Default);
					
					var targetTypeName = targetType.ToFQDisplayString();
					
					if (isGenericEventHandler)
					{
						var eventArgsTypeName = eventArgsType.ToFQDisplayString();
						writer.WriteLine($"global::Microsoft.Maui.Controls.EventTrigger.Create<{targetTypeName}, {eventArgsTypeName}>(\"{eventName}\", static (target, handler) => target.{eventName} += handler, static (target, handler) => target.{eventName} -= handler);");
					}
					else
					{
						writer.WriteLine($"global::Microsoft.Maui.Controls.EventTrigger.Create<{targetTypeName}>(\"{eventName}\", static (target, handler) => target.{eventName} += handler, static (target, handler) => target.{eventName} -= handler);");
					}
					
					// Skip the Event property - it's already set by Create()
					node.SkipProperties.Add(new XmlName("", "Event"));
					return;
				}
			}
		}
		
		// Fallback: use reflection-based EventTrigger
		if (eventName != null)
		{
			writer.WriteLine($"new global::Microsoft.Maui.Controls.EventTrigger {{ Event = \"{eventName}\" }};");
			node.SkipProperties.Add(new XmlName("", "Event"));
		}
		else
		{
			writer.WriteLine($"new global::Microsoft.Maui.Controls.EventTrigger();");
		}
	}

	/// <summary>
	/// Finds the target type for an EventTrigger by walking up the XAML tree.
	/// EventTrigger is typically inside: Element.Triggers -> Element (e.g., Button.Triggers -> Button)
	/// </summary>
	private static ITypeSymbol? FindTargetType(ElementNode eventTriggerNode, SourceGenContext context)
	{
		INode? current = eventTriggerNode;
		
		while (current != null)
		{
			current = current.Parent;
			
			// Skip ListNodes (they're the collection wrapper)
			if (current is ListNode listNode)
				current = listNode.Parent;
			
			// Found an ElementNode - this should be our target
			if (current is ElementNode parentElement && parentElement != eventTriggerNode)
				return parentElement.XmlType.GetTypeSymbol(context);
		}
		
		return null;
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
