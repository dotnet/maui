using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue8870 : IssuesUITest
	{
		const string Success = "Success";
		const string CheckResult = "Check";

		public Issue8870(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView with HTML Labels Freeze the Screen on Rotation";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public async Task RotatingCollectionViewWithHTMLShouldNotHangOrCrash()
		{
			int delay = 3000;

			RunningApp.WaitForElement(CheckResult);

			RunningApp.SetOrientationPortrait();
			await Task.Delay(delay);

			RunningApp.SetOrientationLandscape();
			await Task.Delay(delay);

			RunningApp.SetOrientationPortrait();
			await Task.Delay(delay);

			RunningApp.SetOrientationLandscape();
			await Task.Delay(delay);

			RunningApp.SetOrientationPortrait();
			await Task.Delay(delay);

			RunningApp.WaitForElement(CheckResult);
			RunningApp.Tap(CheckResult);

			RunningApp.WaitForNoElement(Success);
		}
	}
}
