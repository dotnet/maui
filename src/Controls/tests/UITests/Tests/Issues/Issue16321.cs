using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16321 : _IssuesUITest
	{
		public Issue16321(TestDevice device) : base(device) { }

		public override string Issue => "Alerts Open on top of current presented view";

		[Test]
		[TestCase("OpenAlertWithModals")]
		[TestCase("OpenAlertWithNewUIWindow")]
		[TestCase("OpenActionSheetWithNewUIWindow")]
		[TestCase("OpenActionSheetWithModals")]
		[Category(UITestCategories.DisplayAlert)]
		public void OpenAlertWithModals(string testCase)
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac, TestDevice.Windows, TestDevice.Android
			});

			App.WaitForElement(testCase).Tap();
			App.WaitForElement("Cancel").Tap();
		}
	}
}