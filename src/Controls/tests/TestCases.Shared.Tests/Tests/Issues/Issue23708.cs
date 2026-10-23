using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23708 : _IssuesUITest
{
	public Issue23708(TestDevice device) : base(device) { }

	public override string Issue => "Grouped CollectionView has extra space before group headers";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView)]
	public void Issue23708GroupHeaderDoesNotHaveExtraVerticalItemSpacing()
	{
		App.WaitForElement("GroupedCollectionView");
		var header = App.WaitForElement("GroupHeaderSmall").GetRect();
		var firstRow = App.WaitForElement("ItemRat").GetRect();
		var secondRow = App.WaitForElement("ItemBird").GetRect();
		var thirdRow = App.WaitForElement("ItemCarrot").GetRect();

		// Normalize screen coordinates to layout units using the fixed-height header.
		var scale = header.Height / 40d;
		Assert.That(scale, Is.GreaterThan(0));
		Assert.That((secondRow.Y - firstRow.Y) / scale, Is.EqualTo(30).Within(2),
			"The first two rows should contain a 20-unit item and 10 units of spacing.");
		Assert.That((thirdRow.Y - secondRow.Y) / scale, Is.EqualTo(30).Within(2),
			"The final row before the next group should not retain the native minimum item height.");
	}
}
