using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue16321 : _IssuesUITest
	{
		public Issue16321(TestDevice device) : base(device) { }

		public override string Issue => "Alerts Open on top of current presented view";

		[Test]
		public void OpenAlertWithModals()
		{
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac, TestDevice.Windows, TestDevice.Android
			});

			App.Tap("OpenAlertWithModals");
			App.Tap("Cancel");
		}

		[Test]
		public void OpenAlertWithNewUIWindow()
		{
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac, TestDevice.Windows, TestDevice.Android
			});

			App.Tap("OpenAlertWithNewUIWindow");
			App.Tap("Cancel");
		}
	}
}
