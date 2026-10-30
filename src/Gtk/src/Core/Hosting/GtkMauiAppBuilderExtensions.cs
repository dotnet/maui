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
		/// <para><b>Ordering rule:</b> call it immediately after <c>UseMauiApp&lt;T&gt;()</c> and before any <c>AppendToMapping</c> /
		/// <c>PrependToMapping</c> customisation. The backend patches the stock static property mappers in place with a GTK branch;
		/// customisations added afterwards then wrap the GTK implementation, while customisations added before are shadowed
		/// on GTK. Handler registrations are rewritten when the app is built, so registrations made later by
		/// <c>ConfigureMauiHandlers</c> are also covered.</para>
		/// <para>Apps that use Controls call <c>UseMauiGtk()</c> from <c>Microsoft.Maui.Controls.Gtk</c> instead, which calls this
		/// method and adds the Controls and Essentials registrations.</para>
		/// </remarks>
		/// <param name="builder">The app builder.</param>
		public static MauiAppBuilder UseMauiGtkCore(this MauiAppBuilder builder)
		{
			if (builder is null)
				throw new ArgumentNullException(nameof(builder));

			GtkRuntime.EnsureInitialized();
			GtkMapperRegistration.Apply();
			GtkPlatformViewBackend.Install();

			var services = builder.Services;

			// Dispatching and animation.
			var dispatcherProvider = new GtkDispatcherProvider();
			DispatcherProvider.SetCurrent(dispatcherProvider);
			services.Replace(ServiceDescriptor.Singleton<IDispatcherProvider>(dispatcherProvider));
			services.Replace(ServiceDescriptor.Scoped<ITicker>(_ => new GtkTicker()));

			// Fonts.
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

			// Handlers: runs when the handler collection is built, after every registration that precedes it.
			builder.ConfigureMauiHandlers(GtkHandlerMap.Apply);

			return builder;
		}
	}
}
