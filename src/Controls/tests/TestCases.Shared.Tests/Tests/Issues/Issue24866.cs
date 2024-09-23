using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24866 : _IssuesUITest
	{
		public Issue24866(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Windows] CollectionView with grouping fails to add items when a footer template is present or crashes when removing data.";
		
		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnMac]
		public void CollectionViewShouldAddItemsWithFooterWhenGrouped()
		{
			App.WaitForElement("CollectionView");
			App.Tap("add");
			VerifyScreenshot();

		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnMac]
		public void CollectionViewShouldClearAndAddItemsOnGrouping()
		{
			App.WaitForElement("CollectionView");
			App.Tap("add");
			App.Tap("clear");
			VerifyScreenshot();
		}
	}
}
