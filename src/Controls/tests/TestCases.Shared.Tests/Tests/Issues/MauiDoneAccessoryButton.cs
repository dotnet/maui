#if IOS // MauiDoneAccessoryView is iOS only
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class MauiDoneAccessoryButton : _IssuesUITest
{
	const string DoneButtonAutomationId = "Microsoft.Maui.Platform.MauiDoneAccessory.DoneButton";

	public MauiDoneAccessoryButton(TestDevice device) : base(device)
	{
	}

	public override string Issue => "MauiDoneAccessoryView DoneButton should be accessible for UI testing";

	[Test]
	[Category(UITestCategories.Entry)]
	public void DoneButtonShouldHaveAccessibilityIdentifier()
	{
		// Wait for the entry to be ready
		App.WaitForElement("NumericEntry");
		
		// Tap the entry to bring up the keyboard
		App.Tap("NumericEntry");
		
		// Wait for the Done button to appear in the keyboard accessory view
		var doneButton = App.WaitForElement(DoneButtonAutomationId);
		
		// Verify the Done button exists and is accessible
		Assert.That(doneButton, Is.Not.Null, "Done button should be accessible via its automation ID");
		
		// Tap the Done button
		App.Tap(DoneButtonAutomationId);
		
		// Verify the completed event was triggered
		var result = App.FindElement("ResultLabel").GetText();
		Assert.That(result, Does.Contain("completed"), "Done button tap should trigger entry completion");
	}
}
#endif
