#nullable enable
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIWindow;
#elif MONOANDROID
using PlatformView = Android.App.Activity;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Window;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public static class TestWindow
	{
		static PlatformView? s_platformWindow;

		public static PlatformView PlatformWindow
		{
			get
			{
				if (s_platformWindow is null)
				{
#if __ANDROID__
					s_platformWindow = MauiTestInstrumentation.Current?.CurrentExecutionContext as PlatformView;
#elif __IOS__
					s_platformWindow = MauiTestApplicationDelegate.Current?.Window;
#endif
				}

				if (s_platformWindow is null)
				{
					var application = TestServices.Services.GetService<IApplication>();
					s_platformWindow = application?.Windows.FirstOrDefault()?.Handler?.PlatformView as PlatformView;
				}

				if (s_platformWindow is null)
					throw new InvalidOperationException($"Test app did not provide a window.");

				return s_platformWindow;
			}
		}
	}
}