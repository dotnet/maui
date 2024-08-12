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

	[FailsOnAllPlatforms("TODO From Xamarin.UITest migration, doesn't seem to work, check later")]
	[Test]
	[Category(UITestCategories.ListView)]
	public void DoesntCrashOnNavigatingBackToThePage()
	{
		App.WaitForElement("btnPush");
		App.Tap("btnPush");
		App.WaitForElement("back");
		App.Screenshot("I see the back button");
		App.Tap("back");
		App.WaitForElement("btnPush");
	}
}