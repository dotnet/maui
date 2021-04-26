using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.Controls
{
	public class RedButton : Button
	{
		static RedButton()
		{
			// Accessing the app services via AppHost.Current is only meant
			// for library authors. Using this in unit tests will result in
			// undefined behavior as a result of being a static field set
			// by AppHostBuilder.Build().
			//
			var handlerProvider = AppHost.Current?.Handlers;

			//// A better way to access the current app and services is to
			//// use the single running native app.
			//
			//#if __ANDROID__
			//var nativeApp = MauiApplication.Current;
			//#elif __IOS__
			//var nativeApp = MauiUIApplicationDelegate.Current;
			//#elif WINDOWS
			//var nativeApp = MauiWinUIApplication.Current;
			//#endif
			//
			//var handlerProvider = nativeApp.Services.GetRequiredService<IMauiHandlersServiceProvider>();

			// However, neither way is really good because accessing the
			// services/handlers befire the AppHostBuilder has run will
			// result in there being no handler provider and thus either
			// the cctor will throw if it is not anticipated or the
			// registration will not happen.
			//
			if (handlerProvider != null)
			{
				var handlers = handlerProvider.GetCollection();

				handlers.AddHandler<RedButton, RedButtonHandler>();
			}

			// A better way would be to make use of the builder pattern
			// and actually allow the user to opt in to your handlers.
			// This is also important because order of referencing also
			// matters.
			//
			// For example, if user creates a custom handler for
			// Button and then registers it in the builder but then
			// proceeds to use the Button, then the user's specific
			// registration is now overwritten.
			// There is the option for TryAddHandler, but now the first
			// one wins. This could happen if the user references one
			// of the controls in the builder - maybe because it has
			// some static information that is needed.
		}
	}
}