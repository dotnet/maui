using System;

namespace Xamarin.Platform
{
	public interface IMauiHandlersServiceProvider : IServiceProvider
	{
		IViewHandler? GetHandler(Type type);

		IViewHandler? GetHandler<T>() where T : IView;
	}
}
