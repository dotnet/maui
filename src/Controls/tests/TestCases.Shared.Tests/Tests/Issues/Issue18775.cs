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
			App.WaitForElement("MauiLabel");
			VerifyScreenshot();
		}
	}
}
