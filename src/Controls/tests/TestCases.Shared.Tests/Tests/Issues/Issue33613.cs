#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // This test is specific to Mac Catalyst window maximize/restore behavior
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33613 : _IssuesUITest
{
	public override string Issue => "Mac Catalyst: NavigationPage.TitleView layout shifts and adds extra spacing when window is maximized";
	
	public Issue33613(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Navigation)]
	public void TitleViewLayoutIsCorrectWithBackButton()
	{
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");
		App.WaitForElement("TitleLabel");
		var titleLabelRect = App.WaitForElement("TitleLabel").GetRect();
		var previousWidth = titleLabelRect.Width;
		
		App.EnterFullScreen();
		Thread.Sleep(1000); // Wait for the animation to complete
		
		var newTitleLabelRect = App.WaitForElement("TitleLabel").GetRect();
		var newWidth = newTitleLabelRect.Width;
		
		// Test passes if the TitleView width increases when entering full screen
		// This verifies the TitleView properly expands to fill the available navigation bar space
		Assert.That(newWidth, Is.GreaterThan(previousWidth), 
			$"TitleView should expand when window is maximized. Previous: {previousWidth}, New: {newWidth}");
		
		App.Tap("GoBackButton");
		App.WaitForElement("NavigateButton");
	}
}
#endif
