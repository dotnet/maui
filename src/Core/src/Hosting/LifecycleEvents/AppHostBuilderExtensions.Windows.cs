using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.LifecycleEvents
{
	public static partial class AppHostBuilderExtensions
	{
		internal static MauiAppBuilder ConfigureCrossPlatformLifecycleEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddWindows(OnConfigureLifeCycle));

		internal static MauiAppBuilder ConfigureWindowEvents(this MauiAppBuilder builder) =>
			builder.ConfigureLifecycleEvents(events => events.AddWindows(OnConfigureWindow));

		static void OnConfigureLifeCycle(IWindowsLifecycleBuilder windows)
		{
			windows
				.OnWindowCreated(window =>
				{
					window.GetWindow()?.Created();
				})
				.OnResumed(window =>
				{
					window.GetWindow()?.Resumed();
				})
				.OnActivated((window, args) =>
				{
					switch (args.WindowActivationState)
					{
						case UI.Xaml.WindowActivationState.CodeActivated:
						case UI.Xaml.WindowActivationState.PointerActivated:
							window.GetWindow()?.Activated();
							break;
						case UI.Xaml.WindowActivationState.Deactivated:
							window.GetWindow()?.Deactivated();
							break;
					}
				})
				.OnVisibilityChanged((window, args) =>
				{
					if (!args.Visible)
						window.GetWindow()?.Stopped();
				})
				.OnClosed((window, args) =>
				{
					window.GetWindow()?.Destroying();
				});
		}

		static void OnConfigureWindow(IWindowsLifecycleBuilder windows)
		{
			windows
				.OnPlatformMessage((window, e) =>
				{
					if (e.MessageId == PlatformMethods.MessageIds.WM_SETTINGCHANGE ||
						e.MessageId == PlatformMethods.MessageIds.WM_THEMECHANGE)
					{
						if (IPlatformApplication.Current is IPlatformApplication platformApplication)
						{
							platformApplication.Application?.ThemeChanged();
						}
					}
					else if (e.MessageId == PlatformMethods.MessageIds.WM_DPICHANGED)
					{
						var win = window.GetWindow();
						if (win is not null)
						{
							var dpiX = (short)(long)e.WParam;
							var dpiY = (short)((long)e.WParam >> 16);

							var density = dpiX / DeviceDisplay.BaseLogicalDpi;

							win.DisplayDensityChanged(density);
						}
					}
				});
		}
	}
}
