using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue773 : _IssuesUITest
	{
		public Issue773(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Horizontal ScrollView locks after rotation";

		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		public void Issue773TestsRotationRelayoutIssue()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.SetOrientationLandscape();

			var buttonLabels = new[] {
				"Button1",
				"Button2",
				"Button3",
			};

			foreach (string buttonLabel in buttonLabels)
				App.WaitForElement(buttonLabel);

			App.Screenshot("StackLayout in Modal respects rotation");

			App.SetOrientationPortrait();
		}
	}
}
