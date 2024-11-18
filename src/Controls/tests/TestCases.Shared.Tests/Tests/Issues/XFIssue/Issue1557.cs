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

	public override string Issue => "Setting source crashes if view was detached from visual tre";

	// Maybe this one just works? Not sure where "Bug Repro's" should come from
	// [Test]
	// [Category(UITestCategories.ListView)]
	// public void SettingSourceWhenDetachedDoesNotCrash()
	// {
	// 	Task.Delay(Delay + 1000).Wait();
	// 	App.WaitForElement("Bug Repro's");
	// }
}