#if ANDROID || IOS || MACCATALYST || WINDOWS
using System;
using Microsoft.Extensions.DependencyInjection;

#if ANDROID
using PlatformApplication = Android.App.Application;
#elif IOS || MACCATALYST
using PlatformApplication = UIKit.IUIApplicationDelegate;
#elif WINDOWS
using PlatformApplication = Microsoft.UI.Xaml.Application;
#endif

namespace Microsoft.Maui.Embedding;

/// <summary>
/// A <see cref="IPlatformApplication"/> that is used in embedding scenarios where the native platform application
/// instance is either unknown at the time, or not controlled by MAUI.
/// </summary>
internal class EmbeddedPlatformApplication : IPlatformApplication
{
	private readonly MauiContext _rootContext;

	public EmbeddedPlatformApplication(IServiceProvider services)
	{
		IPlatformApplication.Current = this;

		PlatformApplication = services.GetRequiredService<PlatformApplication>();

#if ANDROID
		_rootContext = new MauiContext(services, PlatformApplication);
#else
		_rootContext = new MauiContext(services);
#endif

		Context = _rootContext.MakeApplicationScope(PlatformApplication);

		Services = Context.Services;

		Application = Services.GetRequiredService<IApplication>();

		PlatformApplication.SetApplicationHandler(Application, Context);
	}

	/// <summary>
	/// Gets the native platform application instance that is hosting this MAUI app.
	/// </summary>
	public PlatformApplication PlatformApplication { get; }

	/// <summary>
	/// Gets the application-scoped <see cref="IMauiContext"/>.
	/// </summary>
	public IMauiContext Context { get; }

	/// Gets the application-scoped services.
	public IServiceProvider Services { get; }

	/// Gets the current MAUI application instance.
	public IApplication Application { get; }
}
#endif
