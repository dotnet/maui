using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33287 : _IssuesUITest
{
	public override string Issue => "DisplayAlertAsync throws NullReferenceException when page is no longer displayed";

	public Issue33287(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Page)]
	public void DisplayAlertAsyncShouldNotCrashWhenPageUnloaded()
	{
		App.WaitForElement("NavigateButton");
		
		// Navigate to second page
		App.Tap("NavigateButton");
		
		// Wait for second page to appear
		App.WaitForElement("GoBackButton");
		
		// Immediately go back before the 5 second delay completes
		App.Tap("GoBackButton");
		
		// Wait for navigation to complete
		App.WaitForElement("StatusLabel");
		
		// Wait for the delayed DisplayAlertAsync to be triggered (5 seconds + buffer)
		System.Threading.Thread.Sleep(6000);
		
		// Get the status - should not show NullReferenceException
		var status = App.FindElement("StatusLabel").GetText();
		Console.WriteLine($"[TEST] Final status: {status}");
		
		// Assert that no NullReferenceException occurred
		Assert.That(status, Does.Not.Contain("NullReferenceException"), 
			"DisplayAlertAsync should not throw NullReferenceException when page is unloaded");
	}
}
