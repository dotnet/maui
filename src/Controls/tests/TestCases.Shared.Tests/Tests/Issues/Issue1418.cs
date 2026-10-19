using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1418 : _IssuesUITest
{
	public Issue1418(TestDevice device) : base(device)
	{
	}

	public override string Issue => "RefreshView preserves horizontal scroll position";

	[Test]
	[Category(UITestCategories.RefreshView)]
	public void ProgrammaticRefreshPreservesHorizontalScrollPosition()
	{
		App.WaitForElement("ScrollToEnd");
		App.WaitForElement("HorizontalCollectionView");
		App.Tap("ScrollToEnd");
		App.WaitForElement("Item29");

		App.WaitForElement("ToggleRefresh");
		var stopRefreshButton = App.WaitForElement("ToggleRefresh").GetRect();
		App.Tap("ToggleRefresh");
		Assert.That(App.WaitForTextToBePresentInElement("RefreshStatus", "IsRefreshing: True"),
			Is.True, "Refresh should remain active until explicitly stopped.");
		App.WaitForElement("Item29",
			timeoutMessage: "The last item should remain visible after starting a refresh.");

		// Native macOS element clicks wait for the active refresh animation to become idle.
		App.TapCoordinates(stopRefreshButton.CenterX(), stopRefreshButton.CenterY());
		Assert.That(App.WaitForTextToBePresentInElement("RefreshStatus", "IsRefreshing: False"),
			Is.True, "Refresh should stop when toggled again.");
		App.WaitForElement("Item29",
			timeoutMessage: "The last item should remain visible after completing a refresh.");
	}
}
