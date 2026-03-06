#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Ignore file for Catalyst and Windows platforms. This test is applicable for mobile platforms only.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32275_Template : _IssuesUITest
{
	public Issue32275_Template(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "SafeAreaEdges should be handled by the FlyoutContentTemplate";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void ShellFlyoutContentTemplateShouldRespectSafeArea()
	{
		App.WaitForElement("Issue32275_Template_Label");
		App.TapShellFlyoutIcon();
		App.SetOrientationLandscape();
		Thread.Sleep(1000); // Allow time for orientation change to take effect and UI to stabilize.
#if ANDROID
		VerifyScreenshot(cropLeft: 125);
#else
		VerifyScreenshot();
#endif
	}
}
#endif