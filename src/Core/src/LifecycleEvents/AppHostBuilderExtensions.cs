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

		class LifecycleBuilder : LifecycleEventService, ILifecycleBuilder, IServiceCollectionBuilder
		{
			public void Build(IServiceCollection services)
			{
				services.AddSingleton<ILifecycleEventService>(this);
			}
		}
	}
}