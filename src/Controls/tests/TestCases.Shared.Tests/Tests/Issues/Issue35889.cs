using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35889 : _IssuesUITest
{
	public Issue35889(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Empty CollectionView (CV2) expands to fill available space on iOS instead of collapsing to zero height";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void EmptyCollectionViewCollapsesAndExpandsWithItems()
	{
		App.WaitForElement("BeforeCVLabel");
		App.WaitForElement("AfterCVLabel");

		// Empty state: gap between flanking labels must be ~0 (CV collapsed)
		var beforeRect = App.WaitForElement("BeforeCVLabel").GetRect();
		var afterRectEmpty = App.WaitForElement("AfterCVLabel").GetRect();
		var emptyGap = afterRectEmpty.Y - (beforeRect.Y + beforeRect.Height);
		Assert.That(emptyGap, Is.EqualTo(0).Within(2), "Empty CollectionView should have zero height — gap between labels should be ~0.");

		// Add items and wait for status label to confirm collection updated
		App.Tap("AddItemsButton");
		App.WaitForElement(() =>
		{
			var el = App.FindElement("StatusLabel");
			return el?.GetText() == "HasItems" ? el : null;
		});

		// With items: AfterCVLabel must have moved down
		var afterRectWithItems = App.WaitForElement("AfterCVLabel").GetRect();
		var filledGap = afterRectWithItems.Y - (beforeRect.Y + beforeRect.Height);
		Assert.That(filledGap, Is.GreaterThan(0), "CollectionView should have non-zero height after items are added.");
	}
}

