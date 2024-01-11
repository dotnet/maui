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
			builder.Services.TryAddSingleton<IDispatcherProvider>(svc =>
				// the DispatcherProvider might have already been initialized, so ensure that we are grabbing the
				// Current and putting it in the DI container.
				DispatcherProvider.Current);

			builder.Services.TryAddScoped(svc =>
			{
				var provider = svc.GetRequiredService<IDispatcherProvider>();
				if (DispatcherProvider.SetCurrent(provider))
					svc.CreateLogger<Dispatcher>()?.LogWarning("Replaced an existing DispatcherProvider with one from the service provider.");

				var dispatch = Dispatcher.GetForCurrentThread();

				// Slight behavior change 
				return dispatch ?? svc.GetRequiredService<ApplicationDispatcher>().AppDispatcher;
			});

			builder.Services.AddSingleton<ApplicationDispatcher>();


			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMauiInitializeService, DispatcherInitializer>());

			return builder;
		}

		class DispatcherInitializer : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
				_ = services.GetRequiredService<ApplicationDispatcher>();
			}
		}
	}

	/// <summary>
	/// If the user tries to retrieve `IDispatcher` from the root `IServiceProvider` on a background thread
	/// this serves as a way to retrieve the App level dispatcher
	/// </summary>
	internal class ApplicationDispatcher
	{
		public IDispatcher AppDispatcher { get; }

		public ApplicationDispatcher() : this(Dispatcher.GetForCurrentThread()!)
		{
		}

		internal ApplicationDispatcher(IDispatcher dispatcher)
		{
			AppDispatcher = dispatcher;
		}
	}
}