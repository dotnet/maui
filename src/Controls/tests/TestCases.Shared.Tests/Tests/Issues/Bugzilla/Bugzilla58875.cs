#if WINDOWS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla58875 : _IssuesUITest
{
	const string Button1Id = "Button1Id";
	const string Target = "Swipe me";
	public Bugzilla58875(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Back navigation disables Context Action in whole app, if Context Action left open";

	[Test]
	[Category(UITestCategories.ContextActions)]
	public void Bugzilla58875Test()
	{
		App.WaitForElement(Button1Id);
		App.Tap(Button1Id);
		App.WaitForElement(Target);
		App.ContextActions(Target);
		App.WaitForElement("More");
		App.TapBackArrow();
		App.TapBackArrow(); // back button dismisses the ContextAction first, so we need to hit back one more time to get to previous page

		App.WaitForElement(Button1Id);
		App.Tap(Button1Id);
		App.WaitForElement(Target);
		App.ContextActions(Target);
		App.WaitForElement("More");
	}
}
#endif