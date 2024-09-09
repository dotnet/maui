using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.DeviceTests
{
	public static class ControlsDeviceTestExtensions
	{
		public static MauiAppBuilder ConfigureTestBuilder(this MauiAppBuilder mauiAppBuilder)
		{
			return
				mauiAppBuilder
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
						handlers.AddHandler(typeof(Editor), typeof(EditorHandler));
						handlers.AddHandler(typeof(VerticalStackLayout), typeof(LayoutHandler));
						handlers.AddHandler(typeof(Controls.ContentPage), typeof(PageHandler));
#if WINDOWS
						handlers.AddHandler(typeof(MauiAppNewWindowStub), typeof(ApplicationHandler));
#endif
					});
		}
	}
}
