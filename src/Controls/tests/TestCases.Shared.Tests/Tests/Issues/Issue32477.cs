
#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/32416
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32477 : _IssuesUITest
{
	public Issue32477(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "[Android] Shell flyout does not disable scrolling when FlyoutVerticalScrollMode is set to Disabled";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutVerticalScrollModeDisabledOnAndroid()
	{
		App.WaitForElement("Item 1");

		for (int i = 0; i < 5; i++)
		{
			App.ScrollDown("Item 5", ScrollStrategy.Gesture);
		}

		App.WaitForElement("Item 1");
	}
}
#endif
