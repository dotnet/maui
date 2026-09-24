using System.Diagnostics;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue29588 : _IssuesUITest
{
	public override string Issue => "CollectionView RemainingItemsThresholdReachedcommand should trigger on scroll near end";

	public Issue29588(TestDevice device) : base(device)
	{
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void RemainingItemsThresholdReachedEventShouldTrigger()
	{
		App.WaitForElement("29588CollectionView");
		var thresholdLabel = App.WaitForElement("29588ThresholdLabel");
		ScrollToVisibleItem("Item 20");
		Assert.That(() => thresholdLabel.GetText(), Is.EqualTo("Threshold reached").After(5000, 200));
		ScrollToVisibleItem("Loaded Item 30");
		Assert.That(App.WaitForElement("Loaded Item 30").GetText(), Is.EqualTo("Loaded Item 30"));
	}

	void ScrollToVisibleItem(string text)
	{
		var elapsed = Stopwatch.StartNew();
		while (elapsed.Elapsed < TimeSpan.FromSeconds(15))
		{
			var viewport = App.WaitForElementAndGetRect("29588CollectionView");
			var item = App.FindElementByText(text);
			if (item is not null)
			{
				var bounds = item.GetRect();
				if (bounds.Width > 0 && bounds.Height > 0
					&& bounds.Left >= viewport.Left && bounds.Right <= viewport.Right
					&& bounds.Top >= viewport.Top && bounds.Bottom <= viewport.Bottom)
					return;
			}

			// Container scrolling uses the native macOS scroll action, unlike App.ScrollTo.
			App.ScrollDown("29588CollectionView", ScrollStrategy.Gesture, swipePercentage: 0.5);
		}

		Assert.Fail($"'{text}' did not become fully visible inside 29588CollectionView.");
	}
}
