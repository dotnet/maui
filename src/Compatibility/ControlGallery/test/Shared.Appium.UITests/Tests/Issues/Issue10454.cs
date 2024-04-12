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
		[FailsOnIOS]
		public void ChildAddedShouldFire()
		{
			RunningApp.WaitForNoElement(Success);
		}
	}
}