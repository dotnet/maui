using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue33415 : _IssuesUITest
	{
		public override string Issue => "ApplyQueryAttributes gets called with empty Dictionary on back";

		public Issue33415(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Shell)]
		public void ApplyQueryAttributesShouldNotBeCalledWithEmptyDictionaryOnBack()
		{
			// Wait for main page to load
			App.WaitForElement("StatusLabel");
			App.WaitForElement("CallCountLabel");

			// Verify initial state - ApplyQueryAttributes should not have been called yet
			var initialStatus = App.FindElement("StatusLabel").GetText();
			var initialCallCount = App.FindElement("CallCountLabel").GetText();
			Assert.That(initialCallCount, Is.EqualTo("Call count: 0"), "ApplyQueryAttributes should not be called on initial page load");

			// Tap navigate button to go to second page WITH parameters
			App.Tap("NavigateButton");

			// Wait for ApplyQueryAttributes to be called with parameters
			App.WaitForElement("StatusLabel");
			
			// Give the UI time to update after ApplyQueryAttributes
			App.WaitForElement("StatusLabel");
			
			// Verify ApplyQueryAttributes was called with data (not empty)
			var statusAfterNav = App.FindElement("StatusLabel").GetText();
			var callCountAfterNav = App.FindElement("CallCountLabel").GetText();
			Assert.That(callCountAfterNav, Is.EqualTo("Call count: 1"), "ApplyQueryAttributes should be called once with parameters");
			Assert.That(statusAfterNav, Does.Contain("1 parameter"), "Status should indicate parameters were received");

			// Wait for second page to appear
			App.WaitForElement("ReceivedLabel");
			
			// Verify second page received the parameter
			var receivedText = App.FindElement("ReceivedLabel").GetText();
			Assert.That(receivedText, Is.EqualTo("Received: TestValue"), "Second page should receive the parameter");

			// Go back to main page
			App.Tap("BackButton");

			// Wait for main page to reappear
			App.WaitForElement("StatusLabel");
			App.WaitForElement("CallCountLabel");

			// BUG: According to documentation, after calling query.Clear() in ApplyQueryAttributes,
			// the method should NOT be called again when navigating back.
			// However, it IS called with an empty dictionary.
			var finalCallCount = App.FindElement("CallCountLabel").GetText();
			var finalStatus = App.FindElement("StatusLabel").GetText();

			// This assertion will FAIL (demonstrating the bug)
			// Expected: Call count should still be 1 (ApplyQueryAttributes should not be called on back)
			// Actual: Call count will be 2 (ApplyQueryAttributes IS called with empty dictionary)
			Assert.That(finalCallCount, Is.EqualTo("Call count: 1"), 
				"ApplyQueryAttributes should NOT be called when navigating back after query.Clear()");
			
			// If the bug exists, this will show it was called with empty dictionary
			if (finalCallCount != "Call count: 1")
			{
				Assert.That(finalStatus, Is.EqualTo("Status: ApplyQueryAttributes called with EMPTY dictionary"),
					"If ApplyQueryAttributes is incorrectly called, it's called with empty dictionary");
			}
		}
	}
}
