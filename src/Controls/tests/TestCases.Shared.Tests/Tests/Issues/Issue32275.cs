#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Ignore file for Catalyst and Windows platforms. This test is applicable for mobile platforms only.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32275 : _IssuesUITest
{
	public Issue32275(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[NET10] SafeAreaEdges cannot be set for Shell and the flyout menu collides with display notch and status bar in landscape mode";

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void ShellFlyoutShouldRespectSafeArea()
	{
		App.WaitForElement("Issue32275Label");
		App.SetOrientationLandscape();
		App.TapFlyoutPageIcon();
		VerifyScreenshot();
	}
}
#endif