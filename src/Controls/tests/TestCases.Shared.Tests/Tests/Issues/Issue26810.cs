#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26810 : _IssuesUITest
{
	public override string Issue => "Scroll To first item in CollectionView when updating the collection with KeepItemsInView";

	public Issue26810(TestDevice device) : base(device)
	{
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void ScrollToFirstItemOnCollectionChanged()
	{
		App.WaitForElement("Item1");
		App.WaitForElement("26810Button");
		App.Tap("26810Button");
		App.WaitForElement("Item20");
		App.WaitForElement("26810AddButton");
		App.Tap("26810AddButton");
		App.WaitForElement("Item1");

		App.WaitForElement("26810ScrollOffsetButton");
		App.Tap("26810ScrollOffsetButton");
		App.Tap("26810Button");
		App.WaitForElement("Item20");
		App.Tap("26810AddButton");
		App.WaitForElement("Item20");

		App.WaitForElement("26810LastItemButton");
		App.Tap("26810LastItemButton");
		App.Tap("26810AddButton");
		App.WaitForElement("Item33");
	}
}
#endif
