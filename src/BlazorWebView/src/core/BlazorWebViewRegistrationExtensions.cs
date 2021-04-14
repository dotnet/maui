using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public static class BlazorWebViewRegistrationExtensions
	{
		public static TAppHostBuilder RegisterBlazorMauiWebView<TAppHostBuilder>(this TAppHostBuilder appHostBuilder) where TAppHostBuilder : IAppHostBuilder
		{
			if (appHostBuilder is null)
			{
				throw new ArgumentNullException(nameof(appHostBuilder));
			}

			appHostBuilder.ConfigureMauiHandlers((_, handlers) => handlers.AddHandler<IBlazorWebView, BlazorWebViewHandler>());

			return appHostBuilder;
		}
	}
}
