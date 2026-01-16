#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public interface IMauiHandlersFactory : IMauiFactory
	{
		Type? GetHandlerType(Type iview);

		[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
		Type? GetConstructibleHandlerType(Type iview);

		IElementHandler? GetHandler(Type type, IMauiContext context);

		IElementHandler? GetHandler<T>(IMauiContext context) where T : IElement;

		IMauiHandlersCollection GetCollection();
	}
}