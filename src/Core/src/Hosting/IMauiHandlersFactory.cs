#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public interface IMauiHandlersFactory : IMauiFactory
	{
		[Obsolete("Use GetHandlerType(IElement) instead.")]
		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		Type? GetHandlerType(Type iview);

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		Type? GetHandlerType(IElement element);

		IElementHandler? GetHandler(IElement element, IMauiContext context);

		[Obsolete("Use GetHandler(IElement) instead.")]
		IElementHandler? GetHandler(Type type);

		[Obsolete("Use GetHandler(IElement) instead.")]
		IElementHandler? GetHandler<T>() where T : IElement;

		IMauiHandlersCollection GetCollection();
	}
}