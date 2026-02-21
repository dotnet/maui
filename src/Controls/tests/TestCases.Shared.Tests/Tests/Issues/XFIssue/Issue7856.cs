using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7856 : _IssuesUITest
{
	public Issue7856(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Shell BackButtonBehaviour TextOverride breaks back";

	[Test]
	[Category(UITestCategories.Shell)]
	public void BackButtonBehaviorTest()
	{
		App.WaitForElementTillPageNavigationSettled("Tap to Navigate To the Page With BackButtonBehavior");
		App.Tap("Tap to Navigate To the Page With BackButtonBehavior");
		App.WaitForElement("Navigate again");
		App.Tap("Navigate again");
		App.WaitForElementTillPageNavigationSettled("Hello");
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			App.TapBackArrow(); // In iOS 26, the previous page title is not shown along with the back arrow, so we use the default back arrow
		}
		else
		{
			App.TapBackArrow("Test");
		}
	}
}