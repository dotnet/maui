#if ANDROID && TEST_FAILS_ON_ANDROID //for more information : https://github.com/dotnet/maui/issues/30411
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue28798 : _IssuesUITest
{
	public Issue28798(TestDevice device) : base(device) { }

	public override string Issue => "Controls Disappear When WebView is Used with Hardware Acceleration Disabled in Android";

	[Test]
	[Category(UITestCategories.WebView)]
	public void ControlsShouldRemainVisibleWithWebViewWhenHardwareAccelerationIsDisabled()
	{
		App.WaitForElement("TestLabel");
		VerifyScreenshot();
	}
}
#endif
