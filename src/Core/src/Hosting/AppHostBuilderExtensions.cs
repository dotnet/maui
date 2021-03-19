using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Hosting
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder RegisterHandlers(this IAppHostBuilder builder, Dictionary<Type, Type> handlers)
		{
			foreach (var handler in handlers)
			{
				builder.ConfigureHandlers((context, handlersCollection) => handlersCollection.AddTransient(handler.Key, handler.Value));
			}

			return builder;
		}

		public static IAppHostBuilder RegisterHandler(this IAppHostBuilder builder, Type viewType, Type handlerType)
		{
			builder.ConfigureHandlers((context, handlersCollection) => handlersCollection.AddTransient(viewType, handlerType));
			return builder;
		}

		public static IAppHostBuilder RegisterHandler<TType, TTypeRender>(this IAppHostBuilder builder)
			where TType : IFrameworkElement
			where TTypeRender : IViewHandler
		{
			builder.ConfigureHandlers((context, handlersCollection) => handlersCollection.AddTransient(typeof(TType), typeof(TTypeRender)));

			return builder;
		}

		public static IAppHostBuilder UseMauiHandlers(this IAppHostBuilder builder)
		{
			builder.RegisterHandlers(new Dictionary<Type, Type>
			{
				{ typeof(IActivityIndicator), typeof(ActivityIndicatorHandler) },
				{ typeof(IButton), typeof(ButtonHandler) },
				{ typeof(ICheckBox), typeof(CheckBoxHandler) },
        { typeof(IDatePicker), typeof(DatePickerHandler) },
				{ typeof(IEditor), typeof(EditorHandler) },
				{ typeof(IEntry), typeof(EntryHandler) },
				{ typeof(ILayout), typeof(LayoutHandler) },
				{ typeof(ILabel), typeof(LabelHandler) },
				{ typeof(IPicker), typeof(PickerHandler) },
				{ typeof(IProgress), typeof(ProgressBarHandler) },
				{ typeof(ISlider), typeof(SliderHandler) },
				{ typeof(IStepper), typeof(StepperHandler) },
				{ typeof(ISwitch), typeof(SwitchHandler) },
				{ typeof(ITimePicker), typeof(TimePickerHandler) }
			});

			return builder;
		}

		public static IAppHostBuilder UseFonts(this IAppHostBuilder builder)
		{
			builder.ConfigureServices((context, collection) =>
			{
				collection.AddSingleton<IEmbeddedFontLoader, EmbeddedFontLoader>();
				collection.AddSingleton<IFontRegistrar>(provider => new FontRegistrar(provider.GetRequiredService<IEmbeddedFontLoader>()));
				collection.AddSingleton<IFontManager>(provider => new FontManager(provider.GetRequiredService<IFontRegistrar>()));
			});
			return builder;
		}
	}
}
