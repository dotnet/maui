#nullable enable
using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui
{
	public interface IMauiHandlersServiceProvider : IMauiServiceProvider
	{
		Type? GetHandlerType(Type iview);

		IViewHandler? GetHandler(Type type);

		IViewHandler? GetHandler<T>() where T : IView;

		IMauiHandlersCollection GetCollection();
	}
}