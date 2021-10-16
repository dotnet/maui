using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.DeviceTests
{
	public static class TestDispatcher
	{
		public static IDispatcher Current
		{
			get
			{
#if WINDOWS
				var app = MauiWinUIApplication.Current;
#elif __IOS__
				var app = MauiUIApplicationDelegate.Current;
#elif __ANDROID__
				var app = MauiApplication.Current;
#else
				// a dummy app because this will never run on a non-platform
				var app = new
				{
					Services = (IServiceProvider)null!
				};
#endif

				return app.Services.GetRequiredService<IDispatcher>();
			}
		}
	}
}