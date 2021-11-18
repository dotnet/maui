#nullable enable
using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public interface IMauiHandlersFactory : IMauiFactory
	{
		Type? GetHandlerType(Type iview);

		IElementHandler? GetHandler(Type type);

		IElementHandler? GetHandler<T>() where T : IElement;

		IMauiHandlersCollection GetCollection();
	}
}