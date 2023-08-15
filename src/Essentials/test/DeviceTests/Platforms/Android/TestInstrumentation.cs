using System;
using Android.App;
using Android.Runtime;
using DeviceRunners.XHarness;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Essentials.DeviceTests;

[Obsolete("Use the runner in the DeviceRunners.XHarness.Maui package instead.")]
[Instrumentation(Name = "com.microsoft.maui.essentials.devicetests.TestInstrumentation")]
public class TestInstrumentation : XHarnessInstrumentationBase
{
	protected TestInstrumentation(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected override HomeViewModel GetHomeViewModel(global::Android.App.Application app)
	{
		if (app is not MauiApplication mauiApp)
			throw new InvalidOperationException("The .NET MAUI instrumentation implementation only supports MauiApplication.");

		var services = mauiApp.Services;
		return services.GetRequiredService<HomeViewModel>();
	}
}
