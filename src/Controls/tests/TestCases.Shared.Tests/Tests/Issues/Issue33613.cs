#if TEST_FAILS_ON_WINDOWS // TitleView is not rendered at its full width on the Windows platform during the initial render.
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
	public void Issue33613Test()
	{
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");
		App.WaitForElement("TitleViewGrid");
		var titleLabelRect = App.WaitForElement("TitleViewGrid").GetRect();
		var previousWidth = titleLabelRect.Width;
#if MACCATALYST || WINDOWS
		App.EnterFullScreen();
#elif ANDROID || IOS
		App.SetOrientationLandscape();
#endif
		Thread.Sleep(1000); // Wait for the animation to complete
		var newTitleLabelRect = App.WaitForElement("TitleViewGrid").GetRect();
		var newWidth = newTitleLabelRect.Width;
		Assert.That(newWidth, Is.GreaterThan(previousWidth), 
			$"TitleView should expand when window is maximized. Previous: {previousWidth}, New: {newWidth}");
	}
}
#endif
