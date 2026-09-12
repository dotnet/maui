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
		public void GroupedCollectionViewRemovalsUseCurrentAdapterPositions()
		{
			App.WaitForElement("RemoveItemButton");

			string[] expectedResults =
			{
				"Removed first; items remaining: 7",
				"Removed middle; items remaining: 6",
				"Removed last; items remaining: 5"
			};

			for (int i = 0; i < expectedResults.Length; i++)
			{
				string expectedResult = expectedResults[i];
				App.Tap("RemoveItemButton");
				Assert.That(
					App.WaitForTextToBePresentInElement(
						"ItemsRemainingLabel",
						expectedResult),
					Is.True,
					$"The grouped CollectionView did not complete the removal: {expectedResult}.");
			}
		}
	}
}