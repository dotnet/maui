#if TEST_FAILS_ON_WINDOWS // Using ActivateContextMenu internally does a right click and is not working correctly.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla58875 : _IssuesUITest
{
	const string Button1Id = "Button1Id";
	const string ContextAction = "More";
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
		App.ActivateContextMenu(Target);
		App.WaitForElement(ContextAction);
		App.Back();

#if ANDROID
		App.Back(); // back button dismisses the ContextAction first, so we need to hit back one more time to get to previous page
#endif

		App.WaitForElement(Button1Id);
		App.Tap(Button1Id);
		App.WaitForElement(Target);
		App.ActivateContextMenu(Target);
		App.WaitForElement(ContextAction);
	}
}