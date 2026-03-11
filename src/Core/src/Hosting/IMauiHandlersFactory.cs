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

		IElementHandler? GetHandler(Type type);

		IElementHandler? GetHandler<T>() where T : IElement;

		[Obsolete("The handlers collection no longer contains all registered handlers. Use GetHandlerType instead.")]
		IMauiHandlersCollection GetCollection();
	}
}