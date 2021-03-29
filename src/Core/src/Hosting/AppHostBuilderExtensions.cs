using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting.Internal;

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
		};

		public static IAppHostBuilder ConfigureMauiHandlers(this IAppHostBuilder builder, Action<HostBuilderContext, IMauiHandlersCollection> configureDelegate)
		{
			builder.ConfigureServices<HandlerCollectionBuilder>(configureDelegate);
			return builder;
		}

		public static IAppHostBuilder UseDefaultMauiHandlers(this IAppHostBuilder builder)
		{
			builder.ConfigureMauiHandlers((_, handlersCollection) => handlersCollection.AddHandlers(DefaultMauiHandlers));
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

		class HandlerCollectionBuilder : MauiServiceCollection, IMauiHandlersCollection, IServiceCollectionBuilder
		{
			public void Build(IServiceCollection services)
			{
				var provider = new MauiHandlersServiceProvider(this);

				services.AddSingleton<IMauiHandlersServiceProvider>(provider);
			}

			public void Configure(IServiceProvider services)
			{
			}
		}
	}
}