#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35125 : _IssuesUITest
{
	public Issue35125(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell top-tab unselected text should remain visible in Material 3 light theme";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TopTabUnselectedTextVisibleWhenSwitchingTabs()
	{
		App.WaitForElement("TAB TWO");
		App.Tap("TAB TWO");
		App.WaitForElement("Issue35125PageTwoLabel");

		VerifyScreenshot();
	}
}
#endif
