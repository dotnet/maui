#if IOS // This test is iOS-specific as the fix addresses iOS behavior for target="_blank" links
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30539 : _IssuesUITest
{
	public override string Issue => "Clicking on a target=\"_blank\" link in WebView on iOS does not do anything";

	public Issue30539(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void TargetBlankLinkShouldTriggerNavigatingEvent()
	{
		App.WaitForElement("TestInstructions");

		// Wait for the WebView to load and the JavaScript to trigger the click
		// The page will automatically click the target="_blank" link once loaded
		// We need to wait for the Navigating event to be triggered and the label to update

		// Wait for the NavigatingLabel to change from "not triggered" to "triggered"
		// Using a polling approach with retries instead of Thread.Sleep
		var maxAttempts = 30; // 30 attempts * 500ms = 15 seconds max
		var navigatingLabel = App.WaitForElement("NavigatingLabel");

		for (int i = 0; i < maxAttempts; i++)
		{
			var text = navigatingLabel?.GetText();
			if (text != null && text.Contains("Navigating event triggered", StringComparison.Ordinal))
			{
				break;
			}

			if (i < maxAttempts - 1)
			{
				Thread.Sleep(500);
				navigatingLabel = App.WaitForElement("NavigatingLabel");
			}
		}

		// Verify that the Navigating event was triggered
		Assert.That(navigatingLabel?.GetText(), Is.EqualTo("Navigating event triggered"));

		// Verify that the URL was captured
		var urlLabel = App.WaitForElement("UrlLabel");
		var urlText = urlLabel?.GetText();
		Assert.That(urlText, Does.Contain("URL:"));
		Assert.That(urlText, Does.Contain("microsoft.com").Or.Contains("github.com"));

		// Verify that navigation can be cancelled
		var cancelLabel = App.WaitForElement("CancelLabel");
		Assert.That(cancelLabel?.GetText(), Does.Contain("Can cancel: Yes"));
	}
}
#endif
