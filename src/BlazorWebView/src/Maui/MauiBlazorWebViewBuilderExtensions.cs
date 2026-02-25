using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Extension methods for <see cref="IMauiBlazorWebViewBuilder"/>.
	/// </summary>
	public static class MauiBlazorWebViewBuilderExtensions
	{
		/// <summary>
		/// Registers a custom handler for <see cref="IBlazorWebView"/>, replacing the default
		/// <see cref="BlazorWebViewHandler"/> registered by
		/// <see cref="M:Microsoft.Extensions.DependencyInjection.BlazorWebViewServiceCollectionExtensions.AddMauiBlazorWebView(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
		/// This allows custom platform backends to provide their own BlazorWebView handler
		/// while reusing all shared service registrations.
		/// </summary>
		/// <typeparam name="THandler">The custom handler type to use for <see cref="IBlazorWebView"/>.</typeparam>
		/// <param name="builder">The <see cref="IMauiBlazorWebViewBuilder"/>.</param>
		/// <returns>The <see cref="IMauiBlazorWebViewBuilder"/> for chaining.</returns>
		public static IMauiBlazorWebViewBuilder UsePlatformHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
			this IMauiBlazorWebViewBuilder builder)
			where THandler : IElementHandler
		{
			builder.Services.ConfigureMauiHandlers(handlers =>
				handlers.AddHandler<IBlazorWebView, THandler>());
			return builder;
		}
	}
}
