using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

/// <summary>
/// Known value provider for EventTrigger that specializes to EventTrigger<T> based on parent context.
/// Implements AOT-safe code generation for EventTrigger with static lambda-based event subscription.
/// 
/// NOTE: This provider returns false for CanProvideValue because the generic EventTrigger<T> classes
/// require constructor parameters (add/remove handler lambdas). Instead, the normal EventTrigger is created
/// and then SetPropertyHelpers.SetEventTriggerEvent handles the Event property by generating the
/// appropriate AOT-safe EventTrigger<T> replacement.
/// </summary>
internal class EventTriggerValueProvider : IKnownMarkupValueProvider
{
	public bool CanProvideValue(ElementNode node, SourceGenContext context)
	{
		// Do not provide value here - let normal EventTrigger creation happen.
		// SetPropertyHelpers.SetEventTriggerEvent will handle generating AOT-safe code
		// when the Event property is set.
		return false;
	}

	public bool TryProvideValue(ElementNode node, IndentedTextWriter writer, SourceGenContext context, NodeSGExtensions.GetNodeValueDelegate? getNodeValue, out ITypeSymbol? returnType, out string value)
	{
		// This should never be called since CanProvideValue returns false
		returnType = null;
		value = string.Empty;
		return false;
	}
}
