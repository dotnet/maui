using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31351 : _IssuesUITest
{
	public Issue31351(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[WinUI] Custom CollectionView does not work on ScrollTo";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CustomCollectionViewShouldScroll()
	{
		App.WaitForElement("Issue31351CollectionView");
		App.WaitForElement("Item 50");
		App.WaitForElement("Issue31351TopScrollButton");
		App.Tap("Issue31351TopScrollButton");
		App.WaitForElement("Item 1");
		App.WaitForElement("Issue31351ScrollButton");
		App.Tap("Issue31351ScrollButton");
		App.WaitForElement("Item 50");
	}
}