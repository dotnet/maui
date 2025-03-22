using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8609Tabbed : _IssuesUITest
	{
		public Issue8609Tabbed(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "TabbedPage.AutoResizeIconsProperty";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void TabBarIconsShouldAutoscale()
		{
			App.WaitForElement("Tab1");
			VerifyScreenshot();
		}
	}
}