using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue4484 : _IssuesUITest
	{
		public Issue4484(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] ImageButton inside NavigationView.TitleView throw exception during device rotation";
		public override bool ResetMainPage => false;

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			App.SetOrientationPortrait();
		}

		[Test]
		[Category(UITestCategories.ImageButton)]
		[Category(UITestCategories.Compatibility)]
		public void RotatingDeviceDoesntCrashTitleView()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("Instructions");
			App.SetOrientationLandscape();
			App.WaitForElement("Instructions");
			App.SetOrientationPortrait();
			App.WaitForElement("Instructions");
		}
	}
}