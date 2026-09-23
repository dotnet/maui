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
		App.ScrollTo("Item 20");
		App.RetryAssert(() => Assert.That(App.WaitForElement("29588ThresholdLabel").GetText(),
			Is.EqualTo("Threshold reached")));
		App.ScrollTo("Loaded Item 30");
		Assert.That(App.WaitForElement("Loaded Item 30").GetText(), Is.EqualTo("Loaded Item 30"));
	}
}
