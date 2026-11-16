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
		App.ScrollTo("FooterLabel");
		Assert.That(App.WaitForTextEqualToElement("StatusLabel", "Threshold reached"), Is.True);
	}
}
