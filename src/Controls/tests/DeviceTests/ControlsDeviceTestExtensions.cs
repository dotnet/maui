﻿using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using Xunit;
#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	public static class ControlsDeviceTestExtensions
	{

		public static bool IsAccessibilityElement(this VisualElement view)
		{
			bool result = AssertionExtensions.IsAccessibilityElement(view);

#if IOS
			// UIView.IsAccessibilityElement will never be true if VoiceOver isn't running
			//
			// Because the 'AssertionExtensions.IsAccessibilityElement' just fakes the actual values
			// when VO isn't running we can't really test that calling `SetIsInAccessibleTree` with
			// false works unless we turn on VO
			if (!UIKit.UIAccessibility.IsVoiceOverRunning)
			{
				bool? expected = AutomationProperties.GetIsInAccessibleTree(view);

				if (expected.HasValue && !expected.Value)
				{
					return expected.Value;
				}
			}
#endif

			return result;
		}

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
						handlers.AddHandler(typeof(Editor), typeof(EditorHandler));
						handlers.AddHandler(typeof(VerticalStackLayout), typeof(LayoutHandler));
						handlers.AddHandler(typeof(Controls.Window), typeof(WindowHandlerStub));
						handlers.AddHandler(typeof(Controls.ContentPage), typeof(PageHandler));
						handlers.AddHandler(typeof(MauiAppNewWindowStub), typeof(ApplicationHandler));
					});
		}
	}
}
