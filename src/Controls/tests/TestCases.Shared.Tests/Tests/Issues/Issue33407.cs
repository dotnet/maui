#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33407 : _IssuesUITest
{
	public Issue33407(TestDevice device) : base(device) { }

	public override string Issue => "Focusing and entering texts on entry control causes a gap at the top after rotating simulator.";

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryFocusedShouldNotCauseGapAfterRotation()
	{
		// Navigate: Categories → tap "E - Entry"
		App.WaitForElement("CategoryE");
		App.Tap("CategoryE");

		// Navigate: Entry list → tap "E1"
		App.WaitForElement("TestE1");
		App.Tap("TestE1");

		// Wait for first Entry on E1 page
		App.WaitForElement("Entry1");

		// Click/focus the first Entry
		App.Tap("Entry1");

		// Rotate to landscape
		App.SetOrientationLandscape();
		// Allow orientation change to settle
		Thread.Sleep(2000);

		// Verify no gap appears at top after rotation with Entry focused
		VerifyScreenshot("EntryFocusedLandscape");

		// Navigate back using back button
		App.Back();

		// Allow keyboard dismiss + page transition to settle
		Thread.Sleep(1500);

		// Verify the previous page looks correct after returning
		VerifyScreenshot("EntryListAfterBack");
	}
}
#endif
