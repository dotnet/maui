using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureDispatching(this MauiAppBuilder builder)
		{
			// register the DispatcherProvider as a singleton for the entire app
			builder.Services.TryAddSingleton<IDispatcherProvider>(svc =>
				// the DispatcherProvider might have already been initialized, so ensure that we are grabbing the
				// Current and putting it in the DI container.
				DispatcherProvider.Current);

			// register a fallback dispatcher when the service provider does not support keyed services
			builder.Services.TryAddSingleton<ApplicationDispatcher>((svc) => new ApplicationDispatcher(GetDispatcher(svc)));
			// register the initializer so we can init the dispatcher in the app thread for the app
			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMauiInitializeService, ApplicationDispatcherInitializer>());

			// register the Dispatcher as a scoped service as there may be different dispatchers per window
			builder.Services.TryAddScoped<IDispatcher>((svc) => GetDispatcher(svc));
			// register the initializer so we can init the dispatcher in the window thread for that window
			builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IMauiInitializeScopedService, DispatcherInitializer>());

			return builder;
		}

		internal static IDispatcher GetRequiredApplicationDispatcher(this IServiceProvider provider)
		{
			if (provider is IKeyedServiceProvider keyed)
			{
				var dispatcher = keyed.GetKeyedService<IDispatcher>(typeof(IApplication));
				if (dispatcher is not null)
					return dispatcher;
			}

			return provider.GetRequiredService<ApplicationDispatcher>().Dispatcher;
		}

		internal static IDispatcher? GetOptionalApplicationDispatcher(this IServiceProvider provider)
		{
			if (provider is IKeyedServiceProvider keyed)
			{
				var dispatcher = keyed.GetKeyedService<IDispatcher>(typeof(IApplication));
				if (dispatcher is not null)
					return dispatcher;
			}

			return provider.GetService<ApplicationDispatcher>()?.Dispatcher;
		}

		static IDispatcher GetDispatcher(IServiceProvider services)
		{
			var provider = services.GetRequiredService<IDispatcherProvider>();
			if (DispatcherProvider.SetCurrent(provider))
				services.CreateLogger<Dispatcher>()?.LogWarning("Replaced an existing DispatcherProvider with one from the service provider.");

			return Dispatcher.GetForCurrentThread()!;
		}

		class ApplicationDispatcherInitializer : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
				_ = services.GetOptionalApplicationDispatcher();
			}
		}

		class DispatcherInitializer : IMauiInitializeScopedService
		{
			public void Initialize(IServiceProvider services)
			{
				_ = services.GetRequiredService<IDispatcher>();
			}
		}

		/// <summary>
		/// The default service provider does not support a single service type for
		/// BOTH a singleton (for the root app) AND a scoped (for the window scope).
		/// This is a small wrapper so we can do the same thing. The preferred way is
		/// actually a keyed service, but this is a new feature that existing factories
		/// may not yet support. Also, this wrapper is not public so it is hard to 
		/// replace/substitute in tests.
		/// </summary>
		internal class ApplicationDispatcher
		{
			public IDispatcher Dispatcher { get; }

			public ApplicationDispatcher(IDispatcher dispatcher)
			{
				Dispatcher = dispatcher;
			}
		}
	}
}