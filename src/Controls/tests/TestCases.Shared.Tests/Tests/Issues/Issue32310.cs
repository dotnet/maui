using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32310 : _IssuesUITest
{
	public Issue32310(TestDevice device) : base(device)
	{
	}

	public override string Issue => "App hangs if PopModalAsync is called after PushModalAsync with single await Task.Yield()";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void ModalNavigationShouldNotHangWithTaskYield()
	{
		// Wait for the test button
		App.WaitForElement("TestButton");

		// Tap the test button which will:
		// 1. Push a modal
		// 2. Yield
		// 3. Pop the modal
		// Without the fix, this would hang with a blank screen
		App.Tap("TestButton");

		// Wait for the status label to update to "Success"
		// If the app hangs, this will timeout and fail the test
		App.WaitForElement("StatusLabel");
		
		// Verify the status is "Success" which means the modal operations completed
		var statusLabel = App.FindElement("StatusLabel");
		Assert.That(statusLabel.GetText(), Is.EqualTo("Success"), 
			"Modal push/pop with Task.Yield should complete successfully without hanging");
	}
}
