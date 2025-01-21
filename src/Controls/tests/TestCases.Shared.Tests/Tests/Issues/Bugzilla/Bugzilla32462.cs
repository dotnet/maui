using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla32462 : _IssuesUITest
{
	public Bugzilla32462(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Crash after a page disappeared if a ScrollView is in the HeaderTemplate property of a ListView";

	[Test]
	[Category(UITestCategories.ListView)]
	public void Bugzilla36729Test()
	{
		App.WaitForElement("Click!");
		App.Tap("Click!");
		App.WaitForElement("listview");
		App.WaitForElement("some text 35");
		App.TapBackArrow();
	}
}
