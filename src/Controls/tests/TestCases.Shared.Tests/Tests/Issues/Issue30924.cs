#if IOS // The fix and this test currently cover the iOS handler only; Android/Windows coverage follows in a separate change.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30924 : _IssuesUITest
{
	const string Header = "30924GroupHeader";
	const string Toggle = "30924HeaderToggle";

	public override string Issue => "CollectionView GroupHeaders do not resize if you hide the content";

	public Issue30924(TestDevice device) : base(device)
	{
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void HidingTheGroupHeaderShouldCollapseItsSpace()
	{
		App.WaitForElement(Header);
		var firstItemBefore = App.WaitForElement("A1").GetRect();

		App.Tap(Toggle);
		App.WaitForNoElement(Header);

		var firstItemAfter = App.WaitForElement("A1").GetRect();

		Assert.That(firstItemAfter.Y, Is.LessThan(firstItemBefore.Y - 40),
			"The group header should collapse its height when its content is hidden.");

		// Put the page back in its initial state.
		App.Tap(Toggle);
		App.WaitForElement(Header);
	}
}
#endif
