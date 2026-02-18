#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public interface IMauiHandlersFactory : IMauiFactory
	{
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		Type? GetHandlerType(Type iview);

		[Obsolete("Use GetHandlerType instead.")]
		IElementHandler? GetHandler(Type type, IMauiContext context);

		[Obsolete("Use GetHandlerType instead.")]
		IElementHandler? GetHandler<T>(IMauiContext context) where T : IElement;

		[Obsolete("The handlers collection no longer contains all registered handlers. Use GetHandlerType instead.")]
		IMauiHandlersCollection GetCollection();
	}
}