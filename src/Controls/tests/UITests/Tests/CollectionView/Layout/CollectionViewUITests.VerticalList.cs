using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewVerticalListUITests : CollectionViewUITests
	{
		public CollectionViewVerticalListUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("CollectionView using vertical list layout works")]
		public async Task CollectionViewVerticalList()
		{
			App.Click("VerticalList");
			App.WaitForElement("TestCollectionView");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// Wait for the collection to update
			await Task.Delay(1000);

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}
	}
}