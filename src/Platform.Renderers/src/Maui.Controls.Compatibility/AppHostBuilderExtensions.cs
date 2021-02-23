using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xamarin.Forms;
using Xamarin.Platform;
using Xamarin.Platform.Handlers;
using Xamarin.Platform.Hosting;

namespace Maui.Controls.Compatibility
{
	public static class AppHostBuilderExtensions
	{

		public static IAppHostBuilder RegisterCompatibilityRenderer<TControlType, TMauiType, TRenderer>(this IAppHostBuilder builder)
			where TMauiType : IFrameworkElement
		{
			// register renderer with old registrar so it can get shimmed
			// This will move to some extension method
			Xamarin.Forms.Internals.Registrar.Registered.Register(
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
