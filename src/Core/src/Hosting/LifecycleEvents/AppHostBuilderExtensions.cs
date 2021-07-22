using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.LifecycleEvents
{
	public static partial class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureLifecycleEvents(this IAppHostBuilder builder, Action<ILifecycleBuilder> configureDelegate)
		{
			builder.ConfigureServices<LifecycleBuilder>((_, lifecycle) => configureDelegate(lifecycle));

			return builder;
		}

		public static IAppHostBuilder ConfigureLifecycleEvents(this IAppHostBuilder builder, Action<HostBuilderContext, ILifecycleBuilder> configureDelegate)
		{
			builder.ConfigureServices<LifecycleBuilder>(configureDelegate);

			return builder;
		}

		class LifecycleBuilder : LifecycleEventService, ILifecycleBuilder, IMauiServiceBuilder
		{
			public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
			{
				services.AddSingleton<ILifecycleEventService>(this);
			}

			public void Configure(HostBuilderContext context, IServiceProvider services)
			{
			}
		}
	}
}