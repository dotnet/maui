using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue773 : IssuesUITest
	{
		public Issue773(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Horizontal ScrollView locks after rotation"; 
		
		[Test]
		[Category(UITestCategories.ScrollView)]
		[FailsOnAndroid]
		public void Issue773TestsRotationRelayoutIssue()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.SetOrientationLandscape();

			var buttonLabels = new[] {
				"Button1",
				"Button2",
				"Button3",
			};

			foreach (string buttonLabel in buttonLabels)
				RunningApp.WaitForElement(buttonLabel);

			RunningApp.Screenshot("StackLayout in Modal respects rotation");

			RunningApp.SetOrientationPortrait();
		}
	}
}
