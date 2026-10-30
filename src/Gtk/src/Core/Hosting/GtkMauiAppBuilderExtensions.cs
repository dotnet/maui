using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Gtk.Dispatching;
using Microsoft.Maui.Gtk.Fonts;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Gtk.Platform;

namespace Microsoft.Maui.Hosting
{
	/// <summary>
	/// Registers the GTK 4 backend of .NET MAUI.
	/// </summary>
	public static class GtkMauiAppBuilderExtensions
	{
		/// <summary>
		/// Registers the core GTK backend: the platform-view backend hook, the dispatcher and ticker, fonts, image source
		/// services and the GTK handlers (by rewriting the registrations of the stock handlers).
		/// </summary>
		/// <remarks>
		/// <para><b>Mappers.</b> Each GTK handler carries its own property mapper in front of the stock one (see <see cref="GtkMapperBuilder{TVirtualView, THandler}"/>):
		/// the GTK implementation runs first, then the stock mapping of the same key, so Controls' remaps and
		/// <c>AppendToMapping</c>/<c>PrependToMapping</c> customisations of the stock mappers still run, in whatever order they were registered, but always after the GTK implementation.
		/// There is no ordering rule between this call and <c>UseMauiApp&lt;T&gt;()</c> or <c>ConfigureMauiHandlers</c>: the handler collection is
		/// wrapped so the rewrite (<see cref="GtkHandlerMap.Apply"/>) runs after every registration, whenever it was made.</para>
		/// <para>Apps that use Controls call <c>UseMauiGtk()</c> from <c>Microsoft.Maui.Controls.Gtk</c> instead, which calls this
		/// method and adds the Controls and Essentials registrations.</para>
		/// </remarks>
		/// <param name="builder">The app builder.</param>
		public static MauiAppBuilder UseMauiGtkCore(this MauiAppBuilder builder)
		{
			if (builder is null)
				throw new ArgumentNullException(nameof(builder));

			GtkRuntime.EnsureInitialized();
			GtkPlatformViewBackend.Install();

			var services = builder.Services;

			// Dispatching and animation.
			var dispatcherProvider = new GtkDispatcherProvider();
			DispatcherProvider.SetCurrent(dispatcherProvider);
			services.Replace(ServiceDescriptor.Singleton<IDispatcherProvider>(dispatcherProvider));
			services.Replace(ServiceDescriptor.Scoped<ITicker>(_ => new GtkTicker()));

			// Fonts. Fonts registered by file name are read from the folders next to the app.
			FontRegistrar.NativeFontStreamProvider = GtkEmbeddedFontLoader.OpenNativeFont;
			services.Replace(ServiceDescriptor.Singleton<IEmbeddedFontLoader>(_ => new GtkEmbeddedFontLoader()));
			services.Replace(ServiceDescriptor.Singleton<IFontManager>(sp => new GtkFontManager(sp.GetRequiredService<IFontRegistrar>(), sp)));

			// Image sources (registered after the stock ones, so they win).
			builder.ConfigureImageSources(sources =>
			{
				sources.AddService<IFileImageSource>(sp => new GtkFileImageSourceService(sp.GetService<ILogger<GtkFileImageSourceService>>()));
				sources.AddService<IStreamImageSource>(sp => new GtkStreamImageSourceService(sp.GetService<ILogger<GtkStreamImageSourceService>>()));
				sources.AddService<IUriImageSource>(sp => new GtkUriImageSourceService(sp.GetService<ILogger<GtkUriImageSourceService>>()));
				sources.AddService<IFontImageSource>(sp => new GtkFontImageSourceService(sp.GetService<IFontManager>(), sp.GetService<ILogger<GtkFontImageSourceService>>()));
			});

			// Handlers: ConfigureMauiHandlers runs the registrations in call order, so a callback could only rewrite those before it. Make sure the
			// stock collection/factory registrations exist (TryAdd), then replace the collection with one that applies the map once all registrations ran.
			builder.ConfigureMauiHandlers(null);
			services.Replace(ServiceDescriptor.Singleton<IMauiHandlersCollection>(sp =>
			{
				var handlers = new HandlerMauiAppBuilderExtensions.HandlerServiceBuilder(sp.GetServices<HandlerMauiAppBuilderExtensions.HandlerRegistration>());
				GtkHandlerMap.Apply(handlers);
				return handlers;
			}));

			return builder;
		}
	}
}
