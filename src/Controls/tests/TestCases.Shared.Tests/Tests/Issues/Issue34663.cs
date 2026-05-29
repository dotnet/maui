using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34663 : _IssuesUITest
{
	public Issue34663(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CV2 MakeVisible grouped ScrollTo produces inconsistent positions";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void MakeVisibleScrollToGroupedItemProducesConsistentPosition()
	{
		App.WaitForElement("ScrollToThirdCatButton");

		App.Tap("ScrollToThirdCatButton");
		var bengalRect1 = App.WaitForElement("Bengal").GetRect();
		var baselineY = bengalRect1.Y;

		App.ScrollUp("GroupedCollectionView");
		App.ScrollUp("GroupedCollectionView");
		App.ScrollUp("GroupedCollectionView");

		App.Tap("ScrollToThirdCatButton");
		var bengalRect2 = App.WaitForElement("Bengal").GetRect();

		App.ScrollUp("GroupedCollectionView");
		App.ScrollUp("GroupedCollectionView");
		App.ScrollUp("GroupedCollectionView");

		App.Tap("ScrollToThirdCatButton");
		var bengalRect3 = App.WaitForElement("Bengal").GetRect();

		App.ScrollUp("GroupedCollectionView");
		App.ScrollUp("GroupedCollectionView");
		App.ScrollUp("GroupedCollectionView");

		App.Tap("ScrollToThirdCatButton");
		var bengalRect4 = App.WaitForElement("Bengal").GetRect();

		const int tolerance = 30;

		Assert.That(bengalRect2.Y, Is.EqualTo(baselineY).Within(tolerance),
			$"2nd scroll Y={bengalRect2.Y} should match baseline Y={baselineY}");

		Assert.That(bengalRect3.Y, Is.EqualTo(baselineY).Within(tolerance),
			$"3rd scroll Y={bengalRect3.Y} should match baseline Y={baselineY}");

		Assert.That(bengalRect4.Y, Is.EqualTo(baselineY).Within(tolerance),
			$"4th scroll Y={bengalRect4.Y} should match baseline Y={baselineY}");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void MakeVisibleScrollToGroupedItemMakesItemVisible()
	{
		App.WaitForElement("ScrollToThirdCatButton");

		App.Tap("ScrollToThirdCatButton");

		var bengalRect = App.WaitForElement("Bengal").GetRect();
		var cvRect = App.WaitForElement("GroupedCollectionView").GetRect();

		Assert.That(bengalRect.Y, Is.GreaterThanOrEqualTo(cvRect.Y),
			$"Bengal Y={bengalRect.Y} should be at or below CollectionView top Y={cvRect.Y}");

		Assert.That(bengalRect.Y + bengalRect.Height, Is.LessThanOrEqualTo(cvRect.Y + cvRect.Height + 1),
			$"Bengal bottom={bengalRect.Y + bengalRect.Height} should be within CollectionView bottom={cvRect.Y + cvRect.Height}");
	}
}
