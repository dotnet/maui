using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla30324 : _IssuesUITest
{
	public Bugzilla30324(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Detail view of FlyoutPage does not get appearance events on Android when whole FlyoutPage disappears/reappears";

	// [Test]
	// [Category(UITestCategories.FlyoutPage)]
	// public void Bugzilla30324Test ()
	// {
	// 	App.WaitForElement("navigate");

	// 	App.Tap("navigate");
	// 	App.Tap("navigateback");
	// 	App.WaitForElement("Disappeardetail");
	// 	App.Tap("navigate");
	// 	App.Tap("navigateback");
	// 	App.WaitForElement("Appeardetail");
	// }
}