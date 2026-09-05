using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7993 : _IssuesUITest
{
	public Issue7993(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] CollectionView.Scrolled event offset isn't correctly reset when items change";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewVerticalOffset()
	{
		App.WaitForElement("CollectionView7993");
		App.WaitForElement("VerticalOffset: 0");
		App.Tap("ScrollToEnd");
		App.WaitForElement("19");
		App.WaitForNoElement("VerticalOffset: 0");
		App.Tap("NewItemsSource");
		App.WaitForElement("VerticalOffset: 0");
	}
}