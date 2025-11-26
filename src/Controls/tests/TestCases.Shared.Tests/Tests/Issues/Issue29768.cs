#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST //The test fails on Windows and MacCatalyst because the BackgroundApp and ForegroundApp method, which is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29768 : _IssuesUITest
{
	public override string Issue => "Switch OffColor not displayed after minimizing and reopening the app";

	public Issue29768(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Switch)]
	public void VerifySwitchOffColorAfterReopeningApp()
	{
		App.WaitForElement("toggleButton");
		App.Tap("toggleButton");
		App.BackgroundApp();
		App.ForegroundApp();

		VerifyScreenshot();
	}
}
#endif