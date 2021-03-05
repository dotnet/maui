using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder RegisterCompatibilityRenderers(this IAppHostBuilder builder)
		{
			// This won't really be a thing once we have all the handlers built
			var defaultHandlers = new Dictionary<Type, Type>
			{
				{  typeof(IButton), typeof(ButtonHandler) },
				{  typeof(IEntry), typeof(EntryHandler) },
				{  typeof(ILayout), typeof(LayoutHandler) },
				{  typeof(ILabel), typeof(LabelHandler) },
				{  typeof(ISlider), typeof(SliderHandler) },
				{  typeof(ISwitch), typeof(SwitchHandler) }
			};

			Forms.RegisterCompatRenderers(
				new[] { typeof(RendererToHandlerShim).Assembly } ,
				typeof(RendererToHandlerShim).Assembly,
				(controlType) =>
				{
					foreach(var type in defaultHandlers.Keys)
					{
						if (type.IsAssignableFrom(controlType))
							return;
					}

					builder.RegisterHandler(controlType, typeof(RendererToHandlerShim));
				});

			return builder;
		}

		public static IAppHostBuilder RegisterCompatibilityRenderer(
			this IAppHostBuilder builder, 
			Type controlType,
			Type rendererType)
		{
			// register renderer with old registrar so it can get shimmed
			// This will move to some extension method
			Microsoft.Maui.Controls.Internals.Registrar.Registered.Register(
				controlType,
				rendererType);

			builder.RegisterHandler(controlType, typeof(RendererToHandlerShim));
			return builder;
		}


		public static IAppHostBuilder RegisterCompatibilityRenderer<TControlType, TMauiType, TRenderer>(this IAppHostBuilder builder)
			where TMauiType : IFrameworkElement
		{
			// register renderer with old registrar so it can get shimmed
			// This will move to some extension method
			Controls.Internals.Registrar.Registered.Register(
				typeof(TControlType),
				typeof(TRenderer));

			builder.RegisterHandler<TMauiType, RendererToHandlerShim>();
			return builder;
		}

		public static IAppHostBuilder RegisterCompatibilityRenderer<TControlType, TRenderer>(this IAppHostBuilder builder)
			where TControlType : IFrameworkElement =>		
				builder.RegisterCompatibilityRenderer<TControlType, TControlType, TRenderer>();
		
	}
}
