using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Handlers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal class ElementHandlerAttribute : Attribute
{
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	readonly Type? _handlerType;

	public ElementHandlerAttribute()
	{
	}

	public ElementHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType)
	{
		_handlerType = handlerType;
	}

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public Type? HandlerType => GetHandlerType();

	/// <summary>
	/// Gets the handler type for the annotated element. A conditional attribute returns <see langword="null"/> when
	/// it declares no handler in the current configuration; resolution then continues as if it were absent.
	/// </summary>
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public virtual Type? GetHandlerType()
		=> _handlerType ?? throw new InvalidOperationException($"{GetType().FullName} must provide a handler type or override {nameof(GetHandlerType)}.");
}
