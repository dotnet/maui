#if IOS
using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

// Issue: https://github.com/dotnet/maui/issues/39229
// Re-tapping the selected Shell tab while a push is still animating makes UIKit
// pop the section to its root mid-transition, leaving the tab permanently blank.
public class Issue39229 : _IssuesUITest
{
	public Issue39229(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell tab re-selection during push leaves content blank";

	static (float x, float y) Center(Microsoft.Maui.Graphics.Rectangle rect) =>
		(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);

	[Test]
	[Category(UITestCategories.Navigation)]
	public async Task ReselectingTabDuringPushKeepsPageContentVisible()
	{
		App.WaitForElement("HomeContent");
		var openButton = App.WaitForElement("OpenChildPage").GetRect();
		var homeTab = App.WaitForElement("HomeTab").GetRect();
		var openCenter = Center(openButton);
		var tabCenter = Center(homeTab);

		// Start a push and immediately re-tap the already-selected Home tab so the
		// taps land inside the push animation window. A third tap 150ms later covers
		// the "re-tap during an earlier re-tap's transition" scenario. All of them
		// must be ignored while the transition is in flight.
		App.TapCoordinates(openCenter.x, openCenter.y);
		App.TapCoordinates(tabCenter.x, tabCenter.y);
		await Task.Delay(150);
		App.TapCoordinates(tabCenter.x, tabCenter.y);

		// The push must complete undisturbed and the child page stays visible.
		App.WaitForElement("ChildContent");

		// Switching away and back must not blank the tab either.
		App.Tap("OtherTab");
		App.WaitForElement("OtherContent");

		App.Tap("HomeTab");
		var child = App.WaitForElement("ChildContent").GetRect();

		Assert.Multiple(() =>
		{
			Assert.That(child.Width, Is.GreaterThan(0));
			Assert.That(child.Height, Is.GreaterThan(0));
		});
	}

	[Test]
	[Category(UITestCategories.Navigation)]
	public async Task ReselectingTabDuringPushDoesNotLeaveBlankContent()
	{
		App.WaitForElement("HomeContent");
		var openButton = App.WaitForElement("OpenChildPage").GetRect();
		var homeTab = App.WaitForElement("HomeTab").GetRect();
		var openCenter = Center(openButton);
		var tabCenter = Center(homeTab);

		App.TapCoordinates(openCenter.x, openCenter.y);
		App.TapCoordinates(tabCenter.x, tabCenter.y);
		await Task.Delay(150);
		App.TapCoordinates(tabCenter.x, tabCenter.y);

		// Give the navigation window a moment to settle; the child page must be
		// rendered on screen. Without the fix this screenshot captures a blank tab
		// (Shell's logical state is consistent, but the content is gone).
		await Task.Delay(500);
		VerifyScreenshot();
	}
}
#endif
