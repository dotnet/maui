using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.RefreshView)]
public class Issue16910 : _IssuesUITest
{
	public override string Issue => "IsRefreshing binding works";

	protected override bool ResetAfterEachTest => true;
	public Issue16910(TestDevice device)
		: base(device)
	{

	}

	// On MacCatalyst, each Appium FindElement call takes ~76s because the mac2 driver
	// walks the entire accessibility tree. With RunWithTimeout capping commands at 45s,
	// the test reliably times out. On main (without RunWithTimeout), the same test takes
	// ~1m16s per run but eventually succeeds because nothing caps the HTTP call.
	// See: build 1299547 (main) vs build 1306375 (this PR) for timing comparison.
#if TEST_FAILS_ON_CATALYST
	[Test]
	public void BindingUpdatesFromProgrammaticRefresh()
	{
		App.WaitForElement("StartRefreshing", timeout: TimeSpan.FromSeconds(45));
		App.Tap("StartRefreshing");
		App.WaitForElement("IsRefreshing", timeout: TimeSpan.FromSeconds(45));
		App.Tap("StopRefreshing");
		App.WaitForElement("IsNotRefreshing", timeout: TimeSpan.FromSeconds(45));
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Failing on Mac and Windows. Flaky Test. More information: https://github.com/dotnet/maui/issues/28368
	[Test]
	public void BindingUpdatesFromInteractiveRefresh()
	{
		var scrollViewRect = App.WaitForElement("RefreshScrollView", timeout: TimeSpan.FromSeconds(45)).GetRect();
		//In CI, using App.ScrollDown sometimes fails to trigger the refresh command, so here use DragCoordinates instead of the ScrollDown action in Appium.
		App.DragCoordinates(scrollViewRect.CenterX(), scrollViewRect.Y + 50, scrollViewRect.CenterX(), scrollViewRect.Y + scrollViewRect.Height - 50);
		App.WaitForElement("IsRefreshing", timeout: TimeSpan.FromSeconds(45));
		App.Tap("StopRefreshing");
		App.WaitForElement("IsNotRefreshing", timeout: TimeSpan.FromSeconds(45));
	}
#endif
}