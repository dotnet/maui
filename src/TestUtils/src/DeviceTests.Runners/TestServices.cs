#nullable enable
using System;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public static class TestServices
	{
		static IServiceProvider? s_services = null;

		public static IServiceProvider Services
		{
			get
			{
				if (s_services is null)
				{
#if __ANDROID__
					s_services = MauiTestInstrumentation.Current?.Services ?? IPlatformApplication.Current?.Services;
#elif __IOS__
					s_services = MauiTestApplicationDelegate.Current?.Services ?? IPlatformApplication.Current?.Services;
#elif WINDOWS
					s_services = IPlatformApplication.Current?.Services;
#endif
				}

				if (s_services is null)
					throw new InvalidOperationException($"Test app could not find services.");

				return s_services;
			}
		}
	}
}