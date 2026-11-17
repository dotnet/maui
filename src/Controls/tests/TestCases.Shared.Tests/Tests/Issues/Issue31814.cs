#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Catalyst: WebDriverAgentMac only supports mouse pointers, so the gesture scroll fails. Windows: RemainingItemsThresholdReached isn't raised for a single item under a Header there; this fix is Android only
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31814 : _IssuesUITest
{
	public Issue31814(TestDevice device) : base(device) { }

	public override string Issue => "RemainingItemsThresholdReached is not raised for a single item when the CollectionView has a Header";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView)]
	public void RemainingItemsThresholdReachedFiresForSingleItemWithHeader()
	{
		App.WaitForElement("CollectionView");

		// A short scroll keeps only the tall Header on screen, so no item is visible yet
		App.ScrollDown("CollectionView", ScrollStrategy.Gesture, 0.3, 1000);
		WaitForScrollToSettle();
		Assert.That(App.FindElement("StatusLabel").GetText(), Is.EqualTo("Waiting"));

		App.ScrollTo("FooterLabel");
		Assert.That(App.WaitForTextEqualToElement("StatusLabel", "Threshold reached"), Is.True);
	}

	// Scrolling (and any fling) has ended once the offset reported by the Scrolled event stops changing
	void WaitForScrollToSettle()
	{
		var offset = App.FindElement("OffsetLabel").GetText();
		for (int i = 0; i < 20; i++)
		{
			Thread.Sleep(500);
			var current = App.FindElement("OffsetLabel").GetText();
			if (current == offset)
			{
				return;
			}

			offset = current;
		}

		Assert.Fail("Scrolling did not settle");
	}
}
#endif
