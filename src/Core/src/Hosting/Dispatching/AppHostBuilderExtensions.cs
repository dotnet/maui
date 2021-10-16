using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureCoreServices(this MauiAppBuilder builder)
		{
			builder.Services.TryAddSingleton<IDispatcherProvider>(svc => new DispatcherProvider());
			builder.Services.TryAddScoped(svc =>
			{
				var provider = svc.GetRequiredService<IDispatcherProvider>();
				if (Dispatcher.SetProvider(provider))
					svc.CreateLogger<Dispatcher>()?.LogWarning("Replaced an existing Dispatcher with one from the service provider.");

				return Dispatcher.GetForCurrentThread()!;
			});
			builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IMauiInitializeScopedService, DispatcherInitializer>());

			return builder;
		}

		class DispatcherInitializer : IMauiInitializeScopedService
		{
			public void Initialize(IServiceProvider services)
			{
				_ = services.GetRequiredService<IDispatcher>();
			}
		}
	}
}