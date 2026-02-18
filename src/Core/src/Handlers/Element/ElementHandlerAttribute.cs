using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Handlers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ElementHandlerAttribute : Attribute
{
#if ANDROID
	private static readonly Type[] s_androidContextConstructorSignature = [typeof(global::Android.Content.Context)];
#endif

	public ElementHandlerAttribute([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType)
	{
		HandlerType = handlerType;
	}

	public virtual IElementHandler CreateHandler(IMauiContext context)
	{
		object? handler;

#if ANDROID
		if (HandlerType.GetConstructor(s_androidContextConstructorSignature) is {} constructor)
		{
			handler = constructor.Invoke([context.Context]);
		}
		else
#endif
		{
			handler = Activator.CreateInstance(HandlerType);
		}

		if (handler is IElementHandler elementHandler)
		{
			return elementHandler;
		}

		if (typeof(IElementHandler).IsAssignableFrom(HandlerType))
		{
			throw new InvalidOperationException($"Could not create an instance of handler type {HandlerType} for element handler. Ensure it has a public parameterless constructor"
#if ANDROID
				+ " or a constructor that accepts an Android.Content.Context parameter"
#endif
				+ ".");
		}
		else
		{
			throw new InvalidOperationException($"The specified handler type {HandlerType} does not implement {nameof(IElementHandler)}.");
		}
	}

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
	public Type HandlerType { get; }
}
