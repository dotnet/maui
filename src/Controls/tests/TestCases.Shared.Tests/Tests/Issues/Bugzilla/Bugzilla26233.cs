using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla26233 : _IssuesUITest
{
	public Bugzilla26233(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Windows phone crashing when going back to page containing listview with Frame inside ViewCell";

	[Test]
	[Category(UITestCategories.ListView)]
	public void DoesntCrashOnNavigatingBackToThePage()
	{
		App.WaitForElement("btnPush");
		App.Tap("btnPush");
		App.WaitForElement("btnBack");
		App.Screenshot("I see the back button");
		App.Tap("btnBack");
		App.WaitForElement("btnPush");
	}
}