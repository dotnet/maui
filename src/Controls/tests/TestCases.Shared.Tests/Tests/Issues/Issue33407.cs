#if IOS || ANDROID // Orientation change is not supported on Windows and MacCatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33407 : _IssuesUITest
{
	public Issue33407(TestDevice device) : base(device) { }

	public override string Issue => "Focusing and entering texts on entry control causes a gap at the top after rotating simulator.";

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryFocusedShouldNotCauseGapAfterRotation()
	{
		App.WaitForElement("CategoryE");
		App.Tap("CategoryE");

		App.WaitForElement("TestE1");
		App.Tap("TestE1");

		App.WaitForElement("Entry1");
		App.Tap("Entry1");

		// Rotate while keyboard is visible — RestorePosition() incorrectly used the portrait Y
		// in landscape space before the fix, causing a gap at the top.
		App.SetOrientationLandscape();

		// Dismiss keyboard before screenshot to avoid cursor flakiness.
		App.DismissKeyboard();

		// Use retryTimeout to wait for the keyboard-dismiss/restore animation to fully settle
		// before asserting, avoiding flaky mid-transition screenshots.
#if ANDROID
		VerifyScreenshot(cropLeft:125, retryTimeout: TimeSpan.FromSeconds(2));
#else
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
#endif
	}
}
#endif
