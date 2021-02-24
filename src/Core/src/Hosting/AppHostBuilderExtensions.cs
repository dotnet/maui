using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Hosting
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder RegisterHandlers(this IAppHostBuilder builder, Dictionary<Type, Type> handlers)
		{
			foreach (var handler in handlers)
			{
				builder?.ConfigureHandlers((context, handlersCollection) => handlersCollection.AddTransient(handler.Key, handler.Value));
			}

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
				{  typeof(IButton), typeof(ButtonHandler) },
				{  typeof(ILayout), typeof(LayoutHandler) },
				{  typeof(ILabel), typeof(LabelHandler) },
				{  typeof(ISlider), typeof(SliderHandler) },
				{  typeof(ISwitch), typeof(SwitchHandler) }
			});
			return builder;
		}
	}
}
