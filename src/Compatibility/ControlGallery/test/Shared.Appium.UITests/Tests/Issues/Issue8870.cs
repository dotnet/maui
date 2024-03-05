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

			App.WaitForElement(CheckResult);

			App.SetOrientationPortrait();
			await Task.Delay(delay);

			App.SetOrientationLandscape();
			await Task.Delay(delay);

			App.SetOrientationPortrait();
			await Task.Delay(delay);

			App.SetOrientationLandscape();
			await Task.Delay(delay);

			App.SetOrientationPortrait();
			await Task.Delay(delay);

			App.WaitForElement(CheckResult);
			App.Click(CheckResult);

			App.WaitForNoElement(Success);
		}
	}
}
