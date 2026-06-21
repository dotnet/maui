#if ANDROID || IOS || MACCATALYST || WINDOWS
using System;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Embedding;
using Microsoft.Maui.Hosting;

#if ANDROID
using PlatformView = Android.Views.View;
using PlatformWindow = Android.App.Activity;
#elif IOS || MACCATALYST
using PlatformView = UIKit.UIView;
using PlatformWindow = UIKit.UIWindow;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
using PlatformWindow = Microsoft.UI.Xaml.Window;
#endif

namespace Microsoft.Maui.Controls.Embedding;

/// <summary>
/// A set of extension methods that allow for embedding a MAUI view within a native application.
/// </summary>
public static class EmbeddingExtensions
{
	/// <summary>
	/// Enables MAUI to be embedded in native platform application by injecting embedded handlers into the service collection.
	/// </summary>
	/// <param name="builder">The <see cref="MauiAppBuilder"/> instance.</param>
	/// <returns>The <see cref="MauiAppBuilder"/> instance.</returns>
	/// <remarks>
	/// This is internal as it is exposed in Controls.Xaml since it needs to setup XAML defaults.
	/// </remarks>
	internal static MauiAppBuilder UseMauiEmbedding(this MauiAppBuilder builder)
	{
#if ANDROID
		var platformApplication = (global::Android.App.Application)global::Android.App.Application.Context;
#elif IOS || MACCATALYST
		var platformApplication = UIKit.UIApplication.SharedApplication.Delegate;
#elif WINDOWS
		var platformApplication = Microsoft.UI.Xaml.Application.Current;
#endif

		// Enable Core embedded features.
		builder.UseMauiEmbedding(platformApplication);

		// Register the embedded window handler.
		builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<EmbeddedWindow, EmbeddedWindowHandler>();
		});

		return builder;
	}

	/// <summary>
	/// Creates a window-scoped <see cref="IMauiContext"/> for the provided native platform window.
	/// </summary>
	/// <param name="mauiApp">The <see cref="MauiApp"/> instance.</param>
	/// <param name="platformWindow">The native platform window instance to create the context for.</param>
	/// <returns>The window-scoped <see cref="IMauiContext"/> instance.</returns>
	/// <remarks>
	/// In addition to the context being created, a new Window instance is created and attached to the app.
	/// </remarks>
	public static IMauiContext CreateEmbeddedWindowContext(this MauiApp mauiApp, PlatformWindow platformWindow)
	{
		var window = new EmbeddedWindow();

		// Create the Core embedded window scope.
		var windowContext = mauiApp.CreateEmbeddedWindowContext(platformWindow, window);

		// If the app is an embedded app then we need to add the window to the app.
		var embeddedApp = mauiApp.Services.GetRequiredService<EmbeddedPlatformApplication>();
		if (embeddedApp.Application is Application app && !app.Windows.Contains(window))
		{
			app.AddWindow(window);
		}

		return windowContext;
	}

	/// <summary>
	/// Similar to <see cref="ElementExtensions.ToPlatform(IElement, IMauiContext)"/>, but also adds the element as
	/// a logical child to the embedded window.
	/// </summary>
	/// <param name="element">The element to use when creating the native platform view.</param>
	/// <param name="context">The context to use when creating the native platform view.</param>
	/// <returns>The native platform view that represents the element.</returns>
	/// <remarks>
	/// Only if the window is an embedded window and the element is a <see cref="VisualElement"/> will the element
	/// be added as a logical child of that window.
	/// </remarks>
	public static PlatformView ToPlatformEmbedded(this IElement element, IMauiContext context)
	{
		// If the window is an embedded window, then we need to add the element as a logical child.
		var wndProvider = context.Services.GetService<EmbeddedWindowProvider>();
		if (wndProvider is not null && wndProvider.Window is EmbeddedWindow wnd && element is VisualElement visual)
			wnd.AddLogicalChild(visual);

		return element.ToPlatform(context);
	}

	/// <summary>
	/// Similar to <see cref="ElementExtensions.ToPlatform(IElement, IMauiContext)"/>, but also adds the element as
	/// a logical child to a new embedded window.
	/// </summary>
	/// <param name="element">The element to use when creating the native platform view.</param>
	/// <param name="mauiApp">The <see cref="MauiApp"/> instance.</param>
	/// <param name="platformWindow">The native platform window that will host this element.</param>
	/// <returns>The native platform view that represents the element.</returns>
	/// <remarks>
	/// Only if the window is an embedded window and the element is a <see cref="VisualElement"/> will the element
	/// be added as a logical child of that window.
	/// </remarks>
	public static PlatformView ToPlatformEmbedded(this IElement element, MauiApp mauiApp, PlatformWindow platformWindow)
	{
		var windowContext = mauiApp.CreateEmbeddedWindowContext(platformWindow);
		return element.ToPlatformEmbedded(windowContext);
	}
}

#endif
