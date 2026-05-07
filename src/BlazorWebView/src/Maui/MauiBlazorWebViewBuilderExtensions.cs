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
		/// <remarks>
		/// Replacement is "last-registration-wins" through the underlying MAUI handler collection.
		/// Call this method <em>after</em> <c>AddMauiBlazorWebView()</c> so the custom handler
		/// overrides the default registration. If a downstream library calls
		/// <c>AddMauiBlazorWebView()</c> again later in the pipeline, that subsequent default
		/// registration will silently re-override this custom handler — call this method last,
		/// after every other library's MAUI Blazor configuration, when composing multiple sources.
		/// </remarks>
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
		/// Use this overload for handlers that lack a public parameterless constructor or that
		/// need to pull dependencies from the MAUI handler service container at construction time.
		/// </summary>
		/// <remarks>
		/// The <see cref="IServiceProvider"/> passed to <paramref name="factory"/> is the MAUI
		/// handler factory's service provider, not the application's root <see cref="IServiceProvider"/>.
		/// It can resolve services that were registered on the handler collection (via
		/// <c>ConfigureMauiHandlers</c>); it cannot resolve arbitrary services from the app's
		/// <see cref="IServiceCollection"/>. The same call-ordering rule as
		/// <see cref="UsePlatformHandler{THandler}(IMauiBlazorWebViewBuilder)"/> applies — call this
		/// method after <c>AddMauiBlazorWebView()</c> (and after any later re-invocations from
		/// downstream libraries) so the custom handler is the last registration to win.
		/// </remarks>
		/// <param name="builder">The <see cref="IMauiBlazorWebViewBuilder"/>.</param>
		/// <param name="factory">A factory function that creates the handler instance.
		/// The <see cref="IServiceProvider"/> argument is the MAUI handler factory's service provider
		/// (see remarks).</param>
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
