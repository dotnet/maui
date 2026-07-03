using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18775 : _IssuesUITest
	{

		public Issue18775(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[regression/8.0.3] Cannot control unselected text color of tabs within TabbedPage";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void TabbedPageUnselectedBarTextColorConsistency()
		{
			if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			{
				Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/32125
			}
			App.WaitForElement("MauiLabel");
			VerifyScreenshot();
		}
	}
}
