#if ANDROID || IOS || MACCATALYST || WINDOWS
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

#if ANDROID
using PlatformWindow = Android.App.Activity;
using PlatformApplication = Android.App.Application;
#elif IOS || MACCATALYST
using PlatformWindow = UIKit.UIWindow;
using PlatformApplication = UIKit.IUIApplicationDelegate;
#elif WINDOWS
using PlatformWindow = Microsoft.UI.Xaml.Window;
using PlatformApplication = Microsoft.UI.Xaml.Application;
#endif

namespace Microsoft.Maui.Embedding;

/// <summary>
/// A set of extension methods that allow for embedding a MAUI view within a native application.
/// </summary>
internal static class EmbeddingExtensions
{
	/// <summary>
	/// Enables MAUI to be embedded in native application by injecting embedded handlers into the service collection.
	/// </summary>
	/// <param name="builder">The <see cref="MauiAppBuilder"/> instance.</param>
	/// <param name="platformApplication">The native application instance.</param>
	/// <returns>The <see cref="MauiAppBuilder"/> instance.</returns>
	internal static MauiAppBuilder UseMauiEmbedding(this MauiAppBuilder builder, PlatformApplication? platformApplication = null)
	{
#if ANDROID
		platformApplication ??= (global::Android.App.Application)global::Android.App.Application.Context;
#elif IOS || MACCATALYST
		platformApplication ??= UIKit.UIApplication.SharedApplication.Delegate;
#elif WINDOWS
		platformApplication ??= Microsoft.UI.Xaml.Application.Current;
#endif

		if (platformApplication is null)
		{
			throw new InvalidOperationException("Platform application instance is required and was not able to be detected.");
		}

		// Register the current native application that is currently running.
		builder.Services.AddSingleton(platformApplication);

		// Register the IPlatformApplication for the embedded application.
		builder.Services.AddSingleton<EmbeddedPlatformApplication>(svc => new EmbeddedPlatformApplication(svc));

		builder.Services.AddScoped<EmbeddedWindowProvider>(svc => new EmbeddedWindowProvider());

		// Returning null is acceptable here as the platform window is optional.
		// However, we do not know for sure until we resolve it.
		builder.Services.AddScoped<PlatformWindow>(svc => svc.GetRequiredService<EmbeddedWindowProvider>().PlatformWindow!);

		// Register an initializer as we need a platform application to be
		// instantiated as soon as possible. In a typical app, this would be
		// done by the platform-specific bootstrapper.
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMauiInitializeService, EmbeddedInitializeService>(svc => new EmbeddedInitializeService()));

		return builder;
	}

	/// <summary>
	/// Creates a window-scoped <see cref="IMauiContext"/> for the provided platform window.
	/// </summary>
	/// <param name="mauiApp">The <see cref="MauiApp"/> instance.</param>
	/// <param name="platformWindow">The native window instance to create the context for.</param>
	/// <param name="window">The MAUI window instance to connect to the platform window.</param>
	/// <returns>The window-scoped <see cref="IMauiContext"/> instance.</returns>
	internal static IMauiContext CreateEmbeddedWindowContext(this MauiApp mauiApp, PlatformWindow platformWindow, IWindow window)
	{
		// Get the embedded application instance.
		var embeddedApp = mauiApp.Services.GetRequiredService<EmbeddedPlatformApplication>();

		// Create a window context for the platform window that was provided.
		var windowContext = embeddedApp.Context.MakeWindowScope(platformWindow, out var windowScope);

		// Add the platform window to the service provider.
		var wndProvider = windowContext.Services.GetRequiredService<EmbeddedWindowProvider>();
		wndProvider.SetWindow(platformWindow, window);

		// Connect the platform window to the MAUI window in order for lifecycle events to work.
		window.ToHandler(windowContext);

		return windowContext;
	}

	/// <summary>
	/// An initializer to make sure the <see cref="IPlatformApplication.Current"/> is populated.
	/// </summary>
	private class EmbeddedInitializeService : IMauiInitializeService
	{
		public void Initialize(IServiceProvider services) =>
			services.GetRequiredService<EmbeddedPlatformApplication>();
	}
}
#endif
