using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Devices;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public static class Extensions
	{
		public static Task WaitUntilLoaded(this Image image, int timeout = 1000) =>
			AssertEventually(() => !image.IsLoading, timeout, message: $"Timed out loading image {image}");

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

