using NUnit.Framework;
using OpenQA.Selenium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue7823 : _IssuesUITest
	{
		public Issue7823(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "In a ToolbarItems, if an item has no icon but just text, MAUI uses the icon from the previous page in the Navigation";

		[Test]
		public async Task UpdateToolbarItemAfterNavigate()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });

			// 1. Navigate from Page with a TollbarItem using an Icon. 
			App.WaitForElement("WaitForStubControl");
			App.Click("WaitForStubControl");

			await Task.Delay(1000); // Wait navigation animation to complete

			// 2. Verify that the second page with a TollbarItem without an icon does not show the icon of the previous page.
			VerifyScreenshot();
			App.Back();
		}
	}
}
