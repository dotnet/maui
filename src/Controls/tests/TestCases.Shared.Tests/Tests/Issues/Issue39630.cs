using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39630 : _IssuesUITest
{
	public Issue39630(TestDevice device)
		: base(device)
	{
	}

	public override string Issue =>
		"Selecting a CollectionView item invokes an ancestor TapGestureRecognizer";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void SelectingItemDoesNotInvokeParentTapGesture()
	{
		App.WaitForElement("Issue39630CollectionView");
		App.Tap("Issue39630FirstItem");

		App.RetryAssert(() =>
		{
			Assert.That(App.FindElement("Issue39630Selection").GetText(), Is.EqualTo("Selected item: Tap this item"));
			Assert.That(App.FindElement("Issue39630ParentTapCount").GetText(), Is.EqualTo("Parent Grid taps: 0"));
		});
	}
}
