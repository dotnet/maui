#if ANDROID || IOS //This test case verifies "SetOrientationPotrait and Landscape works" exclusively on the Android and IOS platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
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
			try
			{
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
			}
			finally
			{
				App.SetOrientationPortrait();
			}
		}
	}
}
#endif