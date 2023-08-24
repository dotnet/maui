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
			App.Tap("OpenAlertWithModals");
			App.Tap("Cancel");
		}

		[Test]
		public void OpenAlertWithNewUIWindow()
		{
			App.Tap("OpenAlertWithNewUIWindow");
			App.Tap("Cancel");
		}
	}
}
