#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IAppHostBuilder ConfigureMauiHandlers(this IAppHostBuilder builder, Action<IMauiHandlersCollection> configureDelegate)
		{
			builder.ConfigureServices<HandlerCollectionBuilder>((_, handlers) => configureDelegate(handlers));
			return builder;
		}

		public static IAppHostBuilder ConfigureMauiHandlers(this IAppHostBuilder builder, Action<HostBuilderContext, IMauiHandlersCollection> configureDelegate)
		{
			builder.ConfigureServices<HandlerCollectionBuilder>(configureDelegate);
			return builder;
		}

		public static IAppHostBuilder ConfigureServices(this IAppHostBuilder builder, Action<IServiceCollection> configureDelegate)
		{
			builder.ConfigureServices((_, services) => configureDelegate(services));
			return builder;
		}

		public static IAppHostBuilder ConfigureServices<TBuilder>(this IAppHostBuilder builder, Action<TBuilder> configureDelegate)
			where TBuilder : IMauiServiceBuilder, new()
		{
			builder.ConfigureServices<TBuilder>((_, services) => configureDelegate(services));
			return builder;
		}

		public static IAppHostBuilder ConfigureServices<TBuilder>(this IAppHostBuilder builder)
			where TBuilder : IMauiServiceBuilder, new()
		{
			builder.ConfigureServices<TBuilder>((_, services) => { });
			return builder;
		}

		public static IAppHostBuilder ConfigureAppConfiguration(this IAppHostBuilder builder, Action<IConfigurationBuilder> configureDelegate)
		{
			builder.ConfigureAppConfiguration((_, config) => configureDelegate(config));
			return builder;
		}

		public static IAppHostBuilder UseMauiServiceProviderFactory(this IAppHostBuilder builder, bool constructorInjection)
		{
			builder.UseServiceProviderFactory(new MauiServiceProviderFactory(constructorInjection));
			return builder;
		}

		public static IAppHostBuilder UseMicrosoftExtensionsServiceProviderFactory(this IAppHostBuilder builder)
		{
			builder.UseServiceProviderFactory(new DIExtensionsServiceProviderFactory());
			return builder;
		}

		// To use the Microsoft.Extensions.DependencyInjection ServiceCollection and not the MAUI one
		class DIExtensionsServiceProviderFactory : IServiceProviderFactory<ServiceCollection>
		{
			public ServiceCollection CreateBuilder(IServiceCollection services)
				=> new ServiceCollection { services };

			public IServiceProvider CreateServiceProvider(ServiceCollection containerBuilder)
				=> containerBuilder.BuildServiceProvider();
		}

		class HandlerCollectionBuilder : MauiHandlersCollection, IMauiServiceBuilder
		{
			public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
			{
				var provider = new MauiHandlersServiceProvider(this);

				services.AddSingleton<IMauiHandlersServiceProvider>(provider);
			}

			public void Configure(HostBuilderContext context, IServiceProvider services)
			{
			}
		}
	}
}
