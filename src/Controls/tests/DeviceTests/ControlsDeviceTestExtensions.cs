using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
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
						// TODO: removing these causes a crash and it is very hard to fugure out which test
						//       is needing these mappers

						handlers.AddHandler(typeof(Editor), typeof(EditorHandler));
						handlers.AddHandler(typeof(VerticalStackLayout), typeof(LayoutHandler));
						handlers.AddHandler(typeof(Controls.Window), typeof(WindowHandlerStub));
						handlers.AddHandler(typeof(Controls.ContentPage), typeof(PageHandler));
#if WINDOWS
						handlers.AddHandler(typeof(MauiAppNewWindowStub), typeof(ApplicationHandler));
#endif
					});
		}


		public static Task Wait(this Image image, int timeout = 1000) =>
			AssertionExtensions.Wait(() => !image.IsLoading, timeout);

		public static void SetupShellHandlers(this MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(SetupShellHandlers);
		}

		public static void SetupShellHandlers(this IMauiHandlersCollection handlers)
		{
			handlers.TryAddHandler(typeof(Controls.Shell), typeof(ShellHandler));
			handlers.TryAddHandler<Layout, LayoutHandler>();
			handlers.TryAddHandler<Image, ImageHandler>();
			handlers.TryAddHandler<Label, LabelHandler>();
			handlers.TryAddHandler<Button, ButtonHandler>();
			handlers.TryAddHandler<Page, PageHandler>();
			handlers.TryAddHandler(typeof(Toolbar), typeof(ToolbarHandler));
			handlers.TryAddHandler(typeof(MenuBar), typeof(MenuBarHandler));
			handlers.TryAddHandler(typeof(MenuBarItem), typeof(MenuBarItemHandler));
			handlers.TryAddHandler(typeof(MenuFlyoutItem), typeof(MenuFlyoutItemHandler));
			handlers.TryAddHandler(typeof(MenuFlyoutSubItem), typeof(MenuFlyoutSubItemHandler));
			handlers.TryAddHandler<ScrollView, ScrollViewHandler>();

#if WINDOWS
			handlers.TryAddHandler(typeof(ShellItem), typeof(ShellItemHandler));
			handlers.TryAddHandler(typeof(ShellSection), typeof(ShellSectionHandler));
			handlers.TryAddHandler(typeof(ShellContent), typeof(ShellContentHandler));
#endif
		}
	}
}
