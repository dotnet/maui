using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue13551 : IssuesUITest
	{
		const string Success1 = "Success1";
		const string Success2 = "Success2";

		public Issue13551(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [iOS] CollectionView does not display items if `IsVisible` modified via a binding/trigger";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewWithFooterShouldNotCrashOnDisplay()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForNoElement(Success1);
			App.WaitForNoElement(Success2);
		}
	}
}