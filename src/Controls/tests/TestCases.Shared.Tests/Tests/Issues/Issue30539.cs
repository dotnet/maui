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
		// Wait for the page to load
		App.WaitForElement("TriggerLinkButton");

		// Click the button to trigger the target="_blank" link via JavaScript
		App.Tap("TriggerLinkButton");

		// Wait for the NavigatingLabel to change to "triggered"
		// Using a polling approach with retries
		var maxAttempts = 20; // 20 attempts * 500ms = 10 seconds max
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
	}
}
