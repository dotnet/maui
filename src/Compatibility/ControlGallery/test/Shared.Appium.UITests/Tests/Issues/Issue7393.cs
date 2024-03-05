using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue7393 : IssuesUITest
	{
		const string Success = "Success";

		public Issue7393(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView problems and crashes with IsGrouped=\"true\"";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void AddingItemsToGroupedCollectionViewShouldNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(Success, timeout: TimeSpan.FromSeconds(30));
		}
	}
}