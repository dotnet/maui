#nullable enable
using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public interface IMauiHandlersServiceProvider : IMauiServiceProvider
	{
		Type? GetHandlerType(Type iview);

		IElementHandler? GetHandler(Type type);

		IElementHandler? GetHandler<T>() where T : IElement;

		IMauiHandlersCollection GetCollection();
	}
}