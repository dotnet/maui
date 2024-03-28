using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollChangeOrientationUITests : _IssuesUITest
	{
		public ScrollChangeOrientationUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "Horizontal ScrollView locks after rotation";

		// Issue773TestsRotationRelayoutIssue (src\Compatibility\ControlGallery\src\Issues.Shared\Issue773.cs)
		[Test]
		public void ScrollRotationRelayoutIssue()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows });

			App.WaitForElement("Button1");

			App.SetOrientationLandscape();

			var buttonAutomationIds = new[]
			{
					"Button1",
					"Button2",
					"Button3",
				};

			foreach (string buttonAutomationId in buttonAutomationIds)
				App.WaitForElement(buttonAutomationId);

			App.Screenshot("StackLayout respects rotation");

			App.SetOrientationPortrait();
		}
	}
}