#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16321 : _IssuesUITest
	{
		public Issue16321(TestDevice device) : base(device) { }

		public override string Issue => "Alerts Open on top of current presented view";

		[Test, Retry(2)]
		[TestCase("OpenAlertWithModals"), Retry(2), Retry(2)]
		[TestCase("OpenAlertWithNewUIWindow"), Retry(2), Retry(2)]
		[TestCase("OpenActionSheetWithNewUIWindow"), Retry(2), Retry(2)]
		[TestCase("OpenActionSheetWithModals"), Retry(2), Retry(2)]
		[Category(UITestCategories.DisplayAlert)]
		public void OpenAlertWithModals(string testCase)
		{
			App.WaitForElement(testCase).Tap();
			App.WaitForElement("Cancel").Tap();
		}
	}
}
#endif