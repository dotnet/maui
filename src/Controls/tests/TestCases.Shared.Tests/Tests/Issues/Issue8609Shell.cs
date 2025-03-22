using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8609Shell : _IssuesUITest
	{
		public Issue8609Shell(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Shell.AutoResizeTabBarIcons";

		[Test]
		[Category(UITestCategories.Shell)]
		public void TabBarIconsShouldAutoscale()
		{
			App.WaitForElement("Tab1");
			VerifyScreenshot();
		}
	}
}