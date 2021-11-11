#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public static class TestDispatcher
	{
		static IDispatcher? s_dispatcher;

		public static IDispatcher Current
		{
			get
			{
				if (s_dispatcher is null)
				{
					IServiceProvider? services = null;

#if __ANDROID__
					services = MauiTestInstrumentation.Current?.Services ?? MauiApplication.Current.Services;
#elif __IOS__
					services = MauiTestApplicationDelegate.Current?.Services ?? MauiUIApplicationDelegate.Current.Services;
#elif WINDOWS
					services = MauiWinUIApplication.Current.Services;
#endif

					s_dispatcher = services?.GetService<IDispatcher>();
				}

				if (s_dispatcher is null)
					throw new InvalidOperationException($"Test app did not provide a dispatcher.");

				return s_dispatcher;
			}
		}
	}
}