﻿using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public static class MauiProgram
	{
#if ANDROID
		public static global::Android.Content.Context DefaultContext => MauiProgramDefaults.DefaultContext;
#elif WINDOWS
		public static UI.Xaml.Window DefaultWindow => MauiProgramDefaults.DefaultWindow;
#endif

		public static IApplication DefaultTestApp { get; private set; }

		public static MauiApp CreateMauiApp() =>
			MauiProgramDefaults.CreateMauiApp(new List<Assembly>()
			{
				typeof(MauiProgram).Assembly
			});
	}
}