using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
    public  class Issue10454 : IssuesUITest
	{
		const string Success = "Success";

		public Issue10454(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView ChildAdded";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ChildAddedShouldFire()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement(Success);
		}
	}
}