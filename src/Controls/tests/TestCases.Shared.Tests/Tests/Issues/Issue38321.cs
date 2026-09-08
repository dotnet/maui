using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue38321 : _IssuesUITest
	{
		public Issue38321(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Grouped CollectionView with GridItemsLayout throws ArgumentOutOfRangeException after an item is removed";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void GroupedCollectionViewRemoveItem()
		{
			App.WaitForElement("RemoveItemButton");

			App.Tap("RemoveItemButton");
			App.WaitForElement("RemoveItemButton");

			App.Tap("RemoveItemButton");
			App.WaitForElement("RemoveItemButton");

			App.Tap("RemoveItemButton");
			App.WaitForElement("RemoveItemButton");
		}
	}
}