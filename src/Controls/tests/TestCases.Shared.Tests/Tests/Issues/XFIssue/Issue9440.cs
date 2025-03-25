#if TEST_FAILS_ON_WINDOWS //DoubleTap not performing by appium, using DoubleClick also not working for windows getting "OpenQA.Selenium.WebDriverException : Currently only pen and touch pointer input source types are supported" exception. But in manual testing more tap closes the flyout. 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9440 : _IssuesUITest
{
	public Issue9440(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Flyout closes with two or more taps";

	[Test]
	[Category(UITestCategories.Shell)]
	public void GitHubIssue9440()
	{
		App.TapShellFlyoutIcon();
		App.WaitForElement("Test 1");
#if ANDROID // In Android Double Tap does not performs in Appium, which does not closes the flyout, but in manual testing it closes. So, we are using DoubleClick instead of DoubleTap as make this test run.
		App.DoubleClick("Test 1");
#else
		App.DoubleTap("Test 1");
#endif

		App.WaitForElement("Test 1");
		App.WaitForElement("False");
	}
}
#endif