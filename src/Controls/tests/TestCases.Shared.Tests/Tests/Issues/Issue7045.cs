using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7045 : _IssuesUITest
{
	public Issue7045(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell BackButtonBehavior binding Command to valid ICommand causes back button to disappear";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellBackButtonBehaviorShouldWork()
	{
		App.WaitForElement("NavigateButton");
		App.Click("NavigateButton");
		App.WaitForElement("DetailPageLabel");
		if (App is AppiumCatalystApp || App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			App.TapBackArrow();
		else
			App.TapBackArrow("Back");
		Assert.That(() => App.WaitForElement("NavigateButton").GetText(),
			Is.EqualTo("Back command executed").After(10000, 200));
		App.WaitForNoElement("DetailPageLabel");
	}
}
