using System;
#if WEBVIEW2_WINFORMS
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
#elif WEBVIEW2_WPF
using Microsoft.AspNetCore.Components.WebView.Wpf;
#elif WEBVIEW2_MAUI
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui.Hosting;
#else
#error Must define WEBVIEW2_WINFORMS, WEBVIEW2_WPF, WEBVIEW2_MAUI
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extension methods to <see cref="IServiceCollection"/>.
	/// </summary>
	public static class BlazorWebViewServiceCollectionExtensions
	{
		/// <summary>
		/// Configures <see cref="IServiceCollection"/> to add support for <see cref="BlazorWebView"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <param name="settings">The <see cref="BlazorWebViewSettings"/> for the underlying WebView control.</param>
		/// <returns>The <see cref="IServiceCollection"/>.</returns>
#if WEBVIEW2_WINFORMS
		public static IServiceCollection AddWindowsFormsBlazorWebView(this IServiceCollection services, BlazorWebViewSettings settings)
#elif WEBVIEW2_WPF
		public static IServiceCollection AddWpfBlazorWebView(this IServiceCollection services, BlazorWebViewSettings settings)
#elif WEBVIEW2_MAUI
		public static IServiceCollection AddMauiBlazorWebView(this IServiceCollection services, BlazorWebViewSettings settings)
#else
#error Must define WEBVIEW2_WINFORMS, WEBVIEW2_WPF, WEBVIEW2_MAUI
#endif
		{
			services.AddBlazorWebView();
			services.TryAddSingleton(settings);

#if WEBVIEW2_WINFORMS
#elif WEBVIEW2_WPF
#elif WEBVIEW2_MAUI
			services.ConfigureMauiHandlers(static handlers => handlers.AddHandler<IBlazorWebView, BlazorWebViewHandler>());
#else
#error Must define WEBVIEW2_WINFORMS, WEBVIEW2_WPF, WEBVIEW2_MAUI
#endif
			return services;
		}
	}
}
