#nullable enable
using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public interface IMauiHandlersServiceProvider : IMauiServiceProvider
	{
		Type? GetHandlerType(Type iview);

		IFrameworkElementHandler? GetHandler(Type type);

		IFrameworkElementHandler? GetHandler<T>() where T : IView;

		IMauiHandlersCollection GetCollection();
	}
}