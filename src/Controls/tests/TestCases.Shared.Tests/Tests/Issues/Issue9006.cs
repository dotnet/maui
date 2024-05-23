/*
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue9006 : _IssuesUITest
	{
		public Issue9006(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Unable to open a new Page for the second time in Xamarin.Forms Shell Tabbar";

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.Compatibility)]
		public void ClickingOnTabToPopToRootDoesntBreakNavigation()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.Tap("Click Me");
			App.WaitForElement("FinalLabel");
			App.Tap("Tab1AutomationId");
			App.Tap("Click Me");
			App.WaitForNoElement("Success");
		}
	}
}
*/