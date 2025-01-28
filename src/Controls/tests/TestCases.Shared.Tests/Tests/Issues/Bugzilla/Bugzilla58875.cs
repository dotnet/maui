#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // ContextActions Menu Items Not Accessible via Automation on iOS and Catalyst Platforms. 
//For more information see Issue Link: https://github.com/dotnet/maui/issues/27394
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
		App.WaitForElement(ContextAction, timeout: TimeSpan.FromSeconds(2));
		App.Back();

#if ANDROID
		App.Back(); // Back button dismisses the ContextAction first, so we need to hit back one more time to get to previous page
#endif

		App.WaitForElement(Button1Id);
		App.Tap(Button1Id);
		App.WaitForElement(Target);
		App.ActivateContextMenu(Target);
		App.WaitForElement(ContextAction, timeout: TimeSpan.FromSeconds(2));
	}
}
#endif