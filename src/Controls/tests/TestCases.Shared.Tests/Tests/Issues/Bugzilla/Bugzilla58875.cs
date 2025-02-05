using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla58875 : _IssuesUITest
{
	public Bugzilla58875(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Back navigation disables Context Action in whole app, if Context Action left open";

	// 	[Test]
	// 	[Category(UITestCategories.ContextActions)]
	// 	public void Bugzilla58875Test()
	// 	{
	// 		App.WaitForElement(q => q.Marked(Button1Id));
	// 		App.Tap(q => q.Marked(Button1Id));
	// 		App.WaitForElement(q => q.Marked(Target));
	// 		App.ActivateContextMenu(Target);
	// 		App.WaitForElement(q => q.Marked(ContextAction));
	// 		App.Back();

	// #if ANDROID
	// 		App.Back(); // back button dismisses the ContextAction first, so we need to hit back one more time to get to previous page
	// #endif

	// 		App.WaitForElement(q => q.Marked(Button1Id));
	// 		App.Tap(q => q.Marked(Button1Id));
	// 		App.WaitForElement(q => q.Marked(Target));
	// 		App.ActivateContextMenu(Target);
	// 		App.WaitForElement(q => q.Marked(ContextAction));
	// 	}
}