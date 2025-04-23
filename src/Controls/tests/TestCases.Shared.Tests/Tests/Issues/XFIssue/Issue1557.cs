using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1557 : _IssuesUITest
{
	const int Delay = 3000;

	public Issue1557(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Setting source crashes if view was detached from visual tree";

	[Test]
	[Category(UITestCategories.ListView)]
	public void SettingSourceWhenDetachedDoesNotCrash()
	{
		App.WaitForElement("Bug Repro");
		App.Tap("Bug Repro");
		App.WaitForElement("foo");
		App.WaitForElement("bar");
		App.WaitForElementTillPageNavigationSettled("Bug Repro");
	}
}