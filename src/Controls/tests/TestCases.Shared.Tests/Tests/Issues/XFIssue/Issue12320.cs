using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12320 : _IssuesUITest
{
	public Issue12320(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] TabBarIsVisible = True/False doesn't work on Back Navigation When using BackButtonBehavior";

	[Test]
	[Category(UITestCategories.Shell)]
	public void PopLogicExecutesWhenUsingBackButtonBehavior()
	{
		App.WaitForElementTillPageNavigationSettled("TestReady");
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			App.Back(); // In iOS 26, the AutomationID set on the Back button is not found by Appium, so the Back method is used for iOS 26.
		}
		else
		{
			App.TapBackArrow("BackButtonImage");
		}
		App.WaitForElement("Tab 1");
	}
}
