using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public static class MauiProgram
	{
		static MauiProgram()
		{
			AppContext.SetSwitch("HybridWebView.InvokeJavaScriptThrowsExceptions", isEnabled: true);
		}

#if ANDROID
		public static Android.Content.Context CurrentContext => MauiProgramDefaults.DefaultContext;
#elif WINDOWS
		public static Microsoft.UI.Xaml.Window CurrentWindow => MauiProgramDefaults.DefaultWindow;
#endif

		public static IApplication DefaultTestApp => MauiProgramDefaults.DefaultTestApp;

		public static MauiApp CreateMauiApp() =>
			MauiProgramDefaults.CreateMauiApp((sp) =>
			{
				var options = new TestOptions
				{
					Assemblies = new List<Assembly>()
					{
						typeof(MauiProgram).Assembly
					},
					SkipCategories = typeof(TestCategory).GetExcludedTestCategories()
				};

				return options;

			});
	}
}