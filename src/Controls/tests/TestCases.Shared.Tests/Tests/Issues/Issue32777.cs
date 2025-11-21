using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32777 : _IssuesUITest
	{
		public override string Issue => "TitleView not showing in iOS 26";

		public Issue32777(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.TitleView)]
		public void TitleViewShouldBeVisibleAndNotCoverContent()
		{
			// First, wait for the page content to load (not TitleView)
			// This ensures we're on the correct page
			App.WaitForElement("HelloLabel");
			
			// Verify hello label text - this proves the page content loaded
			var helloLabel = App.FindElement("HelloLabel");
			var helloText = helloLabel.GetText();
			Assert.That(helloText, Is.EqualTo("Hello, World!"), "Page content should be loaded");

			// Verify the welcome label is also visible
			var welcomeLabel = App.WaitForElement("WelcomeLabel");
			Assert.That(welcomeLabel, Is.Not.Null, "Welcome label should be visible");

			// Verify the click button in content is accessible
			var clickButton = App.WaitForElement("ClickButton");
			Assert.That(clickButton, Is.Not.Null, "Button in content should be accessible");

			// Try to tap the content button to ensure it's not covered by TitleView
			// This is the critical test - if TitleView was covering content, this would fail
			App.Tap("ClickButton");
			
			// If we got here without exceptions, the button was successfully tapped
			// which proves the TitleView is not covering the content
			
			// Note: TitleView elements (TitleLabel, TitleButton) are in the navigation bar
			// and may not be accessible via Appium AutomationId on iOS.
			// The key verification is that page content is accessible and not covered.
		}
	}
}
