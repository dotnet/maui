#if TEST_FAILS_ON_WINDOWS // Drag and drop with grouping is not supported on Windows (as in Issue12008)
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39880 : _IssuesUITest
{
	public Issue39880(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView with CanReorderItems crashes when an item is dragged onto a Footer or GroupFooter";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void DraggingItemOntoFooterDoesNotCrash()
	{
		App.WaitForElement("Item39880C");
		App.DragAndDrop("Item39880C", "Footer39880");

		// The move onto the footer is refused: the app is alive and the order is unchanged.
		Assert.That(App.WaitForElement("Order39880").GetText(), Is.EqualTo("A,B,C"));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void DraggingItemOntoOwnGroupFooterDoesNotCrash()
	{
		App.WaitForElement("Item39880Banana");
		App.DragAndDrop("Item39880Banana", "GroupFooter39880Fruits");

		Assert.That(App.WaitForElement("GroupOrder39880").GetText(), Is.EqualTo("Fruits:Apple,Banana|Vegetables:Carrot,Leek"));
	}
}
#endif
