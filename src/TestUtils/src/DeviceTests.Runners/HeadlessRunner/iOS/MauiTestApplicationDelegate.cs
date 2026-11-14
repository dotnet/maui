using System;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

public abstract class MauiTestApplicationDelegate : MauiUIApplicationDelegate
{
	protected MauiTestApplicationDelegate() =>
		Environment.SetEnvironmentVariable("headlessrunner", "true");
}
