using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23158 : _IssuesUITest
{
	public override string Issue => "Respect Entry.ClearButtonVisibility on Windows";

	public Issue23158(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public async Task ValidateEntryClearButtonVisibilityBehavior()
	{
		App.WaitForElement("TestInstructions");

		// Click the button to add dynamically Entry3.
		App.Click("AddEntry");

		await Task.Delay(500);

		// Click the new entry to see if there is the clear button or not. No such button should be present.
		App.Tap("Entry3");

		await Task.Delay(500);

		VerifyScreenshot();
	}
}