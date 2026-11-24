#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Windows: the threshold check has no visible-item guard (ItemCount - 1 - LastVisibleItemIndex is 0 for an empty source), so the event is raised. Catalyst: WebDriverAgentMac only supports mouse pointers, so the gesture scroll fails
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31814_EmptySource : _IssuesUITest
{
	public Issue31814_EmptySource(TestDevice device) : base(device) { }

	public override string Issue => "RemainingItemsThresholdReached is not raised for an empty source with a Footer";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView)]
	public void RemainingItemsThresholdReachedNotRaisedForEmptySourceWithFooter()
	{
		App.WaitForElement("CollectionView");

		App.ScrollDown("CollectionView", ScrollStrategy.Gesture, 0.3, 1000);
		WaitForScrollToSettle();
		Assert.That(App.FindElement("StatusLabel").GetText(), Is.EqualTo("Waiting"));
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
