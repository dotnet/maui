using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.LifecycleEvents
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureLifecycleEvents(this IAppHostBuilder builder, Action<HostBuilderContext, ILifecycleBuilder> configureDelegate)
		{
			builder.ConfigureServices<LifecycleBuilder>(configureDelegate);

			return builder;
		}

		class LifecycleBuilder : LifecycleEventService, ILifecycleBuilder, IMauiServiceBuilder
		{
			public void ConfigureServices(IServiceCollection services)
			{
				services.AddSingleton<ILifecycleEventService>(this);
			}

			public void Configure(IServiceProvider services)
			{
			}
		}
	}
}