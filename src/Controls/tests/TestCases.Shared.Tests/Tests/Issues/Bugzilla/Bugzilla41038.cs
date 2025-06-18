using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla41038 : _IssuesUITest
{
	public Bugzilla41038(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "FlyoutPage loses menu icon on iOS after reusing NavigationPage as Detail";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void Bugzilla41038Test()
	{
		App.WaitForElement("ViewA");

		App.TapFlyoutPageIcon("Flyout");

		App.WaitForElement("ViewB");
		App.Tap("ViewB");

#if ANDROID || WINDOWS // On Android and Windows, the hamburger icon was displayed, while on iOS and Catalyst, the title text was shown.
		App.TapFlyoutPageIcon("Flyout");
#else
		App.WaitForElementTillPageNavigationSettled("Flyout");
#endif
	}
}