using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Handlers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal class ElementHandlerAttribute : Attribute
{
	public ElementHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type handlerType)
	{
		HandlerType = handlerType;
	}

	public virtual IElementHandler CreateHandler(IMauiContext context)
	{
		object? handler = Activator.CreateInstance(HandlerType);
		if (handler is IElementHandler elementHandler)
		{
			return elementHandler;
		}

		if (typeof(IElementHandler).IsAssignableFrom(HandlerType))
		{
			throw new InvalidOperationException($"Could not create an instance of handler type {HandlerType} for element handler. Ensure it has a public parameterless constructor.");
		}
		else
		{
			throw new InvalidOperationException($"The specified handler type {HandlerType} does not implement {nameof(IElementHandler)}.");
		}
	}

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	public virtual Type HandlerType { get; }
}

#if ANDROID
internal sealed class ElementHandlerWithAndroidContextAttribute<THandler> : ElementHandlerAttribute
	where THandler : IElementHandler, IElementHandlerWithAndroidContext<THandler>
{
	public ElementHandlerWithAndroidContextAttribute() : base(typeof(THandler)) { }

	public override IElementHandler CreateHandler(IMauiContext context)
		=> THandler.CreateHandler(context.Context!); // TODO: revisit nullability

	public override Type HandlerType => typeof(THandler);
}
#endif
