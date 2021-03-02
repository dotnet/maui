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

		public static IAppHostBuilder RegisterCompatibilityRenderer<TControlType, TMauiType, TRenderer>(this IAppHostBuilder builder)
			where TMauiType : IFrameworkElement
		{
			// register renderer with old registrar so it can get shimmed
			// This will move to some extension method
			Microsoft.Maui.Controls.Internals.Registrar.Registered.Register(
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
