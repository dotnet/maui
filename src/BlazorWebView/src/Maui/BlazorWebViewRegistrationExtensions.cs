using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Extension methods to <see cref="MauiAppBuilder"/>.
	/// </summary>
	public static class BlazorWebViewRegistrationExtensions
	{
		/// <summary>
		/// Configures <see cref="MauiAppBuilder"/> to add support for <see cref="BlazorWebView"/>.
		/// </summary>
		/// <param name="appHostBuilder">The <see cref="MauiAppBuilder"/>.</param>
		/// <returns>The <see cref="MauiAppBuilder"/>.</returns>
		public static MauiAppBuilder RegisterBlazorMauiWebView(this MauiAppBuilder appHostBuilder)
		{
			if (appHostBuilder is null)
			{
				throw new ArgumentNullException(nameof(appHostBuilder));
			}

			appHostBuilder.ConfigureMauiHandlers(static handlers => handlers.AddHandler<IBlazorWebView, BlazorWebViewHandler>());

			return appHostBuilder;
		}
	}
}
