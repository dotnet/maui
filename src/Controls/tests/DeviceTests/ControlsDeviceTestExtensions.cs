using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Xunit;
#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	public static class ControlsDeviceTestExtensions
	{
		public static MauiAppBuilder ConfigureTestBuilder(this MauiAppBuilder mauiAppBuilder)
		{
			return
				mauiAppBuilder
					.RemapForControls()
					.ConfigureLifecycleEvents(lifecycle =>
					{
#if IOS || MACCATALYST
						lifecycle
							.AddiOS(iOS => iOS
								.OpenUrl((app, url, options) =>
									ApplicationModel.Platform.OpenUrl(app, url, options))
								.ContinueUserActivity((application, userActivity, completionHandler) =>
									ApplicationModel.Platform.ContinueUserActivity(application, userActivity, completionHandler))
								.PerformActionForShortcutItem((application, shortcutItem, completionHandler) =>
									ApplicationModel.Platform.PerformActionForShortcutItem(application, shortcutItem, completionHandler)));
#elif WINDOWS
						lifecycle
							.AddWindows(windows =>
							{
								windows
									.OnLaunched((app, e) =>
										ApplicationModel.Platform.OnLaunched(e));
								windows
									.OnActivated((window, e) =>
										ApplicationModel.Platform.OnActivated(window, e));
							});
#endif
					})
					.ConfigureMauiHandlers(handlers =>
					{
						handlers.AddHandler<Editor>(_ => new EditorHandler());
						handlers.AddHandler<VerticalStackLayout>(_ => new LayoutHandler());
						handlers.AddHandler<Controls.Window>(_ => new WindowHandlerStub());
						handlers.AddHandler<Controls.ContentPage>(_ => new PageHandler());
#if WINDOWS
						handlers.AddHandler<MauiAppNewWindowStub>(_ => new ApplicationHandler());
#endif
					});
		}
	}
}
