using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla32902 : _IssuesUITest
{
	public Bugzilla32902(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS | iPad] App Crashes (without debug log) when Flyout Detail isPresented and navigation being popped";
	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void Bugzilla32902Test()
	{
		App.WaitForElement("btnNext");
		App.Tap("btnNext");
		App.WaitForElementTillPageNavigationSettled("btnPushModal");
		App.Tap("btnPushModal");
		App.WaitForElementTillPageNavigationSettled("PopModal");
		App.TapFlyoutPageIcon("Flyout");
		App.WaitForElementTillPageNavigationSettled("btnPop");
		App.Tap("btnPop");

	}
}
