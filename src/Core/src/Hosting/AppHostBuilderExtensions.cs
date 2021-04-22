#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting.Internal;
using Microsoft.Maui.HotReload;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		static readonly Dictionary<Type, Type> DefaultMauiHandlers = new Dictionary<Type, Type>
		{
			{ typeof(IActivityIndicator), typeof(ActivityIndicatorHandler) },
			{ typeof(IButton), typeof(ButtonHandler) },
			{ typeof(ICheckBox), typeof(CheckBoxHandler) },
			{ typeof(IDatePicker), typeof(DatePickerHandler) },
			{ typeof(IEditor), typeof(EditorHandler) },
			{ typeof(IEntry), typeof(EntryHandler) },
			{ typeof(ILabel), typeof(LabelHandler) },
			{ typeof(ILayout), typeof(LayoutHandler) },
			{ typeof(IPicker), typeof(PickerHandler) },
			{ typeof(IProgress), typeof(ProgressBarHandler) },
			{ typeof(ISearchBar), typeof(SearchBarHandler) },
			{ typeof(ISlider), typeof(SliderHandler) },
			{ typeof(IStepper), typeof(StepperHandler) },
			{ typeof(ISwitch), typeof(SwitchHandler) },
			{ typeof(ITimePicker), typeof(TimePickerHandler) },
			{ typeof(IPage), typeof(PageHandler) },
		};

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

		public static IAppHostBuilder UseMauiHandlers(this IAppHostBuilder builder)
		{
			builder.ConfigureMauiHandlers((_, handlersCollection) => handlersCollection.AddHandlers(DefaultMauiHandlers));
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

		public static IAppHostBuilder ConfigureAppConfiguration(this IAppHostBuilder builder, Action<IConfigurationBuilder> configureDelegate)
		{
			builder.ConfigureAppConfiguration((_, config) => configureDelegate(config));
			return builder;
		}

		public static IAppHostBuilder UseMauiApp<TApp>(this IAppHostBuilder builder)
			where TApp : class, IApplication
		{
			builder.ConfigureServices((context, collection) =>
			{
				collection.AddSingleton<IApplication, TApp>();
			});
			return builder;
		}

		public static IAppHostBuilder UseMauiApp<TApp>(this IAppHostBuilder builder, Func<IServiceProvider, TApp> implementationFactory)
			where TApp : class, IApplication
		{
			builder.ConfigureServices((context, collection) =>
			{
				collection.AddSingleton<IApplication>(implementationFactory);
			});
			return builder;
		}

		public static IAppHostBuilder UseMauiServiceProviderFactory(this IAppHostBuilder builder, bool constructorInjection)
		{
			builder.UseServiceProviderFactory(new MauiServiceProviderFactory(constructorInjection));
			return builder;
		}

		public static IAppHostBuilder EnableHotReload(this IAppHostBuilder builder, string? ideIp = null, int idePort = 9988)
		{
			builder.ConfigureMauiHandlers((context, handlersCollection) =>
			{
				if (handlersCollection is IMauiServiceCollection mauiCollection)
					MauiHotReloadHelper.Init(mauiCollection);
				else
					throw new NotSupportedException("Hot Reload only works with a IMauiServiceCollection");
			});
			Reloadify.Reload.Instance.ReplaceType = (d) => {
				MauiHotReloadHelper.RegisterReplacedView(d.ClassName, d.Type);
			};

			Reloadify.Reload.Instance.FinishedReload = () => {
				MauiHotReloadHelper.TriggerReload();
			};
			Task.Run(async () =>
			{
				try
				{
					var success = await Reloadify.Reload.Init(ideIp, idePort);
					Console.WriteLine($"HotReload Initialize: {success}");
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
			});
			return builder;
		}

		class HandlerCollectionBuilder : MauiServiceCollection, IMauiHandlersCollection, IMauiServiceBuilder
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