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
		App.ResizeOrRotateWindow();
		
		// Poll for width change with timeout (wait for animation to complete)
		var timeout = TimeSpan.FromSeconds(5);
		var startTime = DateTime.Now;
		float newWidth;
		do
		{
			Thread.Sleep(100); // Small delay between polls
			var newTitleLabelRect = App.WaitForElement("TitleViewGrid").GetRect();
			newWidth = newTitleLabelRect.Width;
			
			if (newWidth > previousWidth)
				break;
				
			if (DateTime.Now - startTime > timeout)
				break;
		} while (true);
		
		Assert.That(newWidth, Is.GreaterThan(previousWidth), 
			$"TitleView should expand when window is maximized. Previous: {previousWidth}, New: {newWidth}");
	}
}
#endif
