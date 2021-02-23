using System;

namespace Xamarin.Platform.Hosting
{
	public static class ServiceProviderExtensions
	{
		internal static IServiceProvider BuildServiceProvider(this IMauiServiceCollection serviceCollection)
			=> new MauiServiceProvider(serviceCollection);

		internal static IMauiHandlersServiceProvider BuildHandlersServiceProvider(this IMauiServiceCollection serviceCollection)
			=> new MauiHandlersServiceProvider(serviceCollection);

		public static IViewHandler? GetHandler(this IServiceProvider services, Type type)
			=> services.GetService(type) as IViewHandler;

		public static IViewHandler? GetHandler<T>(this IServiceProvider services) where T : IView
			=> GetHandler(services, typeof(T));

	}
}