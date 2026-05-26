using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23702 : _IssuesUITest
{
	public Issue23702(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView with GridItemsLayout (Span=1) doesn't adapt to window width reduction on Windows platform";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewItemsShouldResizeWhenWidthDecreases()
	{
		App.WaitForElement("SetWidthButton");
		App.Tap("SetWidthButton");
		var widthText = App.WaitForElement("WidthLabel").GetText();
		var actualWidth = int.Parse(widthText ?? "0");

		Assert.That(actualWidth, Is.InRange(80, 100),
			$"Final width should be around 86px but was {actualWidth}");
	}
}
