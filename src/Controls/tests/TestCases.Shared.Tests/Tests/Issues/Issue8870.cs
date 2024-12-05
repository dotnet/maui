using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8870 : _IssuesUITest
	{
		const string Success = "Success";
		const string CheckResult = "Check";

		public Issue8870(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView with HTML Labels Freeze the Screen on Rotation";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest("SetOrientationPortrait method not implemented")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("SetOrientationPortrait method not implemented")]
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
			App.Tap(CheckResult);

			App.WaitForNoElement(Success);
		}
	}
}
