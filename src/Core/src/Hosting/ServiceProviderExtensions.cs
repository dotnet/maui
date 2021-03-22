using System;

namespace Microsoft.Maui.Hosting
{
	public static class ServiceProviderExtensions
	{
		internal static IServiceProvider BuildServiceProvider(this IMauiServiceCollection serviceCollection, bool constructorInjection)
			=> new MauiServiceProvider(serviceCollection, constructorInjection);

		internal static IMauiHandlersServiceProvider BuildHandlersServiceProvider(this IMauiServiceCollection serviceCollection)
			=> new MauiHandlersServiceProvider(serviceCollection);

		public static IViewHandler? GetHandler(this IServiceProvider services, Type type)
			=> services.GetService(type) as IViewHandler;

		public static IViewHandler? GetHandler<T>(this IServiceProvider services) where T : IView
			=> GetHandler(services, typeof(T));

	}
}