using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewHorizontalGridUITests : CollectionViewUITests
	{
		public CollectionViewHorizontalGridUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("CollectionView using horizontal grid layout works")]
		public void CollectionViewHorizontalGrid()
		{
			App.Click("HorizontalGrid");
			App.WaitForElement("TestCollectionView");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}
	}
}