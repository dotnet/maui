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
		
		// Wait for WebView to load
		Thread.Sleep(3000);
		
		// The WebView content should be loaded with links
		App.WaitForElement("TestWebView");
		
		// Tap on the link within the WebView
		// Note: Since we can't directly tap on elements inside a WebView with Appium,
		// we'll tap on the WebView area where the link should be
		App.Tap("TestWebView");
		
		// Wait a bit for the navigation event to be processed
		Thread.Sleep(1000);
		
		// Verify that the Navigating event was triggered
		var navigatingLabel = App.WaitForElement("NavigatingLabel");
		Assert.That(navigatingLabel.GetText(), Is.EqualTo("Navigating event triggered"));
		
		// Verify that the URL was captured
		var urlLabel = App.WaitForElement("UrlLabel");
		var urlText = urlLabel.GetText();
		Assert.That(urlText, Does.Contain("URL:"));
		Assert.That(urlText, Does.Contain("microsoft.com").Or.Contains("github.com"));
		
		// Verify that navigation can be cancelled
		var cancelLabel = App.WaitForElement("CancelLabel");
		Assert.That(cancelLabel.GetText(), Does.Contain("Can cancel: Yes"));
	}
}
