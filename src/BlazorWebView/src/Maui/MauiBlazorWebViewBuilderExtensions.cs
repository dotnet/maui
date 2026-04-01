using System;
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
		/// <typeparam name="THandler">The custom handler type to use for <see cref="IBlazorWebView"/>.
		/// Must have a public parameterless constructor.</typeparam>
		/// <param name="builder">The <see cref="IMauiBlazorWebViewBuilder"/>.</param>
		/// <returns>The <see cref="IMauiBlazorWebViewBuilder"/> for chaining.</returns>
		public static IMauiBlazorWebViewBuilder UsePlatformHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
			this IMauiBlazorWebViewBuilder builder)
			where THandler : IViewHandler, new()
		{
			ArgumentNullException.ThrowIfNull(builder);
			builder.Services.ConfigureMauiHandlers(handlers =>
				handlers.AddHandler<IBlazorWebView, THandler>());
			return builder;
		}

		/// <summary>
		/// Registers a custom handler for <see cref="IBlazorWebView"/> using a factory method,
		/// replacing the default <see cref="BlazorWebViewHandler"/> registered by
		/// <see cref="M:Microsoft.Extensions.DependencyInjection.BlazorWebViewServiceCollectionExtensions.AddMauiBlazorWebView(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
		/// This overload supports handlers that require dependency injection or lack a public
		/// parameterless constructor.
		/// </summary>
		/// <param name="builder">The <see cref="IMauiBlazorWebViewBuilder"/>.</param>
		/// <param name="factory">A factory function that creates the handler instance using the <see cref="IServiceProvider"/>.</param>
		/// <returns>The <see cref="IMauiBlazorWebViewBuilder"/> for chaining.</returns>
		public static IMauiBlazorWebViewBuilder UsePlatformHandler(
			this IMauiBlazorWebViewBuilder builder,
			Func<IServiceProvider, IViewHandler> factory)
		{
			ArgumentNullException.ThrowIfNull(builder);
			ArgumentNullException.ThrowIfNull(factory);
			builder.Services.ConfigureMauiHandlers(handlers =>
				handlers.AddHandler<IBlazorWebView>(factory));
			return builder;
		}
	}
}
