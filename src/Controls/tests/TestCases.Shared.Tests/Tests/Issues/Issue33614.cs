using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33614 : _IssuesUITest
{
	public override string Issue => "CollectionView Scrolled event reports incorrect FirstVisibleItemIndex after programmatic ScrollTo";

	public Issue33614(TestDevice device) : base(device) { }

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void FirstVisibleItemIndexShouldBeCorrectAfterScrollTo()
	{
		App.WaitForElement("ScrollToButton");
		App.WaitForElement("Item_0");
		App.Tap("ScrollToButton");
		App.RetryAssert(() =>
			Assert.That(App.WaitForElement("FirstIndexLabel").GetText(),
				Is.EqualTo("FirstVisibleItemIndex: 15")));
	}
}
