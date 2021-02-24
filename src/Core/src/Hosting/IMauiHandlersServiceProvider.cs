using System;

namespace Microsoft.Maui
{
	public interface IMauiHandlersServiceProvider : IServiceProvider
	{
		IViewHandler? GetHandler(Type type);

		IViewHandler? GetHandler<T>() where T : IView;
	}
}
