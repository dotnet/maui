using Microsoft.Maui.AppiumTests;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Controls.AppiumTests.Tests.Issues
{
	public class Issue19673 : _IssuesUITest
	{
		public Issue19673(TestDevice device) : base(device) { }

		public override string Issue => "ToolbarItem icon/image doesn't update on page navigation on Android";

		[Test]
		public void ToolbarItemUpdateOnNavigation()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });

			// 1. Swipe to second page.
			App.WaitForElement("Page1");
			App.SwipeRightToLeft("Page1");

			// 2. Verify that the ToolbarItem is rendered.
			VerifyScreenshot();
		}
	}
}